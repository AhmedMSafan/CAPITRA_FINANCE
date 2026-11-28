using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Gl;
using Capitra.Application.Modules.Scr;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Crm.CfSuppliers;

/// <summary>
/// The business effect of CF_SUPPLIER_APPROVAL's steps, run inside the task's completion (so a failed check leaves the task open):
/// <list type="bullet">
/// <item>Any step, REJECT: the application is RETURNED to Sales and the run ends.</item>
/// <item>Any step, ACCEPT: the step's checks (<see cref="CrmCfSupplierRules.CheckAccept"/>) and segregation of duties.</item>
/// <item>Compliance ACCEPT: the customer's risk grade (Customer 360) is kept on the application and becomes the run's risk_rating,
/// which sends a HIGH risk supplier to the committee.</item>
/// <item>Contract ACCEPT: the supplier role becomes ACTIVE and the application ACTIVE (the agreement in force from its start date).</item>
/// </list>
/// </summary>
public sealed class CrmCfSupplierWorkflowHandler(ICapitraDbContext db, IClock clock, IScrUserProfiles profiles, CrmCustomer360Service customers)
    : IWfeTaskCompletionHandler
{
    public bool CanHandle(WfeTaskContext context) =>
        string.Equals(context.WorkflowCode, CrmCfSupplierRules.WorkflowCode, StringComparison.OrdinalIgnoreCase)
        && CrmCfSupplierRules.Steps.Contains(context.StepCode ?? "", StringComparer.OrdinalIgnoreCase);

    public async Task HandleAsync(WfeTaskContext context, CancellationToken cancellationToken)
    {
        var a = await db.Set<CrmCfSupplierApplication>().FirstOrDefaultAsync(x => x.WorkflowInstanceId == context.WorkflowInstanceId, cancellationToken)
            ?? throw new BusinessRuleException("The workflow's supplier application was not found.");
        if (a.StatusCode != CrmCfSupplierStatus.InWorkflow)
            throw new BusinessRuleException($"Supplier application {a.ApplicationRef} is {a.StatusCode.ToLowerInvariant().Replace('_', ' ')}, not in its workflow.");

        if (!string.Equals(context.Action, "ACCEPT", StringComparison.OrdinalIgnoreCase))
        {
            a.StatusCode = CrmCfSupplierStatus.Returned;
            a.ReturnReason = $"Returned at {StepName(context.StepCode)} by {context.User}.";
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        var profile = await profiles.ByUserNameAsync(context.User, cancellationToken);
        CrmCfSupplierRules.CheckPerson(a.EnteredByPerson, ScrDuties.PersonKey(profile?.EmployeeNo, context.User));

        var step = (context.StepCode ?? "").ToUpperInvariant();
        var parties = await db.Set<CrmClientParty>().AsNoTracking().Where(x => x.ClientId == a.ClientId).ToListAsync(cancellationToken);
        var documentTypes = await db.Set<CrmCfSupplierDocument>().AsNoTracking()
            .Where(x => x.ApplicationId == a.ApplicationId && x.IsActive && x.DocumentTypeCode != null).Select(x => x.DocumentTypeCode!).Distinct()
            .ToListAsync(cancellationToken);
        var bank = a.ClientBankAccountId is { } bankId
            ? await db.Set<CrmClientBankAccount>().AsNoTracking().Where(x => x.BankAccountId == bankId && x.ClientId == a.ClientId)
                .Select(x => x.StatusCode).FirstOrDefaultAsync(cancellationToken)
            : null;
        var client = await db.Set<CrmClient>().AsNoTracking().FirstAsync(x => x.ClientId == a.ClientId, cancellationToken);
        var today = GlTime.Today(clock);
        var compliance = step == CrmCfSupplierRules.Compliance ? (await customers.ComplianceBadgeAsync(a.ClientId, cancellationToken)).Code : null;
        var problems = CrmCfSupplierRules.CheckAccept(step, a, parties, documentTypes, bank,
            !string.IsNullOrWhiteSpace(client.RegistrationNumber) && !string.IsNullOrWhiteSpace(client.TaxNumber), today, compliance, client.RiskGrade);
        if (problems.Count > 0)
            throw new BusinessRuleException($"{StepName(step)} cannot be accepted: {string.Join(" ", problems)}");

        if (step == CrmCfSupplierRules.Compliance)
        {
            a.RiskRatingCode = client.RiskGrade;
            await SetVariableAsync(context.WorkflowInstanceId, "risk_rating", client.RiskGrade!, cancellationToken);
        }

        if (step == CrmCfSupplierRules.Contract)
        {
            var blockers = await customers.ActivationBlockersAsync(a.ClientId, CrmCustomer360Service.Supplier, cancellationToken);
            if (blockers.Count > 0)
                throw new BusinessRuleException($"The supplier cannot be activated: {string.Join(" ", blockers)}");
            var role = await db.Set<CrmClientRole>().FirstAsync(x => x.ClientRoleId == a.ClientRoleId, cancellationToken);
            role.StatusCode = CrmClientRole.Active;
            role.ActiveSinceDate ??= today;
            a.StatusCode = CrmCfSupplierStatus.Active;
            a.ActivatedAt = clock.UtcNow;
            a.ReturnReason = null;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Writes a run's data field, so the paths after it route on it whichever page completed the step.</summary>
    private async Task SetVariableAsync(long instanceId, string name, string value, CancellationToken cancellationToken)
    {
        var v = await db.Set<WfeInstanceVariable>().FirstOrDefaultAsync(x => x.WorkflowInstanceId == instanceId && x.VariableName == name, cancellationToken);
        if (v is null)
            db.Add(new WfeInstanceVariable { WorkflowInstanceId = instanceId, VariableName = name, VariableType = "String", VariableValue = value });
        else
            v.VariableValue = value;
    }

    public static string StepName(string? stepCode) => (stepCode ?? "").ToUpperInvariant() switch
    {
        CrmCfSupplierRules.DocumentCheck => "Document check",
        CrmCfSupplierRules.Compliance => "Compliance",
        CrmCfSupplierRules.SiteVisit => "Site visit",
        CrmCfSupplierRules.RiskReview => "Risk review",
        CrmCfSupplierRules.ManagerApproval => "Manager approval",
        CrmCfSupplierRules.CommitteeApproval => "Committee approval",
        CrmCfSupplierRules.Contract => "Contract signing",
        _ => stepCode ?? "the step",
    };
}
