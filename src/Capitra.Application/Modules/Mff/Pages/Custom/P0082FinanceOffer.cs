using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mff.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 82, After Submit "Save Finance Offer" (SAVE_OFFER).</summary>
public sealed class P0082FinanceOffer(ICapitraDbContext db, MffControlPackage control, ICurrentUser user) : IApexProcess
{
    public int App => 110;

    public int Page => 82;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") != "SAVE_OFFER")
            return Task.FromResult<object>(new { ok = true });
        return MffForm.RunAsync(db, async () =>
        {
            const string t = "MFF_FINANCE_OFFERS";
            var appId = Long(request, "P82_APPLICATION_ID");
            var id = Long(request, "P82_OFFER_ID");
            var status = Text(request, "P82_OFFER_STATUS");
            MffFinanceOffer? o;
            if (id is null)
            {
                o = new MffFinanceOffer { ApplicationId = Req(appId, t, "APPLICATION_ID") };
                db.Add(o);
            }
            else
            {
                o = await db.Set<MffFinanceOffer>().FirstOrDefaultAsync(x => x.OfferId == id, ct);
            }

            if (o is not null)
            {
                o.OfferNo = Req(Text(request, "P82_OFFER_NO"), t, "OFFER_NO");
                o.OfferDate = Req(Date(request, "P82_OFFER_DATE"), t, "OFFER_DATE");
                o.ExpiryDate = Date(request, "P82_EXPIRY_DATE");
                o.FinanceAmount = Number(request, "P82_FINANCE_AMOUNT");
                o.TermMonths = Int(request, "P82_TERM_MONTHS");
                o.AnnualRatePct = Number(request, "P82_ANNUAL_RATE_PCT");
                o.InitialInstallment = Number(request, "P82_INITIAL_INSTALLMENT");
                o.DownpaymentAmount = Number(request, "P82_DOWNPAYMENT_AMOUNT");
                o.SubsidyAmount = Number(request, "P82_SUBSIDY_AMOUNT");
                o.LtvPct = Number(request, "P82_LTV_PCT");
                o.OfferStatus = Req(status, t, "OFFER_STATUS");
                o.AcceptedDate = Date(request, "P82_ACCEPTED_DATE");
                o.Notes = Text(request, "P82_NOTES");
            }

            await db.SaveChangesAsync(ct);
            if (status is "ISSUED" or "ACCEPTED")
                await control.ChangeStatusCheckedAsync(appId ?? 0, "OFFERED", "OPERATIONS", user.UserName, "Finance offer issued", ct);
            return ("Finance offer saved.", o is null ? null : new Dictionary<string, string?> { ["P82_OFFER_ID"] = Id(o.OfferId) });
        }, ct);
    }
}
