using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Crm;
using Capitra.Domain.Modules.Fct;

namespace Capitra.Tests.Crm;

/// <summary>The repayment schedule from the terms (Ahmed, 2 Nov 2026): dates, interest and principal of each FCT_REPAYMENTS row.</summary>
public sealed class CrmRepaymentScheduleTests
{
    private static FctDisbursement Request(string principalMethod, string interestMethod, int? principalMonths = null, int? interestMonths = null) => new()
    {
        DisbursementId = 1, DisbursementDate = new DateOnly(2026, 10, 30), RepaymentStartDate = new DateOnly(2026, 10, 30), TenorMonth = 3,
        IndexRate = 28.25m, SpreadRate = 3m, BaseDayCountBasis = "ACTUAL/360", PrincipalRepaymentScheduleType = "NORMAL_SCHEDULE",
        PrincipalRepaymentMethod = principalMethod, PrincipalDistributionMethod = "EQUAL_AMOUNT", InterestRepaymentMethod = interestMethod,
        PrincipalFrequencyMonths = principalMonths, InterestFrequencyMonths = interestMonths, ApprovedAdvanceAmount = 1_000_000m,
    };

    [Fact]
    public void At_maturity_with_interest_at_maturity_is_one_row()
    {
        var rows = CrmRepaymentSchedule.Build(Request("AT_MATURITY", "AT_MATURITY"), 0, [], [], 1);
        var row = Assert.Single(rows);
        Assert.Equal(new DateOnly(2027, 1, 30), row.DueDate);
        Assert.Equal(1_000_000m, row.PrincipalDue);
        // 1,000,000 x 31.25% x 92 days / 360, in 2 decimals
        Assert.Equal(79861.11m, row.InterestDue);
        Assert.Equal(1_079_861.11m, row.TotalInstallment);
    }

    [Fact]
    public void End_of_month_actual_falls_on_the_real_month_ends_and_maturity()
    {
        var d = Request("AT_MATURITY", "END_OF_MONTH_ACTUAL");
        d.InterestFirstDueCode = "SAME_MONTH";
        var rows = CrmRepaymentSchedule.Build(d, 0, [], [], 1);
        Assert.Equal(
            [new DateOnly(2026, 10, 31), new DateOnly(2026, 11, 30), new DateOnly(2026, 12, 31), new DateOnly(2027, 1, 30)],
            rows.Select(x => x.DueDate));
        Assert.All(rows.SkipLast(1), x => Assert.Equal(0m, x.PrincipalDue));
        Assert.Equal(1_000_000m, rows[^1].PrincipalDue);
        Assert.Equal(0m, rows[^1].ClosingPrincipal);
    }

    [Fact]
    public void End_of_month_starts_next_month_by_default() =>
        Assert.Equal(new DateOnly(2026, 11, 30), CrmRepaymentSchedule.Build(Request("AT_MATURITY", "END_OF_MONTH_ACTUAL"), 0, [], [], 1)[0].DueDate);

    [Fact]
    public void Older_monthly_interest_reads_as_end_of_month_actual() =>
        Assert.Equal(
            CrmRepaymentSchedule.Build(Request("AT_MATURITY", "END_OF_MONTH_ACTUAL"), 0, [], [], 1).Select(x => x.DueDate),
            CrmRepaymentSchedule.Build(Request("AT_MATURITY", "MONTHLY"), 0, [], [], 1).Select(x => x.DueDate));

    [Fact]
    public void End_of_month_30_is_the_30th_and_february_s_last_day()
    {
        var d = Request("AT_MATURITY", "END_OF_MONTH_30");
        d.DisbursementDate = new DateOnly(2027, 1, 15);
        d.InterestStartDate = new DateOnly(2027, 1, 15);
        d.InterestFirstDueCode = "SAME_MONTH";
        d.TenorMonth = 4;
        var rows = CrmRepaymentSchedule.Build(d, 0, [], [], 1);
        Assert.Equal(
            [new DateOnly(2027, 1, 30), new DateOnly(2027, 2, 28), new DateOnly(2027, 3, 30), new DateOnly(2027, 4, 30), new DateOnly(2027, 5, 15)],
            rows.Select(x => x.DueDate));
    }

    [Fact]
    public void Interest_every_n_months_counts_from_its_start_date()
    {
        var d = Request("AT_MATURITY", "EVERY_N_MONTHS", interestMonths: 1);
        d.InterestStartDate = new DateOnly(2026, 10, 15);
        var rows = CrmRepaymentSchedule.Build(d, 0, [], [], 1);
        Assert.Equal(
            [new DateOnly(2026, 11, 15), new DateOnly(2026, 12, 15), new DateOnly(2027, 1, 15), new DateOnly(2027, 1, 30)],
            rows.Select(x => x.DueDate));
    }

    [Fact]
    public void Principal_every_n_months_is_equal_and_the_rounding_is_on_the_last()
    {
        var d = Request("EVERY_N_MONTHS", "WITH_PRINCIPAL", principalMonths: 1);
        d.ApprovedAdvanceAmount = 1_000_000.01m;
        var rows = CrmRepaymentSchedule.Build(d, 0, [], [], 1);
        Assert.Equal([new DateOnly(2026, 11, 30), new DateOnly(2026, 12, 30), new DateOnly(2027, 1, 30)], rows.Select(x => x.DueDate));
        Assert.Equal([333_333.34m, 333_333.34m, 333_333.33m], rows.Select(x => x.PrincipalDue!.Value));
        Assert.All(rows, x => Assert.Equal(x.PrincipalDue + x.InterestDue, x.TotalInstallment));
    }

    [Fact]
    public void Invoice_amounts_repay_on_invoice_due_dates_and_the_rest_at_maturity()
    {
        var invoices = new[] { new CrmScheduleSource(new DateOnly(2026, 12, 15), 400_000m), new CrmScheduleSource(new DateOnly(2027, 3, 1), 600_000m) };
        var d = Request("INVOICE_DUE_DATE", "SAME_AS_PRINCIPAL");
        d.PrincipalDistributionMethod = "INVOICE_AMOUNT";
        var rows = CrmRepaymentSchedule.Build(d, 0, invoices, [], 1);
        Assert.Equal([new DateOnly(2026, 12, 15), new DateOnly(2027, 1, 30)], rows.Select(x => x.DueDate));
        Assert.Equal([400_000m, 600_000m], rows.Select(x => x.PrincipalDue!.Value));
    }

    [Fact]
    public void Custom_schedule_takes_the_entered_principal_and_ends_on_its_last_date()
    {
        var d = Request("AT_MATURITY", "WITH_PRINCIPAL");
        d.PrincipalRepaymentScheduleType = "CUSTOM_SCHEDULE";
        d.TenorMonth = null;
        var custom = new[] { new CrmScheduleSource(new DateOnly(2027, 5, 29), 600_000m), new CrmScheduleSource(new DateOnly(2026, 12, 15), 400_000m) };
        var rows = CrmRepaymentSchedule.Build(d, 0, [], custom, 1);
        Assert.Equal([new DateOnly(2026, 12, 15), new DateOnly(2027, 5, 29)], rows.Select(x => x.DueDate));
        Assert.Equal([400_000m, 600_000m], rows.Select(x => x.PrincipalDue!.Value));
        Assert.All(rows, x => Assert.True(x.InterestDue > 0));
        Assert.Equal(new DateOnly(2027, 5, 29), CrmRepaymentSchedule.Maturity(d, custom));
    }

    [Fact]
    public void Custom_schedule_must_total_the_amount_financed()
    {
        var d = Request("AT_MATURITY", "AT_MATURITY");
        d.PrincipalRepaymentScheduleType = "CUSTOM_SCHEDULE";
        var ex = Assert.Throws<BusinessRuleException>(() =>
            CrmRepaymentSchedule.Build(d, 0, [], [new CrmScheduleSource(new DateOnly(2027, 1, 1), 900_000m)], 1));
        Assert.Contains("1,000,000.00", ex.Message);
    }

    [Fact]
    public void Tenor_is_required()
    {
        var d = Request("AT_MATURITY", "END_OF_MONTH_ACTUAL");
        d.TenorMonth = 0;
        Assert.Throws<BusinessRuleException>(() => CrmRepaymentSchedule.Build(d, 0, [], [], 1));
    }

    [Fact]
    public void Cheque_based_principal_is_no_longer_available() =>
        Assert.Throws<BusinessRuleException>(() => CrmRepaymentSchedule.Build(Request("CHEQUE_DUE_DATE", "AT_MATURITY"), 0, [], [], 1));

    [Fact]
    public void First_due_is_the_same_or_the_next_month_end()
    {
        // 15 Apr, End of Month: Same Month gives 30 Apr; Next Month 31 May, a long first period
        var d = Request("AT_MATURITY", "END_OF_MONTH_ACTUAL");
        d.DisbursementDate = new DateOnly(2027, 4, 15);
        d.InterestStartDate = new DateOnly(2027, 4, 15);
        d.InterestFirstDueCode = "SAME_MONTH";
        Assert.Equal(new DateOnly(2027, 4, 30), CrmRepaymentSchedule.Build(d, 0, [], [], 1)[0].DueDate);
        d.InterestFirstDueCode = "NEXT_MONTH";
        var rows = CrmRepaymentSchedule.Build(d, 0, [], [], 1);
        Assert.Equal(new DateOnly(2027, 5, 31), rows[0].DueDate);
        Assert.Equal(46, rows[0].DayCount);
    }

    [Fact]
    public void Principal_can_fall_on_month_ends_too()
    {
        var d = Request("END_OF_MONTH_ACTUAL", "WITH_PRINCIPAL", principalMonths: 1);
        d.PrincipalFirstDueCode = "SAME_MONTH";
        var rows = CrmRepaymentSchedule.Build(d, 0, [], [], 1);
        Assert.Equal(
            [new DateOnly(2026, 10, 31), new DateOnly(2026, 11, 30), new DateOnly(2026, 12, 31), new DateOnly(2027, 1, 30)],
            rows.Select(x => x.DueDate));
        Assert.Equal(1_000_000m, rows.Sum(x => x.PrincipalDue!.Value));
    }
}
