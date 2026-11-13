using Capitra.Application.Modules.Lnd;
using Capitra.Domain.Modules.Lnd;
using Xunit;

namespace Capitra.Tests.Lnd;

/// <summary>Part prepayment and early settlement: the quote, the rebuilt schedule, and the instalments after.</summary>
public sealed class LndPrepaymentTests
{
    private static readonly DateOnly Day = new(2026, 2, 16);

    private static LndLoan Loan(string recalc = "REDUCE_INSTALMENT") => new()
    {
        LoanId = 1, StatusCode = "ACTIVE", DisbursementDate = new(2026, 1, 1), RepaymentFrequencyCode = "MONTHLY", InterestMethodCode = "DECLINING",
        AnnualRatePct = 12, InstallmentAmount = 1020.07m, IsPrepaymentAllowed = true, PrepaymentRecalcCode = recalc, PrepaymentFeePct = 1,
        EarlySettlementFeePct = 2, PaymentOrder = "LATE_FEE,CHARGES,INSURANCE,INTEREST,PRINCIPAL",
    };

    // 3,000 over 3 months at 12%: 1,020.07 a month; instalment 1 paid
    private static List<LndInstallment> Schedule(bool firstPaid = true)
    {
        var lines = LndSchedule.Build(3000, 3, "MONTHLY", "DECLINING", 12, new(2026, 2, 1));
        var list = lines.Select(l => new LndInstallment
        {
            InstallmentId = 100 + l.InstallmentNo, InstallmentNo = l.InstallmentNo, DueDate = l.DueDate, OpeningPrincipalAmount = l.OpeningPrincipal,
            PrincipalDueAmount = l.Principal, InterestDueAmount = l.Interest, ClosingPrincipalAmount = l.ClosingPrincipal, StatusCode = "DUE",
        }).ToList();
        if (firstPaid)
        {
            list[0].PrincipalPaidAmount = list[0].PrincipalDueAmount;
            list[0].InterestPaidAmount = list[0].InterestDueAmount;
            list[0].StatusCode = "PAID";
            list[0].PaidDate = new(2026, 2, 1);
        }
        return list;
    }

    [Fact]
    public void Interest_to_date_is_the_share_of_the_current_period()
    {
        var s = Schedule();
        var (current, start) = LndPrepayment.CurrentPeriod(s, new(2026, 1, 1), Day);
        Assert.Equal(2, current!.InstallmentNo);
        Assert.Equal(new DateOnly(2026, 2, 1), start);
        // 20.10 over 28 days, 15 days gone
        Assert.Equal(10.77m, LndPrepayment.InterestToDate(current, start, Day));
        current.InterestAccruedAmount = 12;
        Assert.Equal(12m, LndPrepayment.InterestToDate(current, start, Day));
    }

    [Fact]
    public void A_settlement_quote_is_dues_interest_to_date_principal_and_fee()
    {
        var q = LndPrepayment.Make(Loan(), Schedule(), LndPrepayment.Settle, null, Day);
        Assert.Null(q.Problem);
        Assert.Equal(0m, q.DuesAmount);
        Assert.Equal(2009.93m, q.PrincipalAmount);
        Assert.Equal(40.20m, q.FeeAmount);
        Assert.Equal(10.77m + 2009.93m + 40.20m, q.TotalAmount);

        var late = LndPrepayment.Make(Loan(), Schedule(firstPaid: false), LndPrepayment.Settle, null, Day);
        Assert.Equal(1020.07m, late.DuesAmount);
    }

    [Fact]
    public void A_part_prepayment_needs_nothing_due_and_less_than_all_principal()
    {
        Assert.NotNull(LndPrepayment.Make(Loan(), Schedule(firstPaid: false), LndPrepayment.Prepay, 500, Day).Problem);
        Assert.NotNull(LndPrepayment.Make(Loan(), Schedule(), LndPrepayment.Prepay, 2009.93m, Day).Problem);
        var noPrepay = Loan();
        noPrepay.IsPrepaymentAllowed = false;
        Assert.NotNull(LndPrepayment.Make(noPrepay, Schedule(), LndPrepayment.Prepay, 500, Day).Problem);
    }

    [Fact]
    public void Lower_instalment_keeps_the_count_and_the_first_interest_runs_from_today()
    {
        var q = LndPrepayment.Make(Loan(), Schedule(), LndPrepayment.Prepay, 1000, Day);
        Assert.Null(q.Problem);
        Assert.Equal(10m, q.FeeAmount);
        Assert.Equal(10.77m + 1000 + 10, q.TotalAmount);
        Assert.Equal(2, q.NewSchedule.Count);
        Assert.Equal(1009.93m, q.NewSchedule.Sum(x => x.Principal));
        // a full month on 1,009.93 is 10.10; 13 of 28 days are left
        Assert.Equal(4.69m, q.NewSchedule[0].Interest);
        Assert.True(q.NewSchedule[1].Total < 1020.07m);
    }

    [Fact]
    public void Shorter_term_keeps_the_instalment_and_drops_instalments()
    {
        var q = LndPrepayment.Make(Loan("REDUCE_TERM"), Schedule(), LndPrepayment.Prepay, 1000, Day);
        Assert.Single(q.NewSchedule);
        Assert.Equal(1009.93m, q.NewSchedule[0].Principal);
    }

    [Fact]
    public void A_prepayment_pays_the_current_instalment_and_rebuilds_the_rest()
    {
        var s = Schedule();
        var q = LndPrepayment.Make(Loan(), s, LndPrepayment.Prepay, 1000, Day);
        var lines = LndReceiptService.Prepay(s, q, Day);
        Assert.Equal(1010.77m, lines.Sum(x => x.Amount));
        Assert.Equal(1000m + q.NewSchedule[0].Principal, s[1].PrincipalDueAmount);
        Assert.Equal(1000m, s[1].PrincipalPaidAmount);
        Assert.Equal(10.77m + 4.69m, s[1].InterestDueAmount);
        Assert.Equal("PART_PAID", s[1].StatusCode);
        Assert.Equal(q.NewSchedule[1].Principal, s[2].PrincipalDueAmount);
        // the principal still adds up to the amount lent
        Assert.Equal(3000m, s.Sum(x => x.PrincipalDueAmount));
    }

    [Fact]
    public void A_settlement_pays_everything_and_later_interest_is_not_owed()
    {
        var s = Schedule(firstPaid: false);
        var loan = Loan();
        var q = LndPrepayment.Make(loan, s, LndPrepayment.Settle, null, Day);
        var lines = LndReceiptService.Settle(loan, s, q, Day);
        Assert.Equal(q.TotalAmount - q.FeeAmount, lines.Sum(x => x.Amount));
        Assert.All(s, i => Assert.Equal("PAID", i.StatusCode));
        Assert.Equal(10.77m, s[1].InterestDueAmount);
        Assert.Equal(0m, s[2].InterestDueAmount);
        Assert.Equal(3000m, s.Sum(x => x.PrincipalPaidAmount));
    }
}
