using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Crm;
using Capitra.Application.Modules.Scr;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Lnd;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Lnd;

/// <summary>
/// CF_FINANCE (Ahmed, 26 Nov 2026): a consumer finance bought from a supplier goes through five steps. Sales enters it (creating it
/// submits it), then Credit approval (CREDIT_OFFICER), Agreement and upfront payment (OPERATIONS_OFFICER), Delivery and customer
/// receipt (OPERATIONS_OFFICER) and Supplier payment (TREASURY_OFFICER). The person who entered the finance does none of the steps.
/// </summary>
public static class LndCfFinanceRules
{
    public const string WorkflowCode = "CF_FINANCE";

    public const string Credit = "CF_CREDIT";
    public const string Agreement = "CF_AGREEMENT";
    public const string Delivery = "CF_DELIVERY";
    public const string SupplierPayment = "CF_SUPPLIER_PAYMENT";

    public static readonly string[] Steps = [Credit, Agreement, Delivery, SupplierPayment];

    /// <summary>The steps built so far; the others are refused with a clear message until they come.</summary>
    public static readonly string[] Ready = [Credit];

    public static string StepName(string? stepCode) => (stepCode ?? "").ToUpperInvariant() switch
    {
        Credit => "Credit approval",
        Agreement => "Agreement and upfront payment",
        Delivery => "Delivery and customer receipt",
        SupplierPayment => "Supplier payment",
        _ => stepCode ?? "the step",
    };

    /// <summary>Segregation of duties: the person who entered the finance does none of its steps.</summary>
    public static void CheckPerson(string? enteredByPerson, string actorPerson)
    {
        if (enteredByPerson is not null && string.Equals(enteredByPerson, actorPerson, StringComparison.Ordinal))
            throw new BusinessRuleException("Segregation of duties: the person who entered the finance cannot do its workflow steps.");
    }

    /// <summary>
    /// Credit approval: the customer's compliance is clear (CLEAR, or DUE for review soon) and the supplier is still active and valid today.
    /// </summary>
    public static List<string> CheckCredit(string complianceCode, string? complianceLabel, bool supplierActive)
    {
        var p = new List<string>();
        if (complianceCode is not ("CLEAR" or "DUE"))
            p.Add($"The customer's compliance must be clear (now {(complianceLabel ?? complianceCode).ToLowerInvariant()}, Customer 360 > Compliance).");
        if (!supplierActive) p.Add("The supplier is no longer active, or its agreement is not valid today.");
        return p;
    }
}

/// <summary>The open step of a finance and whether the signed-in user holds its task.</summary>
public sealed record LndCfStepDto(string StepCode, string StepName, long? InboxId, bool IsMine);

public sealed record LndCfCompleteRequest(string? Action, string? Comment);

/// <summary>
/// The business effect of CF_FINANCE's steps, run inside the task's completion (a failed check leaves the task open):
/// REJECT rejects the finance; ACCEPT checks segregation of duties and the step's rules, then moves the finance on.
/// </summary>
public sealed class LndCfFinanceWorkflowHandler(ICapitraDbContext db, IClock clock, IScrUserProfiles profiles, CrmCustomer360Service customers)
    : IWfeTaskCompletionHandler
{
    public bool CanHandle(WfeTaskContext context) =>
        string.Equals(context.WorkflowCode, LndCfFinanceRules.WorkflowCode, StringComparison.OrdinalIgnoreCase)
        && LndCfFinanceRules.Steps.Contains(context.StepCode ?? "", StringComparer.OrdinalIgnoreCase);

    public async Task HandleAsync(WfeTaskContext context, CancellationToken cancellationToken)
    {
        var loan = await db.Set<LndLoan>().FirstOrDefaultAsync(x => x.WorkflowInstanceId == context.WorkflowInstanceId, cancellationToken)
            ?? throw new BusinessRuleException("The workflow's finance was not found.");
        var step = (context.StepCode ?? "").ToUpperInvariant();
        var name = LndCfFinanceRules.StepName(step);

        if (!string.Equals(context.Action, "ACCEPT", StringComparison.OrdinalIgnoreCase))
        {
            Move(loan, LndLoanService.Statuses.Rejected, $"Rejected at {name} by {context.User}.");
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        var profile = await profiles.ByUserNameAsync(context.User, cancellationToken);
        LndCfFinanceRules.CheckPerson(loan.EnteredByPerson, ScrDuties.PersonKey(profile?.EmployeeNo, context.User));
        if (!LndCfFinanceRules.Ready.Contains(step))
            throw new BusinessRuleException($"{name} is not available yet; it comes with the next update.");

        if (step == LndCfFinanceRules.Credit)
        {
            var compliance = await customers.ComplianceBadgeAsync(loan.ClientId, cancellationToken);
            var supplierActive = await LndLoanService.ActiveSupplierAgreements(db, LndLoanService.CairoToday(clock))
                .AnyAsync(x => x.ApplicationId == loan.SupplierApplicationId, cancellationToken);
            var problems = LndCfFinanceRules.CheckCredit(compliance.Code, compliance.Label, supplierActive);
            if (problems.Count > 0) throw new BusinessRuleException($"{name} cannot be accepted: {string.Join(" ", problems)}");
            Move(loan, LndLoanService.Statuses.Approved, $"Approved at {name} by {context.User}.");
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private void Move(LndLoan loan, string to, string reason)
    {
        db.Set<LndLoanStatusHist>().Add(new LndLoanStatusHist { LoanId = loan.LoanId, FromStatusCode = loan.StatusCode, ToStatusCode = to, Reason = reason });
        loan.StatusCode = to;
    }
}

/// <summary>The finance page's side of CF_FINANCE: the open step, and completing the signed-in user's task with a comment.</summary>
public sealed class LndCfFinanceService(ICapitraDbContext db, WfeInbox inbox)
{
    /// <summary>The step waiting now, or null when the finance is not in its workflow; IsMine when inboxId is the user's open task on it.</summary>
    public async Task<LndCfStepDto?> StepAsync(long loanId, long? inboxId, CancellationToken ct)
    {
        var loan = await db.Set<LndLoan>().AsNoTracking().FirstOrDefaultAsync(x => x.LoanId == loanId, ct)
            ?? throw new NotFoundException($"Finance {loanId} was not found.");
        if (loan.WorkflowInstanceId is not { } run) return null;
        var open = await db.Set<WfeStepInstance>().AsNoTracking()
            .Where(s => s.WorkflowInstanceId == run && s.StepStatus != WfeRuntime.Completed && s.StepStatus != WfeRuntime.Cancelled
                && s.StepCode != null && s.StepCode != "")
            .OrderByDescending(s => s.StepInstanceId).Select(s => s.StepCode).FirstOrDefaultAsync(ct);
        if (open is null) return null;
        var mine = false;
        if (inboxId is { } id)
        {
            try
            {
                var item = await inbox.GetAsync(id, ct);
                mine = item.InstanceId == run && item.Status != WfeRuntime.Completed;
            }
            catch (Exception ex) when (ex is NotFoundException or BusinessRuleException) { mine = false; }
        }
        return new LndCfStepDto(open, LndCfFinanceRules.StepName(open), mine ? inboxId : null, mine);
    }

    /// <summary>Completes the signed-in user's task: ACCEPT, or REJECT with the reason. The handler's checks run inside it.</summary>
    public async Task CompleteAsync(long loanId, long inboxId, LndCfCompleteRequest r, CancellationToken ct)
    {
        var action = (r.Action ?? "").Trim().ToUpperInvariant();
        if (action is not ("ACCEPT" or "REJECT")) throw BusinessValidationException.For("action", "Accept or reject the finance.");
        var comment = string.IsNullOrWhiteSpace(r.Comment) ? null : r.Comment.Trim();
        if (action == "REJECT" && comment is null) throw BusinessValidationException.For("comment", "Write why the finance is rejected.");
        if (comment is { Length: > 2000 }) throw BusinessValidationException.For("comment", "The comment is longer than 2000 characters.");

        var loan = await db.Set<LndLoan>().AsNoTracking().FirstOrDefaultAsync(x => x.LoanId == loanId, ct)
            ?? throw new NotFoundException($"Finance {loanId} was not found.");
        var item = await inbox.GetAsync(inboxId, ct); // throws when the task is not the user's
        if (loan.WorkflowInstanceId is null || item.InstanceId != loan.WorkflowInstanceId)
            throw new BusinessRuleException($"This task is not on finance {loan.LoanNumber}.");
        if (item.Status == WfeRuntime.Completed) throw new BusinessRuleException("This step is already completed.");

        await using var tx = await db.BeginTransactionAsync(ct);
        await inbox.CompleteAsync(inboxId, new CompleteInboxItemRequest(action, comment), ct);
        if (comment is not null)
        {
            // the step's comment goes with the status change it made
            var last = await db.Set<LndLoanStatusHist>().Where(x => x.LoanId == loanId).OrderByDescending(x => x.LoanStatusHistId).FirstOrDefaultAsync(ct);
            if (last is not null && last.Reason?.Contains(LndCfFinanceRules.StepName(item.StepCode), StringComparison.Ordinal) == true)
            {
                last.Reason = $"{last.Reason} {comment}";
                if (last.Reason.Length > 500) last.Reason = last.Reason[..500];
                await db.SaveChangesAsync(ct);
            }
        }
        await tx.CommitAsync(ct);
    }

    /// <summary>The variables a CF_FINANCE run starts with.</summary>
    public static Dictionary<string, string?> Variables(LndLoan loan) => new()
    {
        ["entity_id"] = loan.LoanId.ToString(CultureInfo.InvariantCulture),
        ["entity_ref"] = loan.LoanNumber,
        ["entity_type"] = "LND_LOAN",
        ["module_code"] = "LND",
    };
}
