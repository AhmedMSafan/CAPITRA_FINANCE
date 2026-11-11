using Capitra.Application.Modules.Lnd;
using Capitra.Domain.Modules.Lnd;
using Xunit;
using S = Capitra.Application.Modules.Lnd.LndProductTerms.Statuses;

namespace Capitra.Tests.Lnd;

/// <summary>Administration > Product Setup > Lending Product Terms: the checks of a version and the version in force.</summary>
public sealed class LndProductTermsTests
{
    private static LndProductVersion Valid() => LndProductTerms.Defaults(7);

    [Fact]
    public void The_defaults_pass_and_use_the_agreed_payment_order()
    {
        Assert.Empty(LndProductTerms.Check(Valid()));
        Assert.Equal(["LATE_FEE", "CHARGES", "INSURANCE", "INTEREST", "PRINCIPAL"], LndProductTerms.Parts(Valid().PaymentOrder));
    }

    [Fact]
    public void Wrong_terms_are_reported_by_field()
    {
        var v = Valid();
        v.MinAmount = 5_000;
        v.MaxAmount = 1_000;
        v.MaxTermMonths = 2;
        v.LateFeeTypeCode = "PCT_DAILY";
        v.LateFeeRatePct = null;
        v.PaymentOrder = "INTEREST,PRINCIPAL,INTEREST,CHARGES,LATE_FEE";
        var e = LndProductTerms.Check(v);
        Assert.Equal(["lateFeeRatePct", "maxAmount", "maxTermMonths", "paymentOrder"], e.Keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void A_flat_late_fee_needs_its_amount()
    {
        var v = Valid();
        v.LateFeeTypeCode = "FLAT";
        Assert.Contains("lateFeeFlatAmount", LndProductTerms.Check(v).Keys);
        v.LateFeeFlatAmount = 50;
        Assert.Empty(LndProductTerms.Check(v));
    }

    [Fact]
    public void A_product_can_be_stricter_than_the_parameters_never_looser()
    {
        var v = Valid();
        v.MaxAmount = 300_000;
        Assert.Single(LndProductTerms.CheckAgainstParameters(v, LndParameterCatalog.Microfinance, 292_000, null, false));
        v.MaxAmount = 250_000;
        Assert.Empty(LndProductTerms.CheckAgainstParameters(v, LndParameterCatalog.Microfinance, 292_000, null, false));
        // no approved microfinance limit: the version cannot be approved
        Assert.Single(LndProductTerms.CheckAgainstParameters(v, LndParameterCatalog.Microfinance, null, null, false));
        // a consumer cash product is held to the cash limit; other consumer products are not
        Assert.Single(LndProductTerms.CheckAgainstParameters(v, LndParameterCatalog.ConsumerFinance, null, 50_000, true));
        Assert.Empty(LndProductTerms.CheckAgainstParameters(v, LndParameterCatalog.ConsumerFinance, null, null, false));
    }

    [Fact]
    public void The_version_in_force_is_the_latest_approved_one_that_has_started()
    {
        LndProductVersion V(int no, DateOnly from, string status) { var v = Valid(); v.VersionNo = no; v.EffectiveFromDate = from; v.StatusCode = status; return v; }
        List<LndProductVersion> versions =
        [
            V(1, new(2026, 11, 1), S.Approved), V(2, new(2026, 12, 1), S.Approved), V(3, new(2026, 11, 20), S.Pending), V(4, new(2026, 11, 15), S.Rejected),
        ];
        Assert.Equal(1, LndProductTerms.InForce(versions, 7, new(2026, 11, 30))!.VersionNo);
        Assert.Equal(2, LndProductTerms.InForce(versions, 7, new(2026, 12, 1))!.VersionNo);
        Assert.Null(LndProductTerms.InForce(versions, 7, new(2026, 10, 31)));
        Assert.Null(LndProductTerms.InForce(versions, 8, new(2026, 12, 1)));
    }
}
