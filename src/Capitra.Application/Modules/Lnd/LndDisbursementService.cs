using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl;
using Capitra.Application.Modules.Gl.Journals;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Lnd;
using Capitra.Domain.Modules.Trs;
using Microsoft.EntityFrameworkCore;
using St = Capitra.Application.Modules.Lnd.LndLoanService.Statuses;

namespace Capitra.Application.Modules.Lnd;

public sealed record LndPayingAccountDto(long AccountId, string BankName, string AccountRef, string AccountNo, string AccountTypeCode, string? GlAccountCode);

public sealed record LndChargeDto(string ChargeCode, string ChargeName, decimal Amount, string IncomeRecognitionCode);

public sealed record LndDisbursementDto(
    long DisbursementId, string StatusCode, long PaidFromAccountId, string? PaidFromAccount, string PayoutMethodCode, string PayeeName, string? PayeeAccount,
    decimal PrincipalAmount, decimal ChargesAmount, decimal NetPayoutAmount, DateOnly? PaidDate, string? GlTranSetId, IReadOnlyList<LndChargeDto> Charges,
    string RequestedBy, DateTime RequestedAt, string? DecidedBy, DateTime? DecidedAt, string? DecisionNotes, bool CanPay, string? PayBlockedReason);

/// <summary>What paying the loan out today would be: the rule, its deducted charges and the net, or why it cannot be paid.</summary>
public sealed record LndPayoutPreviewDto(
    string? RuleCode, decimal PrincipalAmount, IReadOnlyList<LndChargeDto> Charges, decimal ChargesAmount, decimal NetPayoutAmount, string? Problem);

public sealed record LndDisbursementPageDto(
    long LoanId, string LoanStatusCode, bool CanRequest, bool CanPay, IReadOnlyList<LndPayingAccountDto> Accounts, LndPayoutPreviewDto Preview,
    IReadOnlyList<LndDisbursementDto> Disbursements);

public sealed record LndDisbursementRequestDto(long? PaidFromAccountId, string? PayoutMethodCode, string? PayeeName, string? PayeeAccount);

public sealed record LndDisbursementDecisionDto(string? Notes);

/// <summary>
/// Paying a retail loan out (design deck: step 5 of the lifecycle). A user asks for the payout with the account we pay from and the
/// payee; another user pays it (four eyes, always). Paying works out the charges of the rule LND_DISBURSE of the loan's product (each
/// charge must be deducted from the disbursement), posts its GL under CAP-{product GL tran set code or LND}, writes the loan event, and
/// moves the loan from APPROVED to ACTIVE, all in one transaction. A payout on another day than planned rebuilds the schedule from it.
/// </summary>
public sealed class LndDisbursementService(
    ICapitraDbContext db, ICurrentUser currentUser, IClock clock, AdmProductEventRuleService rules, ProductActionPosting posting, LndLoanService loans)
{
    public const string RuleCode = "LND_DISBURSE";

    public static readonly IReadOnlyList<string> PayoutMethods = ["BANK_TRANSFER", "CASH", "MERCHANT", "WALLET"];

    public static class Statuses
    {
        public const string Requested = "REQUESTED";
        public const string Paid = "PAID";
        public const string Rejected = "REJECTED";
        public const string Withdrawn = "WITHDRAWN";
    }

    private string User => currentUser.UserName ?? currentUser.UserId ?? "SYSTEM";

    private bool SameUser(string? by) => by is not null && string.Equals(by, User, StringComparison.OrdinalIgnoreCase);

    public async Task<LndDisbursementPageDto> GetAsync(long loanId, bool canRequest, bool canPay, CancellationToken cancellationToken)
    {
        var loan = await LoanAsync(loanId, false, cancellationToken);
        var accounts = await AccountsAsync(loan.CurrencyCode, cancellationToken);
        var names = accounts.ToDictionary(x => x.AccountId, x => $"{x.BankName} · {x.AccountRef}");
        var rows = await db.Set<LndDisbursement>().AsNoTracking().Where(x => x.LoanId == loanId).OrderByDescending(x => x.DisbursementId).ToListAsync(cancellationToken);
        var ids = rows.Select(x => x.DisbursementId).ToList();
        var charges = await db.Set<LndDisbursementCharge>().AsNoTracking().Where(x => ids.Contains(x.DisbursementId)).ToListAsync(cancellationToken);
        var preview = await PreviewAsync(loan, GlTime.Today(clock), cancellationToken);
        return new LndDisbursementPageDto(loanId, loan.StatusCode, canRequest, canPay, accounts, preview,
            [.. rows.Select(d =>
            {
                var blocked = d.StatusCode != Statuses.Requested ? null
                    : SameUser(d.RequestedBy) ? "You asked for this payout; another user must pay it."
                    : preview.Problem;
                return new LndDisbursementDto(d.DisbursementId, d.StatusCode, d.PaidFromAccountId, names.GetValueOrDefault(d.PaidFromAccountId), d.PayoutMethodCode,
                    d.PayeeName, d.PayeeAccount, d.PrincipalAmount, d.ChargesAmount, d.NetPayoutAmount, d.PaidDate, d.GlTranSetId,
                    [.. charges.Where(c => c.DisbursementId == d.DisbursementId).Select(c => new LndChargeDto(c.ChargeCode, c.ChargeName, c.ChargeAmount, c.IncomeRecognitionCode))],
                    d.RequestedBy, d.RequestedAt, d.DecidedBy, d.DecidedAt, d.DecisionNotes,
                    d.StatusCode == Statuses.Requested && canPay && blocked is null, blocked);
            })]);
    }

    public async Task RequestAsync(long loanId, LndDisbursementRequestDto dto, CancellationToken cancellationToken)
    {
        var loan = await LoanAsync(loanId, true, cancellationToken);
        if (loan.StatusCode != St.Approved) throw new BusinessRuleException("Only an approved loan can be paid out.");
        if (await db.Set<LndDisbursement>().AnyAsync(x => x.LoanId == loanId && x.StatusCode == Statuses.Requested, cancellationToken))
            throw new BusinessRuleException("A payout of this loan is already waiting. Pay, reject or withdraw it first.");

        var e = new Dictionary<string, string[]>();
        var accounts = await AccountsAsync(loan.CurrencyCode, cancellationToken);
        if (dto.PaidFromAccountId is null) e["paidFromAccountId"] = ["Choose the account the money is paid from."];
        else if (accounts.FirstOrDefault(x => x.AccountId == dto.PaidFromAccountId) is not { } a) e["paidFromAccountId"] = ["Choose one of our active accounts in the loan's currency."];
        else if (string.IsNullOrWhiteSpace(a.GlAccountCode)) e["paidFromAccountId"] = [$"Bank account {a.AccountRef} has no GL account code. Set it in Treasury."];
        var method = (dto.PayoutMethodCode ?? "").Trim().ToUpperInvariant();
        if (!PayoutMethods.Contains(method)) e["payoutMethodCode"] = ["Choose how the money is paid."];
        if (string.IsNullOrWhiteSpace(dto.PayeeName)) e["payeeName"] = ["Enter who is paid."];
        if (method is "BANK_TRANSFER" or "WALLET" && string.IsNullOrWhiteSpace(dto.PayeeAccount)) e["payeeAccount"] = ["Enter the account or wallet number paid to."];
        if (e.Count > 0) throw new BusinessValidationException(e);

        var preview = await PreviewAsync(loan, GlTime.Today(clock), cancellationToken);
        if (preview.Problem is not null) throw new BusinessRuleException(preview.Problem);
        db.Set<LndDisbursement>().Add(new LndDisbursement
        {
            LoanId = loanId, StatusCode = Statuses.Requested, PaidFromAccountId = dto.PaidFromAccountId!.Value, PayoutMethodCode = method,
            PayeeName = dto.PayeeName!.Trim(), PayeeAccount = string.IsNullOrWhiteSpace(dto.PayeeAccount) ? null : dto.PayeeAccount.Trim(),
            PrincipalAmount = loan.PrincipalAmount, ChargesAmount = preview.ChargesAmount, NetPayoutAmount = preview.NetPayoutAmount,
            RequestedBy = User, RequestedAt = clock.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Pays the payout today: charges, GL, loan event, schedule (when the day changed) and ACTIVE, all or nothing.</summary>
    public async Task PayAsync(long disbursementId, LndDisbursementDecisionDto dto, CancellationToken cancellationToken)
    {
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var d = await RequestedAsync(disbursementId, cancellationToken);
        if (SameUser(d.RequestedBy)) throw new BusinessRuleException("You asked for this payout. Another user must pay it.");
        var loan = await LoanAsync(d.LoanId, true, cancellationToken);
        if (loan.StatusCode != St.Approved) throw new BusinessRuleException("The loan is no longer approved for payout.");
        var product = await db.Set<AdmProduct>().AsNoTracking().SingleAsync(x => x.ProductId == loan.ProductId, cancellationToken);
        var account = await db.Set<TrsBankAccount>().AsNoTracking().SingleAsync(x => x.AccountId == d.PaidFromAccountId, cancellationToken);
        if (!account.IsActive || string.IsNullOrWhiteSpace(account.ErpAccountCode))
            throw new BusinessRuleException($"Bank account {account.AccountRef} is inactive or has no GL account code. Reject this payout and ask again.");

        var today = GlTime.Today(clock);
        var (plan, amounts, problem) = await CalculateAsync(product.ProductCode, loan.PrincipalAmount, today, cancellationToken);
        if (problem is not null) throw new BusinessRuleException(problem);
        var chargesTotal = amounts.Sum(x => x.Amount.Amount);
        var net = loan.PrincipalAmount - chargesTotal;

        // a payout on another day than planned: the schedule starts from the real day
        if (today != loan.DisbursementDate) await RescheduleAsync(loan, today, cancellationToken);

        var lines = Lines(plan!, loan.PrincipalAmount, net, amounts, account.ErpAccountCode.Trim());
        var doc = new ProductActionDocument("LND_DISBURSEMENT", d.DisbursementId.ToString(CultureInfo.InvariantCulture), loan.LoanNumber, today, loan.CurrencyCode,
            $"Loan {loan.LoanNumber}: payout of {loan.PrincipalAmount:#,0.00} to {d.PayeeName}");
        string ts;
        try
        {
            ts = await posting.PostLinesAsync(plan!, lines, string.IsNullOrWhiteSpace(product.GlTranSetCode) ? "LND" : product.GlTranSetCode.Trim().ToUpperInvariant(),
                "Loan disbursement", "3100", product.BusinessLineCode, doc, cancellationToken);
        }
        catch (ApexProcessException ex)
        {
            throw new BusinessRuleException($"GL posting failed, nothing was paid: {GlPostingApi.CleanMessage(ex.Message)}");
        }

        d.StatusCode = Statuses.Paid;
        d.ChargesAmount = chargesTotal;
        d.NetPayoutAmount = net;
        d.PaidDate = today;
        d.GlTranSetId = ts;
        d.DecidedBy = User;
        d.DecidedAt = clock.UtcNow;
        d.DecisionNotes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        db.Set<LndDisbursementCharge>().AddRange(amounts.Where(x => x.Amount.Amount != 0).Select(x => new LndDisbursementCharge
        {
            DisbursementId = d.DisbursementId, ChargeCode = x.Charge.ChargeCode, ChargeName = x.Charge.ChargeNameEn, ChargeAmount = x.Amount.Amount,
            IncomeRecognitionCode = x.Charge.IncomeRecognitionCode,
        }));
        db.Set<LndLoanEvent>().Add(new LndLoanEvent
        {
            LoanId = loan.LoanId, EventCode = "DISBURSED", EventDate = today, EventAmount = loan.PrincipalAmount, GlTranSetId = ts,
            SourceDocumentType = "LND_DISBURSEMENT", SourceDocumentId = d.DisbursementId,
            Notes = $"Paid {net:#,0.00} to {d.PayeeName}; charges {chargesTotal:#,0.00}.",
        });
        await db.SaveChangesAsync(cancellationToken);
        await loans.ChangeStatusAsync(loan.LoanId, St.Active, $"Paid out, {ts}.", null, cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    public async Task RejectAsync(long disbursementId, LndDisbursementDecisionDto dto, CancellationToken cancellationToken)
    {
        var d = await RequestedAsync(disbursementId, cancellationToken);
        if (SameUser(d.RequestedBy)) throw new BusinessRuleException("You asked for this payout. Withdraw it instead.");
        Decide(d, Statuses.Rejected, string.IsNullOrWhiteSpace(dto.Notes) ? throw BusinessValidationException.For("notes", "Give the reason for the rejection.") : dto.Notes.Trim());
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task WithdrawAsync(long disbursementId, CancellationToken cancellationToken)
    {
        var d = await RequestedAsync(disbursementId, cancellationToken);
        if (!SameUser(d.RequestedBy)) throw new BusinessRuleException("Only the user who asked for the payout can withdraw it.");
        Decide(d, Statuses.Withdrawn, null);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// The GL lines of the payout: the rule's own lines with the loan amount (PRINCIPAL_AMOUNT) or the net paid (NET_PAYOUT_AMOUNT),
    /// the bank-account lines on the account we pay from; each charge's lines with its amount (all of it, or the part earned now or later).
    /// </summary>
    public static IReadOnlyList<ProductEventPostingLine> Lines(
        ProductEventPlan plan, decimal principal, decimal net, IReadOnlyList<(ProductEventCharge Charge, ProductEventChargeAmount Amount)> charges, string bankGl)
    {
        var byCode = charges.ToDictionary(x => x.Charge.ChargeCode);
        return ProductEventPosting.Build(plan, l =>
        {
            if (l.ChargeCode is null) return l.AmountCode switch { "PRINCIPAL_AMOUNT" => principal, "NET_PAYOUT_AMOUNT" => net, _ => 0 };
            if (!byCode.TryGetValue(l.ChargeCode, out var c)) return 0;
            var prorate = c.Charge.IncomeRecognitionCode == "PRORATE";
            return l.AmountCode switch
            {
                "CHARGE_AMOUNT" => c.Amount.Amount,
                "CHARGE_DEFERRED" => prorate ? c.Amount.Amount : 0,
                "CHARGE_IMMEDIATE" => prorate ? 0 : c.Amount.Amount,
                _ => 0,
            };
        }, bankGl);
    }

    /// <summary>The rule's checks: only the payout's figures, every charge deducted from the disbursement, and a net above zero.</summary>
    public static string? Check(ProductEventPlan plan, decimal principal, IReadOnlyList<(ProductEventCharge Charge, ProductEventChargeAmount Amount)> charges)
    {
        var other = plan.Lines.Where(l => l.ChargeCode is null && l.AmountCode is not ("PRINCIPAL_AMOUNT" or "NET_PAYOUT_AMOUNT")).Select(l => l.AmountCode).Distinct().ToList();
        if (other.Count > 0) return $"Rule {plan.RuleCode} posts {string.Join(", ", other)}; a loan payout gives only PRINCIPAL_AMOUNT and NET_PAYOUT_AMOUNT.";
        if (!plan.Lines.Any(l => l.ChargeCode is null && l.AmountCode == "NET_PAYOUT_AMOUNT" && l.FromBankAccount))
            return $"Rule {plan.RuleCode} has no line paying NET_PAYOUT_AMOUNT from the bank account.";
        var notDeducted = charges.Where(x => x.Charge.ChargeTimingCode != "DEDUCT_FROM_DISBURSEMENT").Select(x => x.Charge.ChargeCode).ToList();
        if (notDeducted.Count > 0) return $"Charge {string.Join(", ", notDeducted)} of rule {plan.RuleCode} is not deducted from the disbursement; only deducted charges are taken at payout for now.";
        var failed = charges.FirstOrDefault(x => x.Amount.Problem is not null);
        if (failed.Charge is not null) return $"Charge {failed.Charge.ChargeCode} cannot be worked out: {failed.Amount.Problem}.";
        if (principal - charges.Sum(x => x.Amount.Amount) <= 0) return "The charges take the whole loan amount; nothing would be paid out.";
        return null;
    }

    private async Task<LndPayoutPreviewDto> PreviewAsync(LndLoan loan, DateOnly date, CancellationToken cancellationToken)
    {
        var product = await db.Set<AdmProduct>().AsNoTracking().SingleAsync(x => x.ProductId == loan.ProductId, cancellationToken);
        var (plan, amounts, problem) = await CalculateAsync(product.ProductCode, loan.PrincipalAmount, date, cancellationToken);
        var charges = amounts.Select(x => new LndChargeDto(x.Charge.ChargeCode, x.Charge.ChargeNameEn, x.Amount.Amount, x.Charge.IncomeRecognitionCode)).ToList();
        var total = charges.Sum(x => x.Amount);
        return new LndPayoutPreviewDto(plan?.RuleCode, loan.PrincipalAmount, charges, total, loan.PrincipalAmount - total, problem);
    }

    private async Task<(ProductEventPlan? Plan, List<(ProductEventCharge Charge, ProductEventChargeAmount Amount)> Amounts, string? Problem)> CalculateAsync(
        string productCode, decimal principal, DateOnly date, CancellationToken cancellationToken)
    {
        var plan = await rules.PlanByCodeAsync(productCode, RuleCode, date, cancellationToken);
        if (plan is null)
            return (null, [], $"Product {productCode} has no active rule {RuleCode} on {date:yyyy-MM-dd}. Add it in Administration > Product Event Rules: "
                + "DR loans receivable PRINCIPAL_AMOUNT, CR the bank account NET_PAYOUT_AMOUNT, and each deducted charge.");
        decimal? BaseOf(string code) => code == "LOAN_AMOUNT" ? principal : null;
        var amounts = plan.Charges.Select(c => (c, ProductEventPosting.Calculate(c, BaseOf, 0))).ToList();
        return (plan, amounts, Check(plan, principal, amounts));
    }

    /// <summary>The schedule from the real payout day: the first instalment keeps its date when still valid, else one period after.</summary>
    private async Task RescheduleAsync(LndLoan loan, DateOnly paidOn, CancellationToken cancellationToken)
    {
        var first = loan.FirstDueDate;
        var version = await db.Set<LndProductVersion>().AsNoTracking().SingleAsync(x => x.ProductVersionId == loan.ProductVersionId, cancellationToken);
        if (LndLoanService.CheckTerms(version, loan.PrincipalAmount, loan.TermMonths, paidOn, first).ContainsKey("firstDueDate"))
            first = LndSchedule.DefaultFirstDueDate(paidOn, loan.RepaymentFrequencyCode);
        var lines = LndSchedule.Build(loan.PrincipalAmount, loan.TermMonths, loan.RepaymentFrequencyCode, loan.InterestMethodCode, loan.AnnualRatePct, first);
        var old = await db.Set<LndInstallment>().Where(x => x.LoanId == loan.LoanId).ToListAsync(cancellationToken);
        db.Set<LndInstallment>().RemoveRange(old);
        db.Set<LndInstallment>().AddRange(lines.Select(x => new LndInstallment
        {
            LoanId = loan.LoanId, InstallmentNo = x.InstallmentNo, DueDate = x.DueDate, OpeningPrincipalAmount = x.OpeningPrincipal,
            PrincipalDueAmount = x.Principal, InterestDueAmount = x.Interest, ClosingPrincipalAmount = x.ClosingPrincipal,
        }));
        loan.DisbursementDate = paidOn;
        loan.FirstDueDate = first;
        loan.MaturityDate = lines[^1].DueDate;
        loan.InstallmentCount = lines.Count;
        loan.InstallmentAmount = lines[0].Total;
        loan.TotalInterestAmount = lines.Sum(x => x.Interest);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<LndPayingAccountDto>> AccountsAsync(string currencyCode, CancellationToken cancellationToken) =>
        await (from a in db.Set<TrsBankAccount>().AsNoTracking()
               join b in db.Set<TrsBank>().AsNoTracking() on a.BankId equals b.BankId
               where a.IsActive && a.CurrencyCode == currencyCode && a.AccountTypeCode != "ESCROW"
               orderby b.BankNameEn, a.AccountRef
               select new LndPayingAccountDto(a.AccountId, b.BankNameEn, a.AccountRef, a.AccountNo, a.AccountTypeCode, a.ErpAccountCode))
            .ToListAsync(cancellationToken);

    private async Task<LndLoan> LoanAsync(long loanId, bool track, CancellationToken cancellationToken)
    {
        var q = track ? db.Set<LndLoan>() : db.Set<LndLoan>().AsNoTracking();
        return await q.SingleOrDefaultAsync(x => x.LoanId == loanId, cancellationToken) ?? throw new NotFoundException($"Loan {loanId} was not found.");
    }

    private async Task<LndDisbursement> RequestedAsync(long id, CancellationToken cancellationToken)
    {
        var d = await db.Set<LndDisbursement>().SingleOrDefaultAsync(x => x.DisbursementId == id, cancellationToken)
            ?? throw new NotFoundException($"Payout {id} was not found.");
        return d.StatusCode == Statuses.Requested ? d : throw new BusinessRuleException("This payout is no longer waiting.");
    }

    private void Decide(LndDisbursement d, string status, string? notes)
    {
        d.StatusCode = status;
        d.DecidedBy = User;
        d.DecidedAt = clock.UtcNow;
        d.DecisionNotes = notes;
    }
}
