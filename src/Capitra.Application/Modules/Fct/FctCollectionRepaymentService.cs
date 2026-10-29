using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Gl;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Chq;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Capitra.Domain.Modules.Trs;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Fct;

/// <summary>App 136 page 5: one row of FNV_REPAYMENT_TRANSACTIONS with the client's name.</summary>
public sealed record FctRepaymentListDto(
    long RepaymentTxnId, string RepaymentNumber, long ClientId, string? ClientName, string RepaymentMethod, DateOnly TransactionDate,
    DateOnly ValueDate, string? CurrencyCode, decimal ReceiptAmount, decimal AllocatedAmount, string StatusCode);

/// <summary>App 136 page 6 "RepaymentSources" grid.</summary>
public sealed record FctRepaymentSourceDto(
    long RepaymentSourceId, string SourceType, long? ChequeId, decimal SourceAmount, decimal AllocatedAmount, string StatusCode);

/// <summary>App 136 page 6 "RepaymentAllocation" grid.</summary>
public sealed record FctRepaymentAllocationDto(
    long AllocationId, long RepaymentSourceId, string ComponentCode, long DisbursementId, decimal AllocationAmount);

/// <summary>App 136 page 6: the repayment transaction form with its sources and allocations.</summary>
public sealed record FctRepaymentFormDto(
    long RepaymentTxnId, string RepaymentNumber, long ClientId, string? ClientName, string RepaymentMethod, DateOnly TransactionDate,
    DateOnly ValueDate, decimal ReceiptAmount, string StatusCode, string? BankStatementReference, string? ExternalReference,
    string? ReversalReason, string? Notes, IReadOnlyList<FctRepaymentSourceDto> Sources, IReadOnlyList<FctRepaymentAllocationDto> Allocations);

/// <summary>The fields page 6 lets the user change (all read-only once the transaction is ALLOCATED).</summary>
public sealed record FctRepaymentUpdateDto(
    DateOnly? TransactionDate, DateOnly? ValueDate, string? Notes, string? BankStatementReference, string? ExternalReference, string? ReversalReason);

/// <summary>App 136 page 7 "New Bank Transfer".</summary>
public sealed record FctBankTransferDto(
    long? ClientId, decimal? ReceiptAmount, DateOnly? TransactionDate, DateOnly? ValueDate, string? Notes, string? BankStatementReference,
    string? ExternalReference);

public sealed record FctRepaymentSavedDto(long RepaymentTxnId, string RepaymentNumber, string Message);

public sealed record FctOptionDto(string Value, string Label);

/// <summary>App 136 page 712 grid: one row of FNV_DUE_CHEQUES_V joined to TRS_BANKS.</summary>
public sealed record FctDueChequeDto(
    long ChequeId, string ChequeRef, string ChequeNo, DateOnly DueDate, string IssuerBankName, string DrawerNameEn, string? CustomerId,
    string ClientName, string BankNameEn, decimal ChequeAmount, string CurrencyCode, string StatusCode, string? RejectionReasonCode);

/// <summary>One cheque's bank result, as page 712's applyBankResults() sends it.</summary>
public sealed record FctChequeResultDto(
    long? ChequeId, string? CustomerId, decimal? ChequeAmount, string? NewStatusCode, DateOnly? StatusDate, string? BankStatementReference,
    string? ReasonCode, string? Notes);

/// <summary>
/// Collection and Repayments (APEX App 136): customer repayment transactions (pages 5, 6, 7) and the cheques collection
/// (page 712). The rules are FNV_COLLECTION_REPAYMENT_PKG's save_repayment, update_cheque_status, apply_cheque_status and
/// create_cheque_payment_sources.
/// </summary>
public sealed class FctCollectionRepaymentService(ICapitraDbContext db, ICurrentUser currentUser, IClock clock, WfeRuntime runtime)
{
    // App 136 passes these fixed values to save_repayment (pages 7 and 712): legal entity 1, currency 1 / EGP, GL account 15.
    public const long LegalEntityId = 1;
    public const string CurrencyCode = "EGP";
    public const long SourceGlAccountId = 15;

    public const string WorkflowCode = "CUSTOMER_REPAYMENT_WORKFLOW";

    /// <summary>The Bank Result list of page 712 (static LOV).</summary>
    public static readonly IReadOnlyList<FctOptionDto> BankResults =
    [
        new("COLLECTED", "Collected"),
        new("BOUNCED", "Bounced"),
        new("REJECTED", "Rejected"),
    ];

    private static readonly string[] DueStatuses = ["RECEIVED", "DEPOSITED", "PRESENTED", "DUE", "BOUNCED", "REJECTED"];

    private string User => string.IsNullOrWhiteSpace(currentUser.UserName) ? "SYSTEM" : currentUser.UserName!;

    // ---- page 5 ----

    public async Task<IReadOnlyList<FctRepaymentListDto>> ListAsync(CancellationToken cancellationToken) =>
        await (from r in db.Set<FctRepaymentTransaction>().AsNoTracking()
               join c in db.Set<CrmClient>().AsNoTracking() on r.ClientId equals c.ClientId into cs
               from c in cs.DefaultIfEmpty()
               join cur in db.Set<CrmLkpCurrency>().AsNoTracking() on r.CurrencyId equals cur.CurrencyId into curs
               from cur in curs.DefaultIfEmpty()
               orderby r.RepaymentTxnId descending
               select new FctRepaymentListDto(r.RepaymentTxnId, r.RepaymentNumber, r.ClientId, c == null ? null : c.ClientName,
                   r.RepaymentMethod, r.TransactionDate, r.ValueDate, cur == null ? r.CurrencyCode : cur.CurrencyCode, r.ReceiptAmount,
                   r.AllocatedAmount, r.StatusCode))
            .ToListAsync(cancellationToken);

    // ---- page 6 ----

    public async Task<FctRepaymentFormDto> GetAsync(long repaymentTxnId, CancellationToken cancellationToken)
    {
        var r = await db.Set<FctRepaymentTransaction>().AsNoTracking().FirstOrDefaultAsync(x => x.RepaymentTxnId == repaymentTxnId, cancellationToken)
            ?? throw new NotFoundException($"Repayment transaction {repaymentTxnId} was not found.");
        var clientName = await db.Set<CrmClient>().AsNoTracking().Where(x => x.ClientId == r.ClientId).Select(x => x.ClientName)
            .FirstOrDefaultAsync(cancellationToken);
        var sources = await db.Set<FctRepaymentSource>().AsNoTracking().Where(x => x.RepaymentTxnId == repaymentTxnId)
            .OrderBy(x => x.RepaymentSourceId)
            .Select(x => new FctRepaymentSourceDto(x.RepaymentSourceId, x.SourceType, x.ChequeId, x.SourceAmount, x.AllocatedAmount, x.StatusCode))
            .ToListAsync(cancellationToken);
        var allocations = await db.Set<FctRepaymentAllocation>().AsNoTracking().Where(x => x.RepaymentTxnId == repaymentTxnId)
            .OrderBy(x => x.AllocationId)
            .Select(x => new FctRepaymentAllocationDto(x.AllocationId, x.RepaymentSourceId, x.ComponentCode, x.DisbursementId, x.AllocationAmount))
            .ToListAsync(cancellationToken);
        return new FctRepaymentFormDto(r.RepaymentTxnId, r.RepaymentNumber, r.ClientId, clientName, r.RepaymentMethod, r.TransactionDate,
            r.ValueDate, r.ReceiptAmount, r.StatusCode, r.BankStatementReference, r.ExternalReference, r.ReversalReason, r.Notes, sources, allocations);
    }

    /// <summary>Page 6 Update (form DML); the button and the fields are off once the transaction is ALLOCATED.</summary>
    public async Task<FctRepaymentFormDto> UpdateAsync(long repaymentTxnId, FctRepaymentUpdateDto dto, CancellationToken cancellationToken)
    {
        var r = await db.Set<FctRepaymentTransaction>().FirstOrDefaultAsync(x => x.RepaymentTxnId == repaymentTxnId, cancellationToken)
            ?? throw new NotFoundException($"Repayment transaction {repaymentTxnId} was not found.");
        if (r.StatusCode == "ALLOCATED")
            throw new BusinessRuleException("An allocated repayment transaction cannot be changed.");

        var errors = new Dictionary<string, string[]>();
        if (dto.TransactionDate is null) errors["transactionDate"] = ["Transaction Date must have some value."];
        if (dto.ValueDate is null) errors["valueDate"] = ["Value Date must have some value."];
        if (errors.Count > 0) throw new BusinessValidationException(errors);

        r.TransactionDate = dto.TransactionDate!.Value;
        r.ValueDate = dto.ValueDate!.Value;
        r.Notes = Trim(dto.Notes);
        r.BankStatementReference = Trim(dto.BankStatementReference);
        r.ExternalReference = Trim(dto.ExternalReference);
        r.ReversalReason = Trim(dto.ReversalReason);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(repaymentTxnId, cancellationToken);
    }

    // ---- page 7 ----

    /// <summary>Page 7 Customer ID list: clients that are not prospects, as "name -id".</summary>
    public async Task<IReadOnlyList<FctOptionDto>> CustomersAsync(CancellationToken cancellationToken) =>
        (await db.Set<CrmClient>().AsNoTracking().Where(x => x.ClientType != "PROSPECT").OrderBy(x => x.ClientName)
            .Select(x => new { x.ClientId, x.ClientName }).ToListAsync(cancellationToken))
        .Select(x => new FctOptionDto(x.ClientId.ToString(CultureInfo.InvariantCulture), $"{x.ClientName} -{x.ClientId}"))
        .ToList();

    /// <summary>Page 7 Save Transfer: save_repayment with method BANK_TRANSFER.</summary>
    public async Task<FctRepaymentSavedDto> SaveBankTransferAsync(FctBankTransferDto dto, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        if (dto.ClientId is null) errors["clientId"] = ["Customer ID must have some value."];
        if (dto.ReceiptAmount is null) errors["receiptAmount"] = ["Receipt Amount must have some value."];
        if (dto.TransactionDate is null) errors["transactionDate"] = ["Transaction Date must have some value."];
        if (dto.ValueDate is null) errors["valueDate"] = ["Value Date must have some value."];
        if (errors.Count > 0) throw new BusinessValidationException(errors);
        if (!await db.Set<CrmClient>().AnyAsync(x => x.ClientId == dto.ClientId, cancellationToken))
            throw BusinessValidationException.For("clientId", "Customer was not found.");

        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var txn = await SaveRepaymentAsync(dto.ClientId!.Value, "BANK_TRANSFER", dto.TransactionDate!.Value, dto.ValueDate!.Value,
            dto.ReceiptAmount!.Value, Trim(dto.BankStatementReference), Trim(dto.ExternalReference), Trim(dto.Notes), cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return new FctRepaymentSavedDto(txn.RepaymentTxnId, txn.RepaymentNumber, $"Bank transfer {txn.RepaymentNumber} saved.");
    }

    // ---- page 712 ----

    /// <summary>FNV_DEPOSIT_BANK_LOV_V: every deposit account used on a cheque, by its IBAN.</summary>
    public async Task<IReadOnlyList<FctOptionDto>> DepositAccountsAsync(CancellationToken cancellationToken)
    {
        var rows = await (from c in db.Set<ChqCheque>().AsNoTracking()
                          where c.DepositBankAccountId != null
                          join b in db.Set<TrsBankAccount>().AsNoTracking() on c.DepositBankAccountId equals b.AccountId into bs
                          from b in bs.DefaultIfEmpty()
                          select new { Id = c.DepositBankAccountId!.Value, Iban = b == null ? null : b.Iban })
            .Distinct().ToListAsync(cancellationToken);
        return rows.Select(x => new FctOptionDto(x.Id.ToString(CultureInfo.InvariantCulture), x.Iban ?? $"Bank Account {x.Id}"))
            .OrderBy(x => x.Label, StringComparer.Ordinal).ToList();
    }

    /// <summary>The Due Cheques grid: FNV_DUE_CHEQUES_V (factoring cheques not yet collected) joined to the deposit bank.</summary>
    public async Task<IReadOnlyList<FctDueChequeDto>> DueChequesAsync(
        long? depositBankAccountId, DateOnly? dueFrom, DateOnly? dueTo, CancellationToken cancellationToken)
    {
        var start = dueFrom ?? new DateOnly(1900, 1, 1);
        var end = dueTo ?? new DateOnly(2999, 12, 31);
        return await (from c in db.Set<ChqCheque>().AsNoTracking()
                      join fc in db.Set<CrmClient>().AsNoTracking() on c.CustomerId equals fc.ClientId.ToString()
                      join bk in db.Set<TrsBank>().AsNoTracking() on c.DepositBankId equals (long?)bk.BankId
                      where c.SourceModule == "FACTORING" && DueStatuses.Contains(c.StatusCode)
                            && (depositBankAccountId == null || c.DepositBankAccountId == depositBankAccountId)
                            && c.DueDate >= start && c.DueDate <= end
                      orderby c.DueDate, c.ChequeId
                      select new FctDueChequeDto(c.ChequeId, c.ChequeRef, c.ChequeNo, c.DueDate, c.IssuerBankName, c.DrawerNameEn, c.CustomerId,
                          fc.ClientName, bk.BankNameEn, c.ChequeAmount, c.CurrencyCode, c.StatusCode, c.RejectionReasonCode))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Apply Bank Results (apply_cheque_status): sets every cheque's result, then for each client with collected cheques
    /// makes one CHEQUE repayment transaction and one repayment request, links the cheques as its sources and starts the
    /// repayment workflow. All of it succeeds or fails together.
    /// </summary>
    public async Task<string> ApplyChequeResultsAsync(IReadOnlyList<FctChequeResultDto>? rows, CancellationToken cancellationToken)
    {
        if (rows is null) throw new BusinessRuleException("No cheque result rows were submitted.");
        if (rows.Count == 0) throw new BusinessRuleException("Select at least one cheque result.");

        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var today = GlTime.Today(clock);
        var customers = new SortedDictionary<string, (int Count, decimal Amount)>(StringComparer.Ordinal);
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var status = row.NewStatusCode?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(status)) throw new BusinessRuleException($"New cheque status is NULL in JSON. Row={i}");
            if (row.ChequeId is null) throw new BusinessRuleException($"No data found while updating cheque. Row={i}, Cheque ID=NULL");

            await UpdateChequeStatusAsync(row.ChequeId.Value, status, row.StatusDate ?? today, row.BankStatementReference, Trim(row.ReasonCode),
                Trim(row.Notes), cancellationToken);

            if (status != "COLLECTED") continue;
            if (!long.TryParse(row.CustomerId?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var customerId))
                throw new BusinessRuleException($"Customer ID is NULL in JSON. Row={i}");
            if (row.ChequeAmount is null) throw new BusinessRuleException($"Cheque amount is NULL in JSON. Row={i}");
            if (row.ChequeAmount < 0) throw new BusinessRuleException($"Cheque amount cannot be negative. Row={i}");

            var key = customerId.ToString(CultureInfo.InvariantCulture);
            customers[key] = customers.TryGetValue(key, out var sum)
                ? (sum.Count + 1, sum.Amount + row.ChequeAmount.Value)
                : (1, row.ChequeAmount.Value);
        }

        foreach (var (key, sum) in customers)
        {
            var clientId = long.Parse(key, CultureInfo.InvariantCulture);
            var txn = await SaveRepaymentAsync(clientId, "CHEQUE", today, today, 0, null, null, null, cancellationToken);
            var request = new FctRepaymentRequest
            {
                ClientId = clientId,
                ChequesCount = sum.Count,
                ChequesAmount = sum.Amount,
                RemainingAmount = sum.Amount,
                RepaymentTranNo = txn.RepaymentTxnId,
                Source = "CHEQUE",
                BankTransfer = 0,
                CreditBalance = 0,
            };
            db.Add(request);
            await db.SaveChangesAsync(cancellationToken);
            txn.AgreementId = request.RepaymentRequestId;
            await db.SaveChangesAsync(cancellationToken);

            foreach (var row in rows.Where(r => r.NewStatusCode?.Trim().ToUpperInvariant() == "COLLECTED" && r.CustomerId?.Trim() == key))
                await CreateChequePaymentSourceAsync(txn, row.ChequeId!.Value, cancellationToken);

            await StartWorkflowAsync(request, sum.Amount, cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
        return "Cheque bank results were applied successfully.";
    }

    // ---- FNV_COLLECTION_REPAYMENT_PKG ----

    /// <summary>save_repayment: a new DRAFT transaction numbered RPT-yyyyMM-id; a bank transfer also gets its source and request.</summary>
    private async Task<FctRepaymentTransaction> SaveRepaymentAsync(
        long clientId, string method, DateOnly transactionDate, DateOnly valueDate, decimal receiptAmount, string? bankStatementReference,
        string? externalReference, string? notes, CancellationToken cancellationToken)
    {
        if (method == "BANK_TRANSFER" && receiptAmount <= 0)
            throw BusinessValidationException.For("receiptAmount", "Repayment amount must be greater than zero.");

        var currencyId = await db.Set<CrmLkpCurrency>().AsNoTracking().Where(x => x.CurrencyCode == CurrencyCode)
            .Select(x => (long?)x.CurrencyId).FirstOrDefaultAsync(cancellationToken);
        var txn = new FctRepaymentTransaction
        {
            RepaymentNumber = "PENDING-" + Guid.NewGuid().ToString("N").ToUpperInvariant(),
            ClientId = clientId,
            LegalEntityId = LegalEntityId,
            RepaymentMethod = method,
            TransactionDate = transactionDate,
            ValueDate = valueDate,
            CurrencyId = currencyId,
            CurrencyCode = CurrencyCode,
            ReceiptAmount = method == "CHEQUE" ? 0 : receiptAmount,
            AllocatedAmount = 0,
            SourceGlAccountId = SourceGlAccountId,
            BankStatementReference = bankStatementReference,
            ExternalReference = externalReference,
            StatusCode = "DRAFT",
            Notes = notes,
        };
        db.Add(txn);
        await db.SaveChangesAsync(cancellationToken);
        txn.RepaymentNumber = $"RPT-{GlTime.Today(clock):yyyyMM}-{txn.RepaymentTxnId:D8}";
        await db.SaveChangesAsync(cancellationToken);

        if (method != "BANK_TRANSFER") return txn;

        db.Add(new FctRepaymentSource
        {
            RepaymentTxnId = txn.RepaymentTxnId,
            SourceType = "BANK_TRANSFER",
            SourceGlAccountId = SourceGlAccountId,
            SourceReference = bankStatementReference ?? externalReference,
            SourceAmount = receiptAmount,
            AllocatedAmount = 0,
        });
        var request = new FctRepaymentRequest
        {
            ClientId = clientId,
            ChequesCount = 0,
            ChequesAmount = 0,
            RemainingAmount = receiptAmount,
            RepaymentTranNo = txn.RepaymentTxnId,
            Source = "BANK_TRANSFER",
            BankTransfer = receiptAmount,
            CreditBalance = 0,
        };
        db.Add(request);
        await db.SaveChangesAsync(cancellationToken);
        txn.AgreementId = request.RepaymentRequestId;
        await db.SaveChangesAsync(cancellationToken);
        await StartWorkflowAsync(request, receiptAmount, cancellationToken);
        return txn;
    }

    /// <summary>update_cheque_status: the bank's result on one cheque, kept in CHQ_CHEQUE_STATUS_HIST.</summary>
    private async Task UpdateChequeStatusAsync(
        long chequeId, string newStatus, DateOnly statusDate, string? bankStatementReference, string? reasonCode, string? notes,
        CancellationToken cancellationToken)
    {
        if (newStatus is not ("COLLECTED" or "BOUNCED" or "REJECTED"))
            throw new BusinessRuleException("Cheque result must be COLLECTED, BOUNCED or REJECTED.");
        var rejected = newStatus is "BOUNCED" or "REJECTED";
        if (rejected && reasonCode is null && notes is null)
            throw new BusinessRuleException("A reason code or note is required for a bounced/rejected cheque.");

        var cheque = await db.Set<ChqCheque>().FirstOrDefaultAsync(x => x.ChequeId == chequeId, cancellationToken)
            ?? throw new BusinessRuleException($"Cheque {chequeId} was not found.");
        var oldStatus = cheque.StatusCode;
        if (oldStatus is "CANCELLED" or "REPLACED" or "WITHDRAWN")
            throw new BusinessRuleException($"Cheque {chequeId} cannot be reconciled from status {oldStatus}.");
        if (oldStatus == newStatus) return;
        if (oldStatus == "COLLECTED" && rejected && await ActiveSourceExistsAsync(chequeId, null, cancellationToken))
            throw new BusinessRuleException("Reverse the linked repayment before changing this collected cheque to bounced/rejected.");

        var at = DateTime.SpecifyKind(statusDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        cheque.StatusCode = newStatus;
        if (newStatus == "COLLECTED") cheque.CollectionAt = at;
        cheque.RejectionAt = rejected ? at : null;
        cheque.RejectionReasonCode = rejected ? reasonCode : null;
        cheque.RejectionNotes = rejected ? notes : null;
        cheque.LastEventAt = clock.UtcNow;
        db.Add(new ChqChequeStatusHist
        {
            ChequeId = chequeId,
            OldStatusCode = oldStatus,
            NewStatusCode = newStatus,
            StatusDate = statusDate,
            BankStatementReference = Cut(bankStatementReference, 100),
            ReasonCode = Cut(reasonCode, 50),
            Notes = Cut(notes, 2000),
            ChangedBy = User,
            ChangedAt = clock.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>create_cheque_payment_sources: one collected cheque becomes a source of a CHEQUE transaction.</summary>
    private async Task CreateChequePaymentSourceAsync(FctRepaymentTransaction txn, long chequeId, CancellationToken cancellationToken)
    {
        if (txn.RepaymentMethod != "CHEQUE") throw new BusinessRuleException("Cheque sources require repayment method CHEQUE.");
        if (await db.Set<FctRepaymentAllocation>().AnyAsync(x => x.RepaymentTxnId == txn.RepaymentTxnId && x.StatusCode == "ACTIVE", cancellationToken))
            throw new BusinessRuleException("Cheque sources cannot be changed after allocation.");

        var cheque = await db.Set<ChqCheque>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.ChequeId == chequeId && x.StatusCode == "COLLECTED" && x.LegalEntityId == txn.LegalEntityId, cancellationToken)
            ?? throw new BusinessRuleException($"Cheque ID={chequeId} was not found, not collected, or belongs to another legal entity.");

        var disbursement = cheque.SourceRefId is null
            ? null
            : await db.Set<FctDisbursement>().AsNoTracking().Where(d => d.DisbursementId.ToString() == cheque.SourceRefId)
                .Select(d => new { d.CurrencyId, d.ClientId }).FirstOrDefaultAsync(cancellationToken);

        var clientText = txn.ClientId.ToString(CultureInfo.InvariantCulture);
        if ((cheque.CustomerId is not null && cheque.CustomerId != clientText) || (disbursement is not null && disbursement.ClientId != txn.ClientId))
            throw new BusinessRuleException($"Cheque {cheque.ChequeRef} belongs to another client.");
        if (txn.CurrencyCode is not null && cheque.CurrencyCode != txn.CurrencyCode)
            throw new BusinessRuleException("Cheque currency must match repayment currency.");
        if (txn.CurrencyId is not null && disbursement is not null && disbursement.CurrencyId != txn.CurrencyId)
            throw new BusinessRuleException("Cheque currency ID must match repayment currency ID.");
        if (await ActiveSourceExistsAsync(chequeId, txn.RepaymentTxnId, cancellationToken))
            throw new BusinessRuleException($"Cheque {cheque.ChequeRef} is already linked to another repayment.");

        db.Add(new FctRepaymentSource
        {
            RepaymentTxnId = txn.RepaymentTxnId,
            SourceType = "CHEQUE",
            ChequeId = chequeId,
            SourceReference = cheque.ChequeRef,
            SourceAmount = cheque.ChequeAmount,
            AllocatedAmount = 0,
        });
        await db.SaveChangesAsync(cancellationToken);

        txn.ReceiptAmount = await db.Set<FctRepaymentSource>()
            .Where(x => x.RepaymentTxnId == txn.RepaymentTxnId && x.SourceType == "CHEQUE" && x.StatusCode == "ACTIVE")
            .SumAsync(x => x.SourceAmount, cancellationToken);
        txn.CurrencyId ??= disbursement?.CurrencyId;
        txn.CurrencyCode ??= cheque.CurrencyCode;
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>An ACTIVE source on a transaction that is not REVERSED or CANCELLED already uses the cheque.</summary>
    private Task<bool> ActiveSourceExistsAsync(long chequeId, long? exceptTxnId, CancellationToken cancellationToken) =>
        (from s in db.Set<FctRepaymentSource>()
         join t in db.Set<FctRepaymentTransaction>() on s.RepaymentTxnId equals t.RepaymentTxnId
         where s.ChequeId == chequeId && s.StatusCode == "ACTIVE" && t.StatusCode != "REVERSED" && t.StatusCode != "CANCELLED"
               && (exceptTxnId == null || t.RepaymentTxnId != exceptTxnId)
         select s).AnyAsync(cancellationToken);

    /// <summary>
    /// wfe_operations_pkg.submit_repayment_request: starts CUSTOMER_REPAYMENT_WORKFLOW for the request, when that workflow
    /// is defined in Capitra (its only step opened App 148 page 713, which is not ported).
    /// </summary>
    private async Task StartWorkflowAsync(FctRepaymentRequest request, decimal amount, CancellationToken cancellationToken)
    {
        if (!await db.Set<WfeWorkflowDefinition>().AnyAsync(x => x.WorkflowCode == WorkflowCode, cancellationToken)) return;
        var reference = $"REPAY-{request.RepaymentRequestId}";
        var variables = new Dictionary<string, string?>
        {
            ["entity_id"] = request.RepaymentRequestId.ToString(CultureInfo.InvariantCulture),
            ["entity_type"] = "REPAYMENT_REQUEST",
            ["entity_ref"] = reference,
            ["entity_name"] = $"Repayment Request - {reference}",
            ["client_id"] = request.ClientId?.ToString(CultureInfo.InvariantCulture),
            ["amount"] = amount.ToString("0.00", CultureInfo.InvariantCulture),
            ["currency_code"] = CurrencyCode,
            ["module_code"] = "FACTORING",
        };
        await runtime.StartAsync(WorkflowCode, reference, variables, User, cancellationToken);
    }

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? Cut(string? value, int max) => value is null || value.Length <= max ? value : value[..max];
}
