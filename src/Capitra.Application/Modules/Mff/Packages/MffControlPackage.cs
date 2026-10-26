using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mff.Pages;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mff.Packages;

/// <summary>MFF_CONTROL_PKG: what an application still needs (MFF_VW_APPLICATION_READINESS) before it moves to a status.</summary>
public sealed class MffControlPackage(ICapitraDbContext db, IApexSqlReader reader, MffWorkflowPackage workflow, ICurrentUser user)
{
    // the statuses from which each part is required (static fields start in the order they are written)
    private static readonly string[] FromDisbursed = ["DISBURSED", "ACTIVE"];
    private static readonly string[] FromContractPending = ["CONTRACT_PENDING", .. FromDisbursed];
    private static readonly string[] FromSubsidyPending = ["SUBSIDY_PENDING", .. FromContractPending];
    private static readonly string[] FromOffered = ["OFFERED", .. FromSubsidyPending];
    private static readonly string[] FromCreditApproved = ["CREDIT_APPROVED", "CONDITIONAL_APPROVAL", .. FromOffered];
    private static readonly string[] FromCreditStudy = ["CREDIT_STUDY", .. FromCreditApproved];
    private static readonly string[] FromDocReviewed = ["DOC_REVIEWED", "REVIEW_APPROVED", .. FromCreditStudy];
    private static readonly string[] FromDocReview = ["UNDER_DOC_REVIEW", .. FromDocReviewed];

    /// <summary>readiness_pct: the application's readiness in percent, 0 when it is not found.</summary>
    public async Task<decimal> ReadinessPctAsync(long applicationId, CancellationToken ct) =>
        (await ReadinessAsync(applicationId, ct))?.Number("READINESS_PCT") ?? 0;

    /// <summary>readiness_message: what is missing for the status (SUMMARY: the master data only), separated by "; ", or READY.</summary>
    public async Task<string> ReadinessMessageAsync(long applicationId, string? toStatus, CancellationToken ct)
    {
        var r = await ReadinessAsync(applicationId, ct);
        if (r is null)
            return "Application not found";
        var target = (toStatus ?? "SUMMARY").ToUpperInvariant();
        bool Ready(string col) => r.Text(col) == "Y";
        decimal Count(string col) => r.Number(col) ?? 0;
        var failedChecks = (await db.Set<MffReviewCheck>().Where(c => c.ApplicationId == applicationId).Select(c => c.ResultCode).ToListAsync(ct))
            .Count(c => c.ToUpperInvariant() is "FAIL" or "REJECTED" or "BLOCK");

        var msg = new List<string>();
        if (!Ready("APPLICANT_READY"))
            msg.Add("Applicant master data is incomplete");
        if (FromCreditStudy.Contains(target))
        {
            if (!Ready("INCOME_READY"))
                msg.Add("No verified income source");
            if (!Ready("PROPERTY_READY"))
                msg.Add("No selected property");
        }

        if (FromDocReview.Contains(target) && Count("DOCUMENT_COUNT") == 0)
            msg.Add("No documents uploaded");
        if (FromDocReviewed.Contains(target))
        {
            if (Count("DOCUMENT_PENDING_COUNT") > 0)
                msg.Add("Documents still pending verification");
            if (Count("MISSING_REQUIRED_DOCUMENT_COUNT") > 0)
                msg.Add("Required documents are still missing or unverified");
            if (Count("DOCUMENT_REJECTED_COUNT") > 0)
                msg.Add("Rejected documents must be replaced or resolved");
            if (failedChecks > 0)
                msg.Add("Failed review checks must be resolved");
        }

        if (FromCreditStudy.Contains(target) && !Ready("CREDIT_INQUIRY_READY"))
            msg.Add("Accepted credit inquiry is required");
        if (FromCreditApproved.Contains(target) && !Ready("CREDIT_STUDY_READY"))
            msg.Add("Credit study is required");
        if (FromOffered.Contains(target) && !Ready("CREDIT_DECISION_READY"))
            msg.Add("Approved or conditional credit decision is required");
        if (FromOffered.Contains(target) && !Ready("CALCULATION_READY"))
            msg.Add("Completed mortgage calculation is required");
        if (FromSubsidyPending.Contains(target) && !Ready("OFFER_READY"))
            msg.Add("Issued or accepted finance offer is required");
        if (FromContractPending.Contains(target) && !Ready("SUBSIDY_READY"))
            msg.Add("Approved Fund subsidy is required");
        if (FromDisbursed.Contains(target) && !Ready("CONTRACT_READY"))
            msg.Add("Signed tripartite contract is required");
        if (target == "ACTIVE" && !Ready("LOAN_READY"))
            msg.Add("Disbursed loan account is required");
        return msg.Count == 0 ? "READY" : string.Join("; ", msg);
    }

    /// <summary>assert_ready: fails with what is missing unless the application is ready for the status.</summary>
    public async Task AssertReadyAsync(long applicationId, string toStatus, CancellationToken ct)
    {
        var msg = await ReadinessMessageAsync(applicationId, toStatus, ct);
        if (msg != "READY")
            throw new ApexProcessException("ORA-20041: Workflow readiness failed: " + msg[..Math.Min(1900, msg.Length)]);
    }

    /// <summary>change_status_checked: the status change of MFF_WORKFLOW_PKG after the readiness check.</summary>
    public async Task ChangeStatusCheckedAsync(long applicationId, string toStatus, string? roleCode, string? changedBy, string? remarks, CancellationToken ct)
    {
        var current = await db.Set<MffApplication>().Where(a => a.ApplicationId == applicationId).Select(a => a.StatusCode).FirstOrDefaultAsync(ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        var to = toStatus.ToUpperInvariant();
        if (current == to)
            return;
        await AssertReadyAsync(applicationId, to, ct);
        await workflow.ChangeStatusAsync(applicationId, to, roleCode?.ToUpperInvariant(), null, remarks, changedBy ?? user.UserName, ct);
    }

    private async Task<IReadOnlyDictionary<string, object?>?> ReadinessAsync(long applicationId, CancellationToken ct)
    {
        var rows = await reader.QueryAsync(MffApexPageSql.CONTROL_READINESS,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { ["APPLICATION_ID"] = applicationId.ToString(System.Globalization.CultureInfo.InvariantCulture) }, ct);
        return rows.Count > 0 ? rows[0] : null;
    }
}
