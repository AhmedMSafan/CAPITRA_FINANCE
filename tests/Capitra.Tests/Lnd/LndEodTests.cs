using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Lnd;
using Capitra.Domain.Modules.Lnd;
using Xunit;

namespace Capitra.Tests.Lnd;

/// <summary>Retail Lending end of day: accrual, days late, late fees from day 1 after the grace days, non-accrual, cure, write-off.</summary>
public sealed class LndEodTests
{
    private static readonly DateOnly Paid = new(2026, 1, 1);

    private static LndLoan Loan(string feeType = "PCT_DAILY", decimal? rate = 0.1m, decimal? flat = null, int grace = 5) => new()
    {
        LoanId = 1, LoanNumber = "LN-2026-0000001", StatusCode = "ACTIVE", LicenceCode = "MF", DisbursementDate = Paid,
        LateFeeGraceDays = grace, LateFeeTypeCode = feeType, LateFeeRatePct = rate, LateFeeFlatAmount = flat, LateFeeBaseCode = "OVERDUE_INSTALMENT",
    };

    // two monthly instalments: 31 days to 1 Feb, 28 days to 1 Mar
    private static List<LndInstallment> Schedule() =>
    [
        new() { InstallmentId = 11, InstallmentNo = 1, DueDate = new(2026, 2, 1), PrincipalDueAmount = 900, InterestDueAmount = 100 },
        new() { InstallmentId = 12, InstallmentNo = 2, DueDate = new(2026, 3, 1), PrincipalDueAmount = 950, InterestDueAmount = 50 },
    ];

    [Fact]
    public void Interest_accrues_evenly_and_the_due_date_takes_the_rounding()
    {
        var loan = Loan();
        var s = Schedule();
        decimal total = 0;
        for (var d = Paid.AddDays(1); d <= new DateOnly(2026, 2, 1); d = d.AddDays(1))
            total += LndEod.Process(loan, s, d, 90, 180).InterestAccrued;
        Assert.Equal(100m, total);
        Assert.Equal(100m, s[0].InterestAccruedAmount);
        Assert.Equal(0m, s[1].InterestAccruedAmount);
        Assert.Equal(3.23m, LndEod.DailyInterest(Schedule(), Paid, new(2026, 1, 2)).Amount);
    }

    [Fact]
    public void Nothing_accrues_on_the_payout_day()
    {
        Assert.Null(LndEod.DailyInterest(Schedule(), Paid, Paid).Installment);
    }

    [Fact]
    public void Days_late_count_from_the_oldest_unpaid_instalment()
    {
        var s = Schedule();
        Assert.Equal(0, LndEod.DaysLate(s, new(2026, 2, 1)));
        Assert.Equal(10, LndEod.DaysLate(s, new(2026, 2, 11)));
        s[0].PrincipalPaidAmount = 900;
        s[0].InterestPaidAmount = 100;
        Assert.Equal(0, LndEod.DaysLate(s, new(2026, 2, 11)));
        Assert.Equal(2, LndEod.DaysLate(s, new(2026, 3, 3)));
    }

    [Fact]
    public void A_daily_late_fee_starts_after_the_grace_days_and_goes_back_to_day_one()
    {
        var loan = Loan();
        var s = Schedule();
        // within the grace days: nothing
        Assert.Equal((0m, 0), LndEod.LateFee(loan, s[0], new(2026, 2, 6)));
        // day 6: 0.1% of 1,000 = 1.00 a day, for all 6 days
        Assert.Equal((6m, 6), LndEod.LateFee(loan, s[0], new(2026, 2, 7)));
        s[0].LateFeeDays = 6;
        Assert.Equal((1m, 7), LndEod.LateFee(loan, s[0], new(2026, 2, 8)));
    }

    [Fact]
    public void A_fixed_late_fee_is_charged_once()
    {
        var loan = Loan("FLAT", null, 75);
        var s = Schedule();
        Assert.Equal((75m, 6), LndEod.LateFee(loan, s[0], new(2026, 2, 7)));
        s[0].LateFeeDays = 6;
        Assert.Equal(0m, LndEod.LateFee(loan, s[0], new(2026, 2, 20)).Fee);
    }

    [Fact]
    public void Non_accrual_moves_income_to_suspense_and_a_cure_brings_it_back()
    {
        var loan = Loan("NONE", null);
        var s = Schedule();
        for (var d = Paid.AddDays(1); d <= new DateOnly(2026, 2, 1); d = d.AddDays(1)) LndEod.Process(loan, s, d, 20, 180);
        var accruedBefore = LndEod.UnpaidAccrued(s);

        LndEod.Result r = null!;
        for (var d = new DateOnly(2026, 2, 2); d <= new DateOnly(2026, 3, 3); d = d.AddDays(1))
        {
            r = LndEod.Process(loan, s, d, 20, 180);
            if (r.WentNonAccrual) break;
        }
        Assert.True(r.WentNonAccrual);
        Assert.True(loan.IsNonAccrual);
        Assert.Equal(20, r.DaysLate);
        Assert.True(r.InterestToSuspense > accruedBefore);
        Assert.True(r.InterestSuspended > 0);
        Assert.Equal(0m, r.InterestAccrued);
        var suspense = loan.InterestSuspenseAmount;
        Assert.Equal(r.InterestToSuspense + r.InterestSuspended, suspense);

        // the customer pays the first instalment: still late? no, the second is not due yet, so the loan cures
        s[0].PrincipalPaidAmount = 900;
        s[0].InterestPaidAmount = 100;
        var cure = LndEod.Process(loan, s, new(2026, 2, 28), 20, 180);
        Assert.True(cure.Cured);
        Assert.False(loan.IsNonAccrual);
        Assert.Equal(suspense, cure.InterestFromSuspense);
        Assert.True(cure.InterestAccrued > 0);
        Assert.Equal(0m, loan.InterestSuspenseAmount);
    }

    [Fact]
    public void A_payment_while_in_non_accrual_releases_its_part_of_the_suspense()
    {
        var loan = Loan("NONE", null);
        var s = Schedule();
        for (var d = Paid.AddDays(1); d <= new DateOnly(2026, 3, 5); d = d.AddDays(1)) LndEod.Process(loan, s, d, 30, 180);
        Assert.True(loan.IsNonAccrual);
        Assert.Equal(150m, loan.InterestSuspenseAmount);
        s[0].InterestPaidAmount = 100; // instalment 1 is still late (its principal is unpaid)
        var r = LndEod.Process(loan, s, new(2026, 3, 6), 30, 180);
        Assert.False(r.Cured);
        Assert.Equal(100m, r.InterestFromSuspense);
        Assert.Equal(50m, loan.InterestSuspenseAmount);
    }

    [Fact]
    public void A_loan_late_beyond_the_write_off_days_is_proposed_once()
    {
        var loan = Loan("NONE", null);
        var s = Schedule();
        var proposed = 0;
        for (var d = Paid.AddDays(1); d <= new DateOnly(2026, 4, 1); d = d.AddDays(1))
            if (LndEod.Process(loan, s, d, 30, 45).NewlyProposedForWriteOff) proposed++;
        Assert.Equal(1, proposed);
        Assert.True(loan.IsWriteOffProposed);
    }

    [Fact]
    public void A_closed_loan_gives_back_its_suspense_and_accrues_what_is_left()
    {
        var loan = Loan("NONE", null);
        loan.StatusCode = "CLOSED";
        loan.IsNonAccrual = true;
        loan.InterestSuspenseAmount = 40;
        var s = Schedule();
        var r = LndEod.Process(loan, s, new(2026, 3, 2), 30, 180);
        Assert.Equal(150m, r.InterestAccrued);
        Assert.Equal(40m, r.InterestFromSuspense);
        Assert.True(r.Cured);
        Assert.Equal(0m, loan.InterestSuspenseAmount);
    }

    [Fact]
    public void The_next_day_follows_the_last_one_done_and_never_passes_today()
    {
        var today = new DateOnly(2026, 11, 12);
        Assert.Equal(today, LndEodService.NextDate(null, today));
        Assert.Equal(new DateOnly(2026, 11, 11), LndEodService.NextDate(new(2026, 11, 10), today));
        Assert.Null(LndEodService.NextDate(today, today));
    }

    private static ProductEventPlan Plan(IReadOnlyList<ProductEventGlLine> lines) => new(9, "LND_EOD", "LENDING", "LND_EOD", [], lines);

    [Fact]
    public void Gl_lines_come_from_the_rule_and_balance()
    {
        var plan = Plan(
        [
            new(1, 1, null, null, "D", "INTEREST_ACCRUED", false, "120501", "Interest receivable"),
            new(1, 2, null, null, "C", "INTEREST_ACCRUED", false, "410501", "Interest income"),
            new(1, 3, null, null, "D", "INTEREST_SUSPENDED", false, "120501", "Interest receivable"),
            new(1, 4, null, null, "C", "INTEREST_SUSPENDED", false, "220601", "Interest suspense"),
        ]);
        var amounts = LndEodService.AmountCodes.ToDictionary(c => c, _ => 0m);
        amounts["INTEREST_ACCRUED"] = 10;
        amounts["INTEREST_SUSPENDED"] = 4;
        var lines = LndEodService.Lines(plan, amounts);
        Assert.Equal(14m, lines.Single(x => x.Account == "120501").Dr);
        Assert.Equal(10m, lines.Single(x => x.Account == "410501").Cr);
        Assert.Equal(4m, lines.Single(x => x.Account == "220601").Cr);

        amounts["LATE_FEE_CHARGED"] = 1;
        Assert.Throws<BusinessRuleException>(() => LndEodService.Lines(plan, amounts));
    }
}
