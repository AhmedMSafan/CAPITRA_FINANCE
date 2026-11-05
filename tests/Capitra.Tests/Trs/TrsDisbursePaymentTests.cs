using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Trs;
using Xunit;

namespace Capitra.Tests.Trs;

/// <summary>Treasury Disburse, Execute: which account pays each line, and the GL entries the rule gives.</summary>
public sealed class TrsDisbursePaymentTests
{
    private static readonly TrsPayingAccount NbeOp = new(1, 10, "NBE", "FINOVA-OP-EGP-NBE", "100", "OPERATIONAL", "EGP", "110102");
    private static readonly TrsPayingAccount NbeColl = new(2, 10, "NBE", "COLL-NBE-001", "101", "COLLECTION", "EGP", null);
    private static readonly TrsPayingAccount CibColl = new(3, 20, "CIB", "FINOVA-COLL-EGP-CIB", "200", "COLLECTION", "EGP", "110106");
    private static readonly TrsPayingAccount BmEscrow = new(4, 30, "Banque Misr", "FINOVA-ESC-EGP-BM", "300", "ESCROW", "EGP", "110107");
    private static readonly TrsPayingAccount[] Accounts = [NbeColl, NbeOp, CibColl, BmEscrow];

    [Fact]
    public void Bank_line_is_paid_from_our_account_at_that_bank()
    {
        Assert.Equal(NbeOp, TrsDisburseService.PickAccount(Accounts, 10));  // operating first
        Assert.Equal(CibColl, TrsDisburseService.PickAccount(Accounts, 20));
    }

    [Fact]
    public void Own_funds_are_paid_from_the_main_operating_account() =>
        Assert.Equal(NbeOp, TrsDisburseService.PickAccount(Accounts, null));

    [Fact]
    public void An_escrow_account_pays_only_when_the_bank_has_nothing_else()
    {
        var bmCurrent = BmEscrow with { AccountId = 5, AccountTypeCode = "COLLECTION" };
        Assert.Equal(BmEscrow, TrsDisburseService.PickAccount(Accounts, 30));
        Assert.Equal(bmCurrent, TrsDisburseService.PickAccount([.. Accounts, bmCurrent], 30));
    }

    [Fact]
    public void Rule_gives_the_drawdown_and_the_payment()
    {
        // net 12,000,000: Bank A (NBE) 2,000,000, Bank B (CIB) 9,000,000, own funds 1,000,000
        var plan = new ProductEventPlan(1, "TRS_DISBURSE", "FACTORING", "TRS_DISBURSE", [],
        [
            new(1, 10, null, null, "D", "BANK_DRAWDOWN", true, null, "Bank drawdown"),
            new(1, 20, null, null, "C", "BANK_DRAWDOWN", false, "BORR", "Bank borrowings"),
            new(2, 10, null, null, "D", "PAYMENT_AMOUNT", false, "RECV", "Factoring receivable"),
            new(2, 20, null, null, "C", "PAYMENT_AMOUNT", true, null, "Paid to the client"),
        ]);
        var parts = TrsDisburseService.PostingParts(
        [
            (true, 2_000_000m, "110102"),
            (true, 9_000_000m, "110106"),
            (false, 1_000_000m, "110102"),
        ]);
        // as ProductActionPosting adds the parts' lines together
        var lines = parts.SelectMany(p => ProductEventPosting.Build(plan, l => p.AmountOf(l.AmountCode) ?? 0, p.BankAccount))
            .GroupBy(x => (x.JournalNo, x.Account, Debit: x.Dr != 0))
            .Select(g => g.First() with { Dr = g.Sum(x => x.Dr), Cr = g.Sum(x => x.Cr) })
            .ToList();

        decimal Dr(int j, string a) => lines.Where(x => x.JournalNo == j && x.Account == a).Sum(x => x.Dr);
        decimal Cr(int j, string a) => lines.Where(x => x.JournalNo == j && x.Account == a).Sum(x => x.Cr);

        // journal 1: only the banks' money
        Assert.Equal(2_000_000m, Dr(1, "110102"));
        Assert.Equal(9_000_000m, Dr(1, "110106"));
        Assert.Equal(11_000_000m, Cr(1, "BORR"));
        // journal 2: the whole net amount
        Assert.Equal(12_000_000m, Dr(2, "RECV"));
        Assert.Equal(3_000_000m, Cr(2, "110102"));
        Assert.Equal(9_000_000m, Cr(2, "110106"));
        foreach (var j in lines.GroupBy(x => x.JournalNo))
            Assert.Equal(j.Sum(x => x.Dr), j.Sum(x => x.Cr));
    }
}
