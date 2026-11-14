using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Lnd;
using Capitra.Domain.Modules.Lnd;
using Xunit;

namespace Capitra.Tests.Lnd;

/// <summary>Retail Lending write-off: what comes off the books, and its GL lines.</summary>
public sealed class LndWriteOffTests
{
    private static LndLoan Loan() => new() { LoanId = 1, StatusCode = "ACTIVE", DisbursementDate = new(2026, 1, 1), InterestSuspenseAmount = 60, LateFeeSuspenseAmount = 5 };

    private static List<LndInstallment> Schedule() =>
    [
        new()
        {
            InstallmentNo = 1, DueDate = new(2026, 2, 1), PrincipalDueAmount = 900, PrincipalPaidAmount = 400, InterestDueAmount = 100, InterestAccruedAmount = 100,
            InterestPaidAmount = 100, ChargesDueAmount = 10, InsuranceDueAmount = 5, LateFeeDueAmount = 8,
        },
        new()
        {
            InstallmentNo = 2, DueDate = new(2026, 3, 1), PrincipalDueAmount = 950, InterestDueAmount = 50, InterestAccruedAmount = 50,
            ChargesDueAmount = 10, InsuranceDueAmount = 5,
        },
        new() { InstallmentNo = 3, DueDate = new(2026, 4, 1), PrincipalDueAmount = 1000, InterestDueAmount = 40, InterestAccruedAmount = 30, ChargesDueAmount = 10 },
    ];

    [Fact]
    public void A_write_off_takes_all_principal_and_what_was_accrued_or_fell_due()
    {
        var f = LndWriteOffService.Figures(Loan(), Schedule(), new(2026, 3, 20));
        Assert.Equal(2450m, f.PrincipalAmount);
        // 80 interest accrued and unpaid: 60 in suspense, 20 taken to income before non-accrual
        Assert.Equal(60m, f.InterestSuspenseAmount);
        Assert.Equal(20m, f.InterestAmount);
        Assert.Equal(5m, f.LateFeeSuspenseAmount);
        Assert.Equal(3m, f.LateFeeAmount);
        // charges and insurance of the two instalments due; the third's are not due yet
        Assert.Equal(20m, f.ChargesAmount);
        Assert.Equal(10m, f.InsuranceAmount);
        Assert.Equal(2450m + 80 + 8 + 20 + 10, f.TotalAmount);
        Assert.Equal(47, f.DaysLate);
    }

    [Fact]
    public void Suspense_never_exceeds_what_is_unpaid()
    {
        var loan = Loan();
        loan.InterestSuspenseAmount = 500;
        var f = LndWriteOffService.Figures(loan, Schedule(), new(2026, 3, 20));
        Assert.Equal(80m, f.InterestSuspenseAmount);
        Assert.Equal(0m, f.InterestAmount);
    }

    [Fact]
    public void Gl_lines_come_from_the_rule_and_balance()
    {
        var plan = new ProductEventPlan(9, "LND_WRITE_OFF", "LENDING", "LND_WRITE_OFF", [],
        [
            new(1, 1, null, null, "D", "PRINCIPAL_WRITTEN_OFF", false, "130901", "Loan loss provision"),
            new(1, 2, null, null, "C", "PRINCIPAL_WRITTEN_OFF", false, "120101", "Loans receivable"),
            new(1, 3, null, null, "D", "INTEREST_SUSPENSE_WRITTEN_OFF", false, "220601", "Interest suspense"),
            new(1, 4, null, null, "C", "INTEREST_SUSPENSE_WRITTEN_OFF", false, "120501", "Interest receivable"),
        ]);
        var amounts = LndWriteOffService.AmountCodes.ToDictionary(c => c, _ => 0m);
        amounts["PRINCIPAL_WRITTEN_OFF"] = 2450;
        amounts["INTEREST_SUSPENSE_WRITTEN_OFF"] = 60;
        var lines = LndGl.Lines(plan, amounts, LndWriteOffService.AmountCodes, "a write-off");
        Assert.Equal(2450m, lines.Single(x => x.Account == "120101").Cr);
        Assert.Equal(60m, lines.Single(x => x.Account == "120501").Cr);

        amounts["INTEREST_WRITTEN_OFF"] = 20;
        Assert.Throws<BusinessRuleException>(() => LndGl.Lines(plan, amounts, LndWriteOffService.AmountCodes, "a write-off"));
    }

    [Fact]
    public void Only_an_active_loan_can_be_written_off_and_only_once()
    {
        Assert.True(LndLoanService.CanMove("ACTIVE", "WRITTEN_OFF"));
        Assert.False(LndLoanService.CanMove("CLOSED", "WRITTEN_OFF"));
        Assert.False(LndLoanService.CanMove("WRITTEN_OFF", "ACTIVE"));
    }
}
