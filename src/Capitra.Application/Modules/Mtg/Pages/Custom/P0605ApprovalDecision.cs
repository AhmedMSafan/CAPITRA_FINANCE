using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mtg.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>App 105 page 605 "Approval Decision", After Submit "Execute Decision" (APPROVE, REJECT, RETURN), then Close Dialog.</summary>
public sealed class P0605ExecuteDecision(ICapitraDbContext db, MtgApprovalPackage approvals, ICurrentUser user, IClock clock) : IApexProcess
{
    public int App => 105;

    public int Page => 605;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var req = request.Item("REQUEST");
        if (req is not ("APPROVE" or "REJECT" or "RETURN"))
            return new { ok = true };
        var me = user.UserName ?? string.Empty;
        var comments = request.Item("P605_COMMENTS");
        await using var tx = await db.BeginTransactionAsync(ct);
        try
        {
            var id = MtgAjax.Long(request.Item("P605_APPROVAL_ID")) ?? throw new ApexProcessException("ORA-01403: no data found");
            var a = await db.Set<MtgPurchasedPortfolioApproval>().FirstOrDefaultAsync(x => x.ApprovalId == id, ct)
                ?? throw new ApexProcessException("ORA-01403: no data found");
            if (a.AssignedTo is not null && !string.Equals(a.AssignedTo, me, StringComparison.OrdinalIgnoreCase))
                throw new ApexProcessException("Task is assigned to another user");
            a.AssignedTo ??= me;
            a.AssignedDate ??= DateOnly.FromDateTime(MtgAjax.CairoNow(clock));
            if (request.Item("P605_STEP_CODE") == "BOARD")
            {
                var pid = MtgAjax.Long(request.Item("P605_PORTFOLIO_ID"));
                var p = await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == pid, ct);
                if (p is not null)
                {
                    var date = MtgAjax.Trim(request.Item("P605_BOARD_DECISION_DATE"));
                    p.BoardDecisionNo = request.Item("P605_BOARD_DECISION_NO");
                    p.BoardDecisionDate = date is null ? null
                        : DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d
                        : throw new ApexProcessException("ORA-01861: literal does not match format string");
                    p.BoardDecision = req switch { "APPROVE" => "APPROVE", "REJECT" => "REJECT", _ => request.Item("P605_BOARD_DECISION") };
                }
            }

            await db.SaveChangesAsync(ct);
            if (req == "APPROVE")
            {
                await approvals.ApproveAsync(id, me, comments, ct);
            }
            else if (req == "REJECT")
            {
                if (string.IsNullOrWhiteSpace(comments))
                    throw new ApexProcessException("Comments are required for rejection");
                await approvals.RejectAsync(id, me, comments, ct);
            }
            else
            {
                var step = request.Item("P605_RETURN_TO_STEP_CODE");
                if (string.IsNullOrEmpty(step) || string.IsNullOrWhiteSpace(comments))
                    throw new ApexProcessException("Return step and comments are required");
                await approvals.ReturnForCorrectionAsync(id, me, step, comments, ct);
            }

            await tx.CommitAsync(ct);
            return new { ok = true };
        }
        catch (ApexProcessException e)
        {
            await tx.RollbackAsync(ct);
            db.DiscardChanges();
            return new { ok = false, message = e.Message };
        }
    }
}
