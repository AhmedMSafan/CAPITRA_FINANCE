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
        v.LateFeeTypeCode = "PCT_DAILY";
        v.LateFeeRatePct = null;
        v.PaymentOrder = "INTEREST,PRINCIPAL,INTEREST,CHARGES,LATE_FEE";
        var e = LndProductTerms.Check(v);
        Assert.Equal(["lateFeeRatePct", "paymentOrder"], e.Keys.Order(StringComparer.Ordinal));
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
    public void Each_finance_is_held_to_the_licence_limit_per_customer()
    {
        Assert.NotNull(LndLoanService.LegalLimit(300_000, 292_000));
        Assert.Null(LndLoanService.LegalLimit(250_000, 292_000));
        Assert.NotNull(LndLoanService.LegalLimit(1_000, null)); // no approved limit: no finance
    }

    [Fact]
    public void A_change_lists_only_what_changed_and_the_first_terms_list_everything()
    {
        var before = Valid();
        before.AnnualRatePct = 22;
        var after = Valid();
        after.AnnualRatePct = 24;
        after.LateFeeTypeCode = "FLAT";
        after.LateFeeFlatAmount = 50;
        after.LateFeeGraceDays = 5;
        var changes = LndProductTerms.Diff(before, after);
        Assert.Equal(["Yearly rate", "Late fee", "Grace days"], changes.Select(x => x.Field));
        Assert.Equal(("22%", "24%"), (changes[0].OldValue, changes[0].NewValue));
        Assert.Equal(("—", "5"), (changes[2].OldValue, changes[2].NewValue)); // no late fee before: no grace days
        Assert.Equal(11, LndProductTerms.Diff(null, after).Count);
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
