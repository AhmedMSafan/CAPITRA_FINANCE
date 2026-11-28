using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Crm.CfSuppliers;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Crm;

/// <summary>
/// A role with where it stands: Detail ("Active since", "Onboarding since", "In approval", ...), Step (the open workflow step) and Since;
/// Tab is the Customer 360 tab that holds it. The screen translates and formats the parts.
/// </summary>
public sealed record CrmOverviewRoleDto(long ClientRoleId, string RoleLabel, string StatusCode, string Detail, string? Step, DateTime? Since, string Tab);

/// <summary>Work still running for the customer; Kind: SUPPLIER_APPLICATION, FACTORING_APPLICATION or FACTORING_AGREEMENT.</summary>
public sealed record CrmOverviewOpenDto(string Kind, string Label, string Reference, string Detail, string? Step, DateTime Since, string Tab, long? Id);

/// <summary>One event: Subject and Reference (e.g. "Supplier application" CFS-2026-001), what happened, and Detail (e.g. the step).</summary>
public sealed record CrmOverviewEventDto(DateTime At, string? By, string Subject, string? Reference, string Text, string? Detail = null);

public sealed record CrmOverviewWorkspaceDto(string Code, string Label);

public sealed record CrmCustomerOverviewDto(
    IReadOnlyList<CrmOverviewRoleDto> Roles, IReadOnlyList<CrmOverviewOpenDto> Open, IReadOnlyList<CrmOverviewEventDto> Activity,
    IReadOnlyList<CrmOverviewWorkspaceDto> Workspaces);

/// <summary>
/// Customer 360 > Overview (Ahmed, 26 Nov 2026): the customer's roles and where each stands, the work still running, the latest
/// activity, and links to the business-line workspaces the customer has. Read only, from data the modules already keep.
/// </summary>
public sealed class CrmCustomerOverviewService(ICapitraDbContext db)
{
    public const int ActivityCount = 20;

    /// <summary>Factoring applications and agreements in these statuses are finished, so they are not "open".</summary>
    internal static readonly string[] Finished = ["ACTIVE", "APPROVED", "CANCELLED", "REJECTED", "CLOSED", "EXPIRED", "CONVERTED", "REPLACED", "TERMINATED"];

    public async Task<CrmCustomerOverviewDto> GetAsync(long clientId, CancellationToken ct)
    {
        var c = await db.Set<CrmClient>().AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == clientId, ct)
            ?? throw new NotFoundException($"Customer {clientId} was not found.");
        var roles = await db.Set<CrmClientRole>().AsNoTracking().Where(x => x.ClientId == clientId).OrderBy(x => x.ClientRoleId).ToListAsync(ct);
        var apps = await db.Set<CrmCfSupplierApplication>().AsNoTracking()
            .Where(x => x.ClientId == clientId && x.StatusCode != CrmCfSupplierStatus.Withdrawn).OrderByDescending(x => x.ApplicationId).ToListAsync(ct);
        var appIds = apps.Select(x => x.ApplicationId).ToList();
        var appRefs = apps.Select(x => x.ApplicationRef).ToList();

        // the open step of each supplier application in approval, and when it started
        var runIds = apps.Where(x => x.WorkflowInstanceId != null).Select(x => x.WorkflowInstanceId!.Value).ToList();
        var openSteps = await db.Set<WfeStepInstance>().AsNoTracking()
            .Where(s => runIds.Contains(s.WorkflowInstanceId) && s.StepStatus != WfeRuntime.Completed && s.StepStatus != WfeRuntime.Cancelled
                && s.StepCode != null && s.StepCode != "")
            .Select(s => new { s.WorkflowInstanceId, s.StepCode, s.EnteredAt }).ToListAsync(ct);
        var openByRun = openSteps.GroupBy(s => s.WorkflowInstanceId).ToDictionary(g => g.Key, g => g.OrderBy(s => s.EnteredAt).Last());

        // ---- roles
        var roleRows = roles.Select(r =>
        {
            var app = apps.FirstOrDefault(a => a.ClientRoleId == r.ClientRoleId);
            var (detail, step, since) = r.StatusCode switch
            {
                CrmClientRole.Active => ("Active since", (string?)null, r.ActiveSinceDate?.ToDateTime(TimeOnly.MinValue) ?? r.UpdatedAt),
                CrmClientRole.Suspended => ("Suspended since", null, r.UpdatedAt),
                _ => ("Onboarding since", null, (DateTime?)r.CreatedAt),
            };
            if (CrmCustomer360Service.ByWorkflow(r) && app is not null && r.StatusCode == CrmClientRole.Onboarding)
                (detail, step, since) = (SupplierStatus(app.StatusCode), StepOf(app), null);
            var tab = CrmCustomer360Service.ByWorkflow(r) ? "cf-supplier" : "roles";
            return new CrmOverviewRoleDto(r.ClientRoleId, CrmCustomer360Service.RoleLabel(r), r.StatusCode, detail, step, since, tab);
        }).ToList();

        // ---- open now
        var open = new List<CrmOverviewOpenDto>();
        foreach (var a in apps.Where(a => a.StatusCode != CrmCfSupplierStatus.Active))
        {
            var step = openByRun.GetValueOrDefault(a.WorkflowInstanceId ?? 0);
            open.Add(new CrmOverviewOpenDto("SUPPLIER_APPLICATION", "Supplier application", a.ApplicationRef, SupplierStatus(a.StatusCode), StepOf(a),
                step?.EnteredAt ?? a.UpdatedAt ?? a.CreatedAt, "cf-supplier", a.ApplicationId));
        }
        var offers = await db.Set<FctOffer>().AsNoTracking().Where(x => x.ClientId == clientId)
            .Select(x => new { x.OfferId, x.OfferRef, x.StatusCode, x.CreatedAt, x.CreatedBy }).ToListAsync(ct);
        var agreements = await db.Set<FctAgreement>().AsNoTracking().Where(x => x.ClientId == clientId)
            .Select(x => new { x.AgreementId, x.AgreementNumber, x.StatusCode, x.CreatedAt, x.CreatedBy }).ToListAsync(ct);
        foreach (var o in offers.Where(o => !IsFinished(o.StatusCode)))
            open.Add(new CrmOverviewOpenDto("FACTORING_APPLICATION", "Factoring application", o.OfferRef, Label(o.StatusCode), null, o.CreatedAt, "factoring", o.OfferId));
        foreach (var g in agreements.Where(g => !IsFinished(g.StatusCode)))
            open.Add(new CrmOverviewOpenDto("FACTORING_AGREEMENT", "Factoring agreement", g.AgreementNumber, Label(g.StatusCode), null, g.CreatedAt, "factoring", g.AgreementId));

        // ---- latest activity
        var events = new List<CrmOverviewEventDto> { new(c.CreatedAt, c.CreatedBy, "Customer", null, "created") };
        events.AddRange(roles.Select(r => new CrmOverviewEventDto(r.CreatedAt, r.CreatedBy, "Role", null, "added", CrmCustomer360Service.RoleLabel(r))));
        events.AddRange((await db.Set<CrmClientComplianceHistory>().AsNoTracking().Where(x => x.ClientId == clientId)
                .OrderByDescending(x => x.ComplianceHistoryId).Take(ActivityCount).Select(x => new { x.ActionCode, x.CreatedAt, x.CreatedBy }).ToListAsync(ct))
            .Select(x => new CrmOverviewEventDto(x.CreatedAt, x.CreatedBy, "Compliance", null, Label(x.ActionCode))));
        events.AddRange((await db.Set<CrmClientBankAccountHistory>().AsNoTracking().Where(x => x.ClientId == clientId)
                .OrderByDescending(x => x.BankAccountHistoryId).Take(ActivityCount).Select(x => new { x.ActionCode, x.BankName, x.CreatedAt, x.CreatedBy }).ToListAsync(ct))
            .Select(x => new CrmOverviewEventDto(x.CreatedAt, x.CreatedBy, "Bank account", x.BankName, Label(x.ActionCode))));
        if (appIds.Count > 0)
        {
            var runs = await (
                from i in db.Set<WfeWorkflowInstance>().AsNoTracking()
                join w in db.Set<WfeWorkflowDefinition>().AsNoTracking() on i.WorkflowDefinitionId equals w.WorkflowDefinitionId
                where w.WorkflowCode == CrmCfSupplierRules.WorkflowCode && i.BusinessReference != null && appRefs.Contains(i.BusinessReference)
                select new { i.BusinessReference, i.StartedAt, i.StartedBy }).ToListAsync(ct);
            events.AddRange(runs.Select(r => new CrmOverviewEventDto(r.StartedAt, r.StartedBy, "Supplier application", r.BusinessReference, "submitted for approval")));
            var notes = await db.Set<CrmCfSupplierStepNote>().AsNoTracking().Where(n => appIds.Contains(n.ApplicationId))
                .Select(n => new { n.ApplicationId, n.ActionCode, n.StepName, n.DoneAt, n.DoneBy }).ToListAsync(ct);
            events.AddRange(notes.Select(n => new CrmOverviewEventDto(n.DoneAt, n.DoneBy, "Supplier application", RefOf(n.ApplicationId),
                n.ActionCode == "REJECT" ? "returned at" : "accepted at", n.StepName)));
            var docs = await db.Set<CrmCfSupplierDocument>().AsNoTracking().Where(d => appIds.Contains(d.ApplicationId) && d.IsActive)
                .Select(d => new { d.DocumentTitle, d.CreatedAt, d.CreatedBy }).ToListAsync(ct);
            events.AddRange(docs.Select(d => new CrmOverviewEventDto(d.CreatedAt, d.CreatedBy, "Document", d.DocumentTitle, "added")));
            events.AddRange(apps.Select(a => new CrmOverviewEventDto(a.CreatedAt, a.CreatedBy, "Supplier application", a.ApplicationRef, "created")));
            events.AddRange(apps.Where(a => a.ActivatedAt != null)
                .Select(a => new CrmOverviewEventDto(a.ActivatedAt!.Value, null, "Supplier application", a.ApplicationRef, "approved: the supplier is active")));
        }
        events.AddRange(offers.Select(o => new CrmOverviewEventDto(o.CreatedAt, o.CreatedBy, "Factoring application", o.OfferRef, "created")));
        events.AddRange(agreements.Select(g => new CrmOverviewEventDto(g.CreatedAt, g.CreatedBy, "Factoring agreement", g.AgreementNumber, "created")));
        events.AddRange((await db.Set<FctDisbursement>().AsNoTracking().Where(x => x.ClientId == clientId)
                .OrderByDescending(x => x.CreatedAt).Take(ActivityCount).Select(x => new { x.DisbursementNumber, x.CreatedAt, x.CreatedBy }).ToListAsync(ct))
            .Select(x => new CrmOverviewEventDto(x.CreatedAt, x.CreatedBy, "Disbursement", x.DisbursementNumber, "requested")));

        // ---- workspaces: only the business lines the customer has
        var workspaces = new List<CrmOverviewWorkspaceDto>();
        if (offers.Count > 0 || agreements.Count > 0 || roles.Any(r => r.BusinessLineCode == CrmCustomer360Service.Factoring || r.RoleCode == "DEBTOR"))
            workspaces.Add(new CrmOverviewWorkspaceDto("factoring", "Factoring Workspace"));
        if (roles.Any(CrmCustomer360Service.ByWorkflow))
            workspaces.Add(new CrmOverviewWorkspaceDto("cf-supplier", "Consumer Finance Supplier"));

        return new CrmCustomerOverviewDto(roleRows, [.. open.OrderByDescending(x => x.Since)],
            [.. events.OrderByDescending(x => x.At).Take(ActivityCount)], workspaces);

        string RefOf(long applicationId) => apps.FirstOrDefault(a => a.ApplicationId == applicationId)?.ApplicationRef ?? "";
        string? StepOf(CrmCfSupplierApplication a) =>
            a.StatusCode == CrmCfSupplierStatus.InWorkflow && openByRun.GetValueOrDefault(a.WorkflowInstanceId ?? 0) is { } s
                ? CrmCfSupplierWorkflowHandler.StepName(s.StepCode) : null;
    }

    internal static bool IsFinished(string? status) => status is null || Finished.Contains(status.ToUpperInvariant());

    /// <summary>Where a supplier application stands (its open step is given apart).</summary>
    internal static string SupplierStatus(string statusCode) => statusCode switch
    {
        CrmCfSupplierStatus.InWorkflow => "In approval",
        CrmCfSupplierStatus.Returned => "Returned to Sales",
        CrmCfSupplierStatus.Active => "Active",
        _ => "Draft: not submitted yet",
    };

    /// <summary>"IN_REVIEW" → "In review".</summary>
    internal static string Label(string? code)
    {
        var s = (code ?? "").Replace('_', ' ').Trim().ToLowerInvariant();
        return s.Length == 0 ? "" : char.ToUpperInvariant(s[0]) + s[1..];
    }
}
