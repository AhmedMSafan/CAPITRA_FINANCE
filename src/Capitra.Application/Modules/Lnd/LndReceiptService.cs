using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl;
using Capitra.Application.Modules.Gl.Journals;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Chq;
using Capitra.Domain.Modules.Col;
using Capitra.Domain.Modules.Lnd;
using Capitra.Domain.Modules.Trs;
using Microsoft.EntityFrameworkCore;
using St = Capitra.Application.Modules.Lnd.LndLoanService.Statuses;

namespace Capitra.Application.Modules.Lnd;

public sealed record LndReceiptSourceDto(string? SourceType, long? DepositAccountId, long? ChequeId, decimal? Amount, string? Reference);

public sealed record LndReceiptRequestDto(DateOnly? ValueDate, IReadOnlyList<LndReceiptSourceDto>? Sources, string? Notes);

public sealed record LndDepositAccountDto(long AccountId, string Label, bool HasGlAccount);

public sealed record LndCollectedChequeDto(long ChequeId, string ChequeRef, string ChequeNo, DateOnly DueDate, decimal ChequeAmount, string? DepositAccount);

public sealed record LndAllocationPreviewDto(int InstallmentNo, string PartCode, decimal Amount);

public sealed record LndReceiptPreviewDto(decimal ReceiptAmount, decimal AllocatedAmount, decimal CreditKeptAmount, decimal OverdueAmount, IReadOnlyList<LndAllocationPreviewDto> Lines);

public sealed record LndReceiptListDto(
    long RepaymentTxnId, string RepaymentNumber, DateOnly ValueDate, DateOnly TransactionDate, decimal ReceiptAmount, decimal AllocatedAmount,
    string StatusCode, string? GlTranSetId, string Sources, string CreatedBy);

public sealed record LndReceiptsPageDto(
    long LoanId, string LoanStatusCode, bool CanReceive, DateOnly Today, decimal OverdueToday, decimal CreditBalance,
    IReadOnlyList<LndDepositAccountDto> DepositAccounts, IReadOnlyList<LndCollectedChequeDto> Cheques, IReadOnlyList<LndReceiptListDto> Receipts,
    decimal? WrittenOffAmount = null, decimal? RecoveredAmount = null);

/// <summary>
/// Customer payments of a retail loan, through the shared Collection module (Ahmed, 11 Nov 2026: COL_ for every business line).
/// A receipt is a COL_REPAYMENT_TRANSACTIONS of the loan's business line with its payment sources, as in Capitra: a bank deposit into
/// one of our accounts, a collected cheque from the Cheque Wallet, or the customer's credit balance (money left on earlier lending
/// receipts). LndAllocator applies it in the loan's payment order; the rest stays as credit. GL posts the rule LND_RECEIPT of the
/// loan's product. The receipt, its sources, the allocations, the instalments, the loan event and GL succeed or fail together.
/// <para>
/// On a written-off loan the same receipt is a recovery: nothing is applied to the instalments (they are off the books); the money up
/// to what was written off and not yet recovered is recovery income, the rest stays as customer credit, and GL posts the product's
/// rule LND_RECOVERY (BANK_RECEIPT_AMOUNT, CREDIT_USED_AMOUNT, RECOVERY_AMOUNT, CREDIT_KEPT_AMOUNT). The loan stays WRITTEN_OFF.
/// </para>
/// </summary>
public sealed class LndReceiptService(ICapitraDbContext db, IClock clock, AdmProductEventRuleService rules,
    ProductActionPosting posting, LndLoanService loans)
{
    public const string RuleCode = "LND_RECEIPT";

    public const string RecoveryRuleCode = "LND_RECOVERY";

    public const long LegalEntityId = 1;

    public static readonly IReadOnlyList<string> Parts = ["LATE_FEE", "CHARGES", "INSURANCE", "INTEREST", "PRINCIPAL"];

    private static readonly string[] LendingLines = [LndProductTerms.BusinessLines.Microfinance, LndProductTerms.BusinessLines.ConsumerFinance];

    public async Task<LndReceiptsPageDto> GetAsync(long loanId, bool canReceive, CancellationToken cancellationToken)
    {
        var loan = await db.Set<LndLoan>().AsNoTracking().SingleOrDefaultAsync(x => x.LoanId == loanId, cancellationToken)
            ?? throw new NotFoundException($"Loan {loanId} was not found.");
        var today = GlTime.Today(clock);
        var owed = await OwedAsync(loanId, false, cancellationToken);
        var accounts = await (from a in db.Set<TrsBankAccount>().AsNoTracking()
                              join b in db.Set<TrsBank>().AsNoTracking() on a.BankId equals b.BankId
                              where a.IsActive && a.CurrencyCode == loan.CurrencyCode && a.AccountTypeCode != "ESCROW"
                              orderby b.BankNameEn, a.AccountRef
                              select new LndDepositAccountDto(a.AccountId, b.BankNameEn + " · " + a.AccountRef + " · " + a.AccountNo,
                                  !string.IsNullOrEmpty(a.ErpAccountCode))).ToListAsync(cancellationToken);
        var txnIds = await db.Set<LndLoanEvent>().AsNoTracking()
            .Where(x => x.LoanId == loanId && (x.EventCode == "RECEIPT" || x.EventCode == "RECOVERY") && x.SourceDocumentId != null).Select(x => x.SourceDocumentId!.Value).ToListAsync(cancellationToken);
        var txns = await db.Set<ColRepaymentTransaction>().AsNoTracking().Where(x => txnIds.Contains(x.RepaymentTxnId))
            .OrderByDescending(x => x.RepaymentTxnId).ToListAsync(cancellationToken);
        var sources = await db.Set<ColRepaymentSource>().AsNoTracking().Where(x => txnIds.Contains(x.RepaymentTxnId)).ToListAsync(cancellationToken);
        var writtenOff = loan.StatusCode == St.WrittenOff;
        var (woTotal, recovered) = writtenOff ? await RecoveryAsync(loanId, cancellationToken) : (0m, 0m);
        return new LndReceiptsPageDto(loanId, loan.StatusCode, canReceive && (loan.StatusCode == St.Active || writtenOff), today,
            writtenOff ? woTotal - recovered : owed.Where(x => x.DueDate <= today).Sum(x => x.Owed.Values.Sum()), await CreditAsync(loan.ClientId, cancellationToken),
            accounts, await ChequesAsync(loan.ClientId, cancellationToken),
            [.. txns.Select(t => new LndReceiptListDto(t.RepaymentTxnId, t.RepaymentNumber, t.ValueDate, t.TransactionDate, t.ReceiptAmount, t.AllocatedAmount,
                t.StatusCode, t.GlTranSetId,
                string.Join(", ", sources.Where(s => s.RepaymentTxnId == t.RepaymentTxnId).Select(s => $"{Label(s.SourceType)} {s.SourceAmount:#,0.00}")),
                t.CreatedBy))],
            writtenOff ? woTotal : null, writtenOff ? recovered : null);
    }

    /// <summary>What was written off on the loan (approved write-offs) and what was recovered since.</summary>
    public async Task<(decimal WrittenOff, decimal Recovered)> RecoveryAsync(long loanId, CancellationToken cancellationToken)
    {
        var writtenOff = await db.Set<LndWriteOff>().AsNoTracking().Where(x => x.LoanId == loanId && x.StatusCode == "APPROVED").SumAsync(x => x.TotalAmount, cancellationToken);
        var recovered = await db.Set<LndLoanEvent>().AsNoTracking().Where(x => x.LoanId == loanId && x.EventCode == "RECOVERY").SumAsync(x => x.EventAmount, cancellationToken);
        return (writtenOff, recovered);
    }

    /// <summary>A recovery on a written-off loan: up to what is still not recovered is income, the rest is kept as customer credit.</summary>
    public static (decimal Recovered, decimal Kept) SplitRecovery(decimal amount, decimal writtenOff, decimal recoveredBefore)
    {
        var recovered = Math.Min(amount, Math.Max(0, writtenOff - recoveredBefore));
        return (recovered, amount - recovered);
    }

    /// <summary>How a payment of this amount on the value date would be applied, without saving anything.</summary>
    public async Task<LndReceiptPreviewDto> PreviewAsync(long loanId, DateOnly? valueDate, decimal? amount, CancellationToken cancellationToken)
    {
        var loan = await db.Set<LndLoan>().AsNoTracking().SingleOrDefaultAsync(x => x.LoanId == loanId, cancellationToken)
            ?? throw new NotFoundException($"Loan {loanId} was not found.");
        var day = valueDate ?? GlTime.Today(clock);
        if (loan.StatusCode == St.WrittenOff)
        {
            var (wo, before) = await RecoveryAsync(loanId, cancellationToken);
            var (rec, kept) = SplitRecovery(Math.Max(0, amount ?? 0), wo, before);
            return new LndReceiptPreviewDto(amount ?? 0, rec, kept, wo - before, [new LndAllocationPreviewDto(0, "RECOVERY", rec)]);
        }
        var owed = await OwedAsync(loanId, false, cancellationToken);
        var (lines, left) = LndAllocator.Allocate(owed, LndProductTerms.Parts(loan.PaymentOrder), Math.Max(0, amount ?? 0), day);
        return new LndReceiptPreviewDto(amount ?? 0, lines.Sum(x => x.Amount), left, owed.Where(x => x.DueDate <= day).Sum(x => x.Owed.Values.Sum()),
            [.. lines.Select(x => new LndAllocationPreviewDto(x.InstallmentNo, x.PartCode, x.Amount))]);
    }

    public async Task<long> ReceiveAsync(long loanId, LndReceiptRequestDto dto, CancellationToken cancellationToken)
    {
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var loan = await db.Set<LndLoan>().SingleOrDefaultAsync(x => x.LoanId == loanId, cancellationToken)
            ?? throw new NotFoundException($"Loan {loanId} was not found.");
        var recovery = loan.StatusCode == St.WrittenOff;
        if (loan.StatusCode != St.Active && !recovery) throw new BusinessRuleException("Payments are received only on an active or written-off loan.");
        var today = GlTime.Today(clock);
        var valueDate = dto.ValueDate ?? today;
        if (valueDate > today) throw BusinessValidationException.For("valueDate", "The value date cannot be after today.");
        if (valueDate < loan.DisbursementDate) throw BusinessValidationException.For("valueDate", "The value date cannot be before the loan was paid out.");
        var product = await db.Set<AdmProduct>().AsNoTracking().SingleAsync(x => x.ProductId == loan.ProductId, cancellationToken);

        // ---- the sources, checked
        var sources = await CheckSourcesAsync(loan, dto.Sources ?? [], cancellationToken);
        var amount = sources.Sum(x => x.Amount);

        // ---- the receipt in the shared Collection module
        var txn = new ColRepaymentTransaction
        {
            BusinessLineCode = product.BusinessLineCode, RepaymentNumber = "PENDING-" + Guid.NewGuid().ToString("N").ToUpperInvariant(), ClientId = loan.ClientId,
            LegalEntityId = LegalEntityId, RepaymentMethod = sources.Select(x => x.Type).Distinct().Count() == 1 ? sources[0].Type : "MIXED",
            TransactionDate = today, ValueDate = valueDate, CurrencyCode = loan.CurrencyCode, ReceiptAmount = amount, StatusCode = "POSTED",
            ExternalReference = loan.LoanNumber, Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
        };
        db.Set<ColRepaymentTransaction>().Add(txn);
        await db.SaveChangesAsync(cancellationToken);
        txn.RepaymentNumber = $"RPT-{today:yyyyMM}-{txn.RepaymentTxnId:D8}";
        if (recovery)
        {
            await RecoverAsync(loan, product, txn, sources, amount, valueDate, today, cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return txn.RepaymentTxnId;
        }

        // ---- allocation in the loan's payment order
        var owed = await OwedAsync(loanId, true, cancellationToken);
        var (lines, left) = LndAllocator.Allocate(owed, LndProductTerms.Parts(loan.PaymentOrder), amount, valueDate);
        var installments = await db.Set<LndInstallment>().Where(x => x.LoanId == loanId).ToDictionaryAsync(x => x.InstallmentId, cancellationToken);
        foreach (var l in lines)
        {
            var i = installments[l.InstallmentId];
            switch (l.PartCode)
            {
                case "LATE_FEE": i.LateFeePaidAmount += l.Amount; break;
                case "CHARGES": i.ChargesPaidAmount += l.Amount; break;
                case "INSURANCE": i.InsurancePaidAmount += l.Amount; break;
                case "INTEREST": i.InterestPaidAmount += l.Amount; break;
                default: i.PrincipalPaidAmount += l.Amount; break;
            }
            db.Set<LndAllocation>().Add(new LndAllocation
            {
                RepaymentTxnId = txn.RepaymentTxnId, LoanId = loanId, InstallmentId = l.InstallmentId, PartCode = l.PartCode, AllocatedAmount = l.Amount, ValueDate = valueDate,
            });
        }
        foreach (var i in installments.Values.Where(x => lines.Any(l => l.InstallmentId == x.InstallmentId)))
        {
            var rest = Owed(i).Values.Sum();
            i.StatusCode = rest == 0 ? "PAID" : "PART_PAID";
            if (rest == 0) i.PaidDate = valueDate;
        }
        var allocated = amount - left;
        txn.AllocatedAmount = allocated;

        // ---- sources: credit is used first, then the new money, in the order entered
        var toAllocate = allocated;
        foreach (var s in sources.OrderBy(x => x.Type == "CREDIT_BALANCE" ? 0 : 1))
        {
            var used = Math.Min(s.Amount, toAllocate);
            toAllocate -= used;
            if (s.Type == "CREDIT_BALANCE")
            {
                await UseCreditAsync(txn, loan.ClientId, s.Amount, cancellationToken);
                // the credit used is not kept as credit again: it is all spent on the loan, or the receipt keeps it
                if (used < s.Amount) throw new BusinessRuleException("The credit balance used is more than the loan owes on the value date. Use less credit.");
                continue;
            }
            db.Set<ColRepaymentSource>().Add(new ColRepaymentSource
            {
                RepaymentTxnId = txn.RepaymentTxnId, SourceType = s.Type, ChequeId = s.ChequeId, SourceReference = s.Reference, SourceAmount = s.Amount, AllocatedAmount = used,
            });
        }

        // ---- GL
        var plan = await rules.PlanByCodeAsync(product.ProductCode, RuleCode, today, cancellationToken)
            ?? throw new BusinessRuleException($"Product {product.ProductCode} has no active rule {RuleCode}. Add it in Administration > Product Event Rules: "
                + "DR the bank account BANK_RECEIPT_AMOUNT, DR customer credit CREDIT_USED_AMOUNT, CR each part paid and CR customer credit CREDIT_KEPT_AMOUNT.");
        var paid = Parts.ToDictionary(p => p, p => lines.Where(l => l.PartCode == p).Sum(l => l.Amount));
        var banks = sources.Where(x => x.Type != "CREDIT_BALANCE").GroupBy(x => x.GlAccount!).Select(g => (g.Key, g.Sum(x => x.Amount))).ToList();
        var creditUsed = sources.Where(x => x.Type == "CREDIT_BALANCE").Sum(x => x.Amount);
        var glLines = Lines(plan, paid, banks, creditUsed, left);
        var doc = new ProductActionDocument("LND_RECEIPT", txn.RepaymentTxnId.ToString(CultureInfo.InvariantCulture), txn.RepaymentNumber, today, loan.CurrencyCode,
            $"Loan {loan.LoanNumber}: receipt {txn.RepaymentNumber} of {amount:#,0.00}, value date {valueDate:yyyy-MM-dd}");
        try
        {
            txn.GlTranSetId = await posting.PostLinesAsync(plan, glLines, string.IsNullOrWhiteSpace(product.GlTranSetCode) ? "LND" : product.GlTranSetCode.Trim().ToUpperInvariant(),
                "Loan receipt", "3100", product.BusinessLineCode, doc, cancellationToken);
            txn.IsGlPosted = true;
        }
        catch (ApexProcessException ex)
        {
            throw new BusinessRuleException($"GL posting failed, nothing was received: {GlPostingApi.CleanMessage(ex.Message)}");
        }

        db.Set<LndLoanEvent>().Add(new LndLoanEvent
        {
            LoanId = loanId, EventCode = "RECEIPT", EventDate = valueDate, EventAmount = amount, GlTranSetId = txn.GlTranSetId,
            SourceDocumentType = "COL_REPAYMENT", SourceDocumentId = txn.RepaymentTxnId,
            Notes = $"{txn.RepaymentNumber}: applied {allocated:#,0.00}" + (left > 0 ? $", kept as credit {left:#,0.00}." : "."),
        });
        await db.SaveChangesAsync(cancellationToken);
        if (installments.Values.All(x => x.StatusCode == "PAID"))
            await loans.ChangeStatusAsync(loanId, St.Closed, $"Fully repaid by {txn.RepaymentNumber}.", null, cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return txn.RepaymentTxnId;
    }

    /// <summary>
    /// The GL lines of a receipt from the rule's lines: BANK_RECEIPT_AMOUNT on the bank-account line, once for each bank account the
    /// money came into; CREDIT_USED_AMOUNT and CREDIT_KEPT_AMOUNT; each part paid (PRINCIPAL_PAID, INTEREST_PAID...). Lines with the same
    /// account and side in one journal are added together.
    /// </summary>
    public static IReadOnlyList<ProductEventPostingLine> Lines(
        ProductEventPlan plan, IReadOnlyDictionary<string, decimal> paid, IReadOnlyList<(string GlAccount, decimal Amount)> banks, decimal creditUsed, decimal creditKept,
        decimal? recovered = null)
    {
        var allowed = (recovered is null
            ? new[] { "BANK_RECEIPT_AMOUNT", "CREDIT_USED_AMOUNT", "CREDIT_KEPT_AMOUNT" }.Concat(Parts.Select(p => p + "_PAID"))
            : new[] { "BANK_RECEIPT_AMOUNT", "CREDIT_USED_AMOUNT", "CREDIT_KEPT_AMOUNT", "RECOVERY_AMOUNT" }).ToHashSet();
        var other = plan.Lines.Where(l => l.ChargeCode is not null || !allowed.Contains(l.AmountCode)).Select(l => l.ChargeCode ?? l.AmountCode).Distinct().ToList();
        if (other.Count > 0)
            throw new BusinessRuleException($"Rule {plan.RuleCode} posts {string.Join(", ", other)}; {(recovered is null ? "a loan receipt" : "a recovery")} gives only {string.Join(", ", allowed)}.");
        if (recovered > 0 && !plan.Lines.Any(l => l.AmountCode == "RECOVERY_AMOUNT"))
            throw new BusinessRuleException($"Rule {plan.RuleCode} has no line for RECOVERY_AMOUNT (CR recovery income).");
        if (banks.Count > 0 && !plan.Lines.Any(l => l.AmountCode == "BANK_RECEIPT_AMOUNT" && l.FromBankAccount))
            throw new BusinessRuleException($"Rule {plan.RuleCode} has no line taking BANK_RECEIPT_AMOUNT into the bank account.");

        var result = new List<ProductEventPostingLine>();
        void Add(int journal, string account, bool debit, decimal amount, string description)
        {
            if (amount == 0) return;
            var at = result.FindIndex(x => x.JournalNo == journal && x.Account == account && (x.Dr != 0) == debit);
            if (at >= 0) result[at] = result[at] with { Dr = result[at].Dr + (debit ? amount : 0), Cr = result[at].Cr + (debit ? 0 : amount) };
            else result.Add(new ProductEventPostingLine(journal, account, debit ? amount : 0, debit ? 0 : amount, description));
        }
        foreach (var l in plan.Lines.OrderBy(x => x.JournalNo).ThenBy(x => x.LineNo))
        {
            var debit = l.DebitCreditCode == "D";
            var description = l.Description ?? l.AmountCode;
            if (l.AmountCode == "BANK_RECEIPT_AMOUNT" && l.FromBankAccount)
            {
                foreach (var (gl, amount) in banks) Add(l.JournalNo, gl, debit, amount, description);
                continue;
            }
            var value = l.AmountCode switch
            {
                "BANK_RECEIPT_AMOUNT" => banks.Sum(x => x.Amount),
                "CREDIT_USED_AMOUNT" => creditUsed,
                "CREDIT_KEPT_AMOUNT" => creditKept,
                "RECOVERY_AMOUNT" => recovered ?? 0,
                _ => paid.GetValueOrDefault(l.AmountCode[..^"_PAID".Length]),
            };
            if (value == 0) continue;
            if (l.FromBankAccount || string.IsNullOrWhiteSpace(l.AccountCode))
                throw new BusinessRuleException($"GL line {l.LineNo} of rule {plan.RuleCode} ({l.AmountCode}) has no account.");
            Add(l.JournalNo, l.AccountCode, debit, value, description);
        }
        foreach (var j in result.GroupBy(x => x.JournalNo))
            if (j.Sum(x => x.Dr) != j.Sum(x => x.Cr))
                throw new BusinessRuleException($"Journal {j.Key} of rule {plan.RuleCode} does not balance: debit {j.Sum(x => x.Dr):#,0.00}, credit {j.Sum(x => x.Cr):#,0.00}. Check its lines.");
        return result;
    }

    /// <summary>The recovery part of a receipt on a written-off loan: sources, GL rule LND_RECOVERY and the RECOVERY event.</summary>
    private async Task RecoverAsync(LndLoan loan, AdmProduct product, ColRepaymentTransaction txn, List<Source> sources, decimal amount, DateOnly valueDate,
        DateOnly today, CancellationToken cancellationToken)
    {
        var (writtenOff, before) = await RecoveryAsync(loan.LoanId, cancellationToken);
        var (recovered, kept) = SplitRecovery(amount, writtenOff, before);
        txn.AllocatedAmount = recovered;
        var creditUsed = sources.Where(x => x.Type == "CREDIT_BALANCE").Sum(x => x.Amount);
        if (creditUsed > recovered) throw new BusinessRuleException("The credit balance used is more than is left to recover. Use less credit.");
        var toAllocate = recovered;
        foreach (var s in sources.OrderBy(x => x.Type == "CREDIT_BALANCE" ? 0 : 1))
        {
            var used = Math.Min(s.Amount, toAllocate);
            toAllocate -= used;
            if (s.Type == "CREDIT_BALANCE")
            {
                await UseCreditAsync(txn, loan.ClientId, s.Amount, cancellationToken);
                continue;
            }
            db.Set<ColRepaymentSource>().Add(new ColRepaymentSource
            {
                RepaymentTxnId = txn.RepaymentTxnId, SourceType = s.Type, ChequeId = s.ChequeId, SourceReference = s.Reference, SourceAmount = s.Amount, AllocatedAmount = used,
            });
        }

        var plan = await rules.PlanByCodeAsync(product.ProductCode, RecoveryRuleCode, today, cancellationToken)
            ?? throw new BusinessRuleException($"Product {product.ProductCode} has no active rule {RecoveryRuleCode}. Add it in Administration > Product Event Rules: "
                + "DR the bank account BANK_RECEIPT_AMOUNT, DR customer credit CREDIT_USED_AMOUNT, CR recovery income RECOVERY_AMOUNT and CR customer credit CREDIT_KEPT_AMOUNT.");
        var banks = sources.Where(x => x.Type != "CREDIT_BALANCE").GroupBy(x => x.GlAccount!).Select(g => (g.Key, g.Sum(x => x.Amount))).ToList();
        var glLines = Lines(plan, new Dictionary<string, decimal>(), banks, creditUsed, kept, recovered);
        var doc = new ProductActionDocument("LND_RECEIPT", txn.RepaymentTxnId.ToString(CultureInfo.InvariantCulture), txn.RepaymentNumber, today, loan.CurrencyCode,
            $"Loan {loan.LoanNumber} (written off): recovery {txn.RepaymentNumber} of {amount:#,0.00}, value date {valueDate:yyyy-MM-dd}");
        try
        {
            txn.GlTranSetId = await posting.PostLinesAsync(plan, glLines, string.IsNullOrWhiteSpace(product.GlTranSetCode) ? "LND" : product.GlTranSetCode.Trim().ToUpperInvariant(),
                "Loan recovery", "3100", product.BusinessLineCode, doc, cancellationToken);
            txn.IsGlPosted = true;
        }
        catch (ApexProcessException ex)
        {
            throw new BusinessRuleException($"GL posting failed, nothing was received: {GlPostingApi.CleanMessage(ex.Message)}");
        }
        db.Set<LndLoanEvent>().Add(new LndLoanEvent
        {
            LoanId = loan.LoanId, EventCode = "RECOVERY", EventDate = valueDate, EventAmount = recovered, GlTranSetId = txn.GlTranSetId,
            SourceDocumentType = "COL_REPAYMENT", SourceDocumentId = txn.RepaymentTxnId,
            Notes = $"{txn.RepaymentNumber}: recovered {recovered:#,0.00} of {writtenOff:#,0.00} written off ({before + recovered:#,0.00} so far)"
                + (kept > 0 ? $", kept as credit {kept:#,0.00}." : "."),
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private sealed record Source(string Type, decimal Amount, long? ChequeId, string? Reference, string? GlAccount);

    private async Task<List<Source>> CheckSourcesAsync(LndLoan loan, IReadOnlyList<LndReceiptSourceDto> rows, CancellationToken cancellationToken)
    {
        if (rows.Count == 0) throw BusinessValidationException.For("sources", "Add where the money came from.");
        var result = new List<Source>();
        foreach (var r in rows)
        {
            var type = (r.SourceType ?? "").Trim().ToUpperInvariant();
            switch (type)
            {
                case "BANK_DEPOSIT":
                {
                    if (r.Amount is not > 0) throw BusinessValidationException.For("sources", "Enter the amount of each bank deposit.");
                    var a = await db.Set<TrsBankAccount>().AsNoTracking().SingleOrDefaultAsync(x => x.AccountId == r.DepositAccountId, cancellationToken);
                    if (a is null || !a.IsActive || a.CurrencyCode != loan.CurrencyCode)
                        throw BusinessValidationException.For("sources", "Choose one of our active accounts in the loan's currency for each bank deposit.");
                    if (string.IsNullOrWhiteSpace(a.ErpAccountCode)) throw BusinessValidationException.For("sources", $"Bank account {a.AccountRef} has no GL account code. Set it in Treasury.");
                    if (string.IsNullOrWhiteSpace(r.Reference)) throw BusinessValidationException.For("sources", "Enter the bank reference of each deposit.");
                    result.Add(new Source(type, Math.Round(r.Amount!.Value, 2), null, r.Reference.Trim(), a.ErpAccountCode.Trim()));
                    break;
                }
                case "CHEQUE":
                {
                    var c = (await ChequesAsync(loan.ClientId, cancellationToken)).FirstOrDefault(x => x.ChequeId == r.ChequeId)
                        ?? throw BusinessValidationException.For("sources", "Choose a collected cheque of this customer that is not used yet.");
                    if (result.Any(x => x.ChequeId == c.ChequeId)) throw BusinessValidationException.For("sources", $"Cheque {c.ChequeRef} is added twice.");
                    var cheque = await db.Set<ChqCheque>().AsNoTracking().SingleAsync(x => x.ChequeId == c.ChequeId, cancellationToken);
                    var gl = await db.Set<TrsBankAccount>().AsNoTracking().Where(x => x.AccountId == cheque.DepositBankAccountId).Select(x => x.ErpAccountCode).SingleOrDefaultAsync(cancellationToken);
                    if (string.IsNullOrWhiteSpace(gl)) throw BusinessValidationException.For("sources", $"Cheque {c.ChequeRef} has no deposit account with a GL account code.");
                    result.Add(new Source(type, cheque.ChequeAmount, cheque.ChequeId, cheque.ChequeRef, gl.Trim()));
                    break;
                }
                case "CREDIT_BALANCE":
                {
                    if (r.Amount is not > 0) throw BusinessValidationException.For("sources", "Enter how much of the credit balance is used.");
                    var credit = await CreditAsync(loan.ClientId, cancellationToken);
                    if (r.Amount > credit) throw BusinessValidationException.For("sources", $"The customer's credit balance is {credit:#,0.00}.");
                    result.Add(new Source(type, Math.Round(r.Amount!.Value, 2), null, null, null));
                    break;
                }
                default:
                    throw BusinessValidationException.For("sources", "Each source is a bank deposit, a cheque or the credit balance.");
            }
        }
        if (result.Count(x => x.Type == "CREDIT_BALANCE") > 1) throw BusinessValidationException.For("sources", "Use the credit balance once per receipt.");
        return result;
    }

    /// <summary>The customer's credit balance: what is left on the sources of their lending receipts.</summary>
    public Task<decimal> CreditAsync(long clientId, CancellationToken cancellationToken) =>
        (from s in db.Set<ColRepaymentSource>()
         join t in db.Set<ColRepaymentTransaction>() on s.RepaymentTxnId equals t.RepaymentTxnId
         where t.ClientId == clientId && LendingLines.Contains(t.BusinessLineCode) && t.StatusCode == "POSTED" && s.StatusCode == "ACTIVE"
               && s.SourceType != "CREDIT_BALANCE"
         select s.SourceAmount - s.AllocatedAmount).SumAsync(cancellationToken);

    /// <summary>Uses the credit balance oldest receipt first: each use is a CREDIT_BALANCE source pointing at the receipt it came from.</summary>
    private async Task UseCreditAsync(ColRepaymentTransaction txn, long clientId, decimal amount, CancellationToken cancellationToken)
    {
        var open = await (from s in db.Set<ColRepaymentSource>()
                          join t in db.Set<ColRepaymentTransaction>() on s.RepaymentTxnId equals t.RepaymentTxnId
                          where t.ClientId == clientId && LendingLines.Contains(t.BusinessLineCode) && t.StatusCode == "POSTED" && s.StatusCode == "ACTIVE"
                                && s.SourceType != "CREDIT_BALANCE" && s.SourceAmount > s.AllocatedAmount && t.RepaymentTxnId != txn.RepaymentTxnId
                          orderby t.ValueDate, s.RepaymentSourceId
                          select s).ToListAsync(cancellationToken);
        var left = amount;
        foreach (var s in open)
        {
            if (left <= 0) break;
            var use = Math.Min(left, s.SourceAmount - s.AllocatedAmount);
            s.AllocatedAmount += use;
            left -= use;
            db.Set<ColRepaymentSource>().Add(new ColRepaymentSource
            {
                RepaymentTxnId = txn.RepaymentTxnId, SourceType = "CREDIT_BALANCE", SourceRepaymentTxnId = s.RepaymentTxnId, SourceAmount = use, AllocatedAmount = use,
                SourceReference = $"Credit of receipt {s.RepaymentTxnId}",
            });
            var from = await db.Set<ColRepaymentTransaction>().SingleAsync(x => x.RepaymentTxnId == s.RepaymentTxnId, cancellationToken);
            from.AllocatedAmount += use;
        }
        if (left > 0) throw new BusinessRuleException("The customer's credit balance changed. Reload the page.");
    }

    /// <summary>Collected cheques of the customer in the Cheque Wallet that no active receipt uses yet.</summary>
    private async Task<List<LndCollectedChequeDto>> ChequesAsync(long clientId, CancellationToken cancellationToken)
    {
        var client = clientId.ToString(CultureInfo.InvariantCulture);
        var used = from s in db.Set<ColRepaymentSource>()
                   join t in db.Set<ColRepaymentTransaction>() on s.RepaymentTxnId equals t.RepaymentTxnId
                   where s.ChequeId != null && s.StatusCode == "ACTIVE" && t.StatusCode != "REVERSED" && t.StatusCode != "CANCELLED"
                   select s.ChequeId!.Value;
        return await (from c in db.Set<ChqCheque>().AsNoTracking()
                      join a in db.Set<TrsBankAccount>().AsNoTracking() on c.DepositBankAccountId equals (long?)a.AccountId into accounts
                      from a in accounts.DefaultIfEmpty()
                      where c.CustomerId == client && c.StatusCode == "COLLECTED" && !used.Contains(c.ChequeId)
                      orderby c.DueDate
                      select new LndCollectedChequeDto(c.ChequeId, c.ChequeRef, c.ChequeNo, c.DueDate, c.ChequeAmount, a == null ? null : a.AccountRef))
            .ToListAsync(cancellationToken);
    }

    private async Task<List<LndInstallmentOwed>> OwedAsync(long loanId, bool track, CancellationToken cancellationToken)
    {
        var q = track ? db.Set<LndInstallment>() : db.Set<LndInstallment>().AsNoTracking();
        return [.. (await q.Where(x => x.LoanId == loanId && x.StatusCode != "PAID").OrderBy(x => x.InstallmentNo).ToListAsync(cancellationToken))
            .Select(i => new LndInstallmentOwed(i.InstallmentId, i.InstallmentNo, i.DueDate, Owed(i)))];
    }

    private static Dictionary<string, decimal> Owed(LndInstallment i) => new()
    {
        ["LATE_FEE"] = i.LateFeeDueAmount - i.LateFeePaidAmount,
        ["CHARGES"] = i.ChargesDueAmount - i.ChargesPaidAmount,
        ["INSURANCE"] = i.InsuranceDueAmount - i.InsurancePaidAmount,
        ["INTEREST"] = i.InterestDueAmount - i.InterestPaidAmount,
        ["PRINCIPAL"] = i.PrincipalDueAmount - i.PrincipalPaidAmount,
    };

    private static string Label(string type) => type switch { "BANK_DEPOSIT" => "Bank deposit", "CHEQUE" => "Cheque", "CREDIT_BALANCE" => "Credit", _ => type };
}
