using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Packages;

/// <summary>FNV_APPROVAL_PKG: the approval steps of a purchased portfolio (approve, reject, return for correction).</summary>
public sealed class MtgApprovalPackage(ICapitraDbContext db, IClock clock)
{
    /// <summary>approve: records the decision and opens the next active step, or approves the portfolio after the last one.</summary>
    public async Task ApproveAsync(long approvalId, string user, string? comments, CancellationToken ct)
    {
        var a = await ApprovalAsync(approvalId, ct);
        var today = Today();
        a.DecisionCode = "APPROVE";
        a.DecisionDate = today;
        a.Comments = comments;
        var p = await PortfolioAsync(a.PortfolioId, ct);
        var next = await db.Set<MtgLkpApprovalStep>().Where(s => s.IsActive && s.SequenceNo > a.SequenceNo)
            .OrderBy(s => s.SequenceNo).FirstOrDefaultAsync(ct);
        if (next is not null)
        {
            db.Set<MtgPurchasedPortfolioApproval>().Add(new MtgPurchasedPortfolioApproval
            {
                PortfolioId = a.PortfolioId,
                StepCode = next.StepCode,
                SequenceNo = next.SequenceNo,
                AssignedDate = today,
            });
            p.CurrentApprovalStep = next.StepCode;
            p.PortfolioStatusCode = "PENDING_APPROVAL";
        }
        else
        {
            p.CurrentApprovalStep = null;
            p.PortfolioStatusCode = "APPROVED";
            p.ApprovedAt = clock.UtcNow;
            db.Set<MtgPurchasedPortfolioStatusHist>().Add(new MtgPurchasedPortfolioStatusHist
            {
                PortfolioId = a.PortfolioId,
                FromStatus = "PENDING_APPROVAL",
                ToStatus = "APPROVED",
                ChangeAt = clock.UtcNow,
                ChangedBy = user,
                Comments = "Final approval completed",
            });
        }

        await db.SaveChangesAsync(ct);
    }

    /// <summary>reject: records the decision and moves the portfolio to REJECTED (change_status).</summary>
    public async Task RejectAsync(long approvalId, string user, string? comments, CancellationToken ct)
    {
        var a = await ApprovalAsync(approvalId, ct);
        a.DecisionCode = "REJECT";
        a.DecisionDate = Today();
        a.Comments = comments;
        var p = await PortfolioAsync(a.PortfolioId, ct);
        db.Set<MtgPurchasedPortfolioStatusHist>().Add(new MtgPurchasedPortfolioStatusHist
        {
            PortfolioId = a.PortfolioId,
            FromStatus = p.PortfolioStatusCode,
            ToStatus = "REJECTED",
            ChangeAt = clock.UtcNow,
            ChangedBy = user,
            Comments = comments,
        });
        p.PortfolioStatusCode = "REJECTED";
        await db.SaveChangesAsync(ct);
    }

    /// <summary>return_for_correction: records the decision and opens the chosen step again (UNDER_REVIEW).</summary>
    public async Task ReturnForCorrectionAsync(long approvalId, string user, string returnToStepCode, string? comments, CancellationToken ct)
    {
        var a = await ApprovalAsync(approvalId, ct);
        var seq = await db.Set<MtgLkpApprovalStep>().Where(s => s.StepCode == returnToStepCode).Select(s => (int?)s.SequenceNo).FirstOrDefaultAsync(ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        var today = Today();
        a.DecisionCode = "RETURN";
        a.DecisionDate = today;
        a.Comments = comments;
        a.ReturnToStepCode = returnToStepCode;
        db.Set<MtgPurchasedPortfolioApproval>().Add(new MtgPurchasedPortfolioApproval
        {
            PortfolioId = a.PortfolioId,
            StepCode = returnToStepCode,
            SequenceNo = seq,
            AssignedDate = today,
            Comments = "Returned for correction",
        });
        var p = await PortfolioAsync(a.PortfolioId, ct);
        p.CurrentApprovalStep = returnToStepCode;
        p.PortfolioStatusCode = "UNDER_REVIEW";
        await db.SaveChangesAsync(ct);
    }

    private DateOnly Today() => DateOnly.FromDateTime(Pages.Custom.MtgAjax.CairoNow(clock));

    private async Task<MtgPurchasedPortfolioApproval> ApprovalAsync(long approvalId, CancellationToken ct) =>
        await db.Set<MtgPurchasedPortfolioApproval>().FirstOrDefaultAsync(x => x.ApprovalId == approvalId, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");

    private async Task<MtgPurchasedPortfolio> PortfolioAsync(long portfolioId, CancellationToken ct) =>
        await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
}
