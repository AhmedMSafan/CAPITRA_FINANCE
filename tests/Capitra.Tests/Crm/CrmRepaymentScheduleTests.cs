using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Crm;
using Capitra.Domain.Modules.Fct;

namespace Capitra.Tests.Crm;

/// <summary>generate_repayment_schedule as ported: dates, interest and principal of each FCT_REPAYMENTS row.</summary>
public sealed class CrmRepaymentScheduleTests
{
    private static FctDisbursement Request(string principalMethod, string distribution, string interestMethod) => new()
    {
        DisbursementId = 1, DisbursementDate = new DateOnly(2026, 10, 30), RepaymentStartDate = new DateOnly(2026, 10, 30), TenorMonth = 3,
        IndexRate = 28.25m, SpreadRate = 3m, BaseDayCountBasis = "ACTUAL/360", PrincipalRepaymentScheduleType = "NORMAL_SCHEDULE",
        PrincipalRepaymentMethod = principalMethod, PrincipalDistributionMethod = distribution, InterestRepaymentMethod = interestMethod,
        ApprovedAdvanceAmount = 1_000_000m,
    };

    [Fact]
    public void At_maturity_with_interest_at_maturity_is_one_row()
    {
        var rows = CrmRepaymentSchedule.Build(Request("AT_MATURITY", "EQUAL_AMOUNT", "AT_MATURITY"), 0, [], [], 1);
        var row = Assert.Single(rows);
        Assert.Equal(new DateOnly(2027, 1, 30), row.DueDate);
        Assert.Equal(1_000_000m, row.PrincipalDue);
        // 1,000,000 x 31.25% x 92 days / 360
        Assert.Equal(79861.1111m, row.InterestDue);
    }

    [Fact]
    public void Monthly_interest_falls_on_month_ends_and_maturity()
    {
        var rows = CrmRepaymentSchedule.Build(Request("AT_MATURITY", "EQUAL_AMOUNT", "MONTHLY"), 0, [], [], 1);
        Assert.Equal(
            [new DateOnly(2026, 10, 31), new DateOnly(2026, 11, 30), new DateOnly(2026, 12, 31), new DateOnly(2027, 1, 30)],
            rows.Select(x => x.DueDate));
        Assert.All(rows.SkipLast(1), x => Assert.Equal(0m, x.PrincipalDue));
        Assert.Equal(1_000_000m, rows[^1].PrincipalDue);
        Assert.Equal(0m, rows[^1].ClosingPrincipal);
    }

    [Fact]
    public void Invoice_amounts_repay_on_invoice_due_dates_and_the_rest_at_maturity()
    {
        var invoices = new[] { new CrmScheduleSource(new DateOnly(2026, 12, 15), 400_000m), new CrmScheduleSource(new DateOnly(2027, 3, 1), 600_000m) };
        var rows = CrmRepaymentSchedule.Build(Request("INVOICE_DUE_DATE", "INVOICE_AMOUNT", "SAME_AS_PRINCIPAL"), 0, invoices, [], 1);
        Assert.Equal([new DateOnly(2026, 12, 15), new DateOnly(2027, 1, 30)], rows.Select(x => x.DueDate));
        Assert.Equal([400_000m, 600_000m], rows.Select(x => x.PrincipalDue!.Value));
    }

    [Fact]
    public void Tenor_is_required()
    {
        var d = Request("AT_MATURITY", "EQUAL_AMOUNT", "MONTHLY");
        d.TenorMonth = 0;
        Assert.Throws<BusinessRuleException>(() => CrmRepaymentSchedule.Build(d, 0, [], [], 1));
    }
}
