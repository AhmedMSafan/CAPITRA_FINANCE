using System.Globalization;
using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Gl;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Chq;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Crm;

/// <summary>App 148 page 705 "Disbursement Request Detail" region: the disbursement terms.</summary>
public sealed record CrmDisbursementTermsDto(
    string DisbursementNumber, DateOnly? DisbursementDate, long? ClientId, long? LimitId, long? CurrencyId, long? ProductId, decimal? GrossAmount,
    decimal? AdvanceRatePct, int? TenorMonth, decimal? IndexRate, decimal? SpreadRate, string? BaseDayCount, DateOnly? RepaymentStartDate,
    string? PrincipalRepaymentScheduleType, string? PrincipalRepaymentMethod, string? PrincipalDistributionMethod, string? InterestRepaymentMethod,
    string? PaymentMethod, string? Remarks);

/// <summary>Page 705 "Invoice Batch Details" region.</summary>
public sealed record CrmInvoiceBatchDto(
    long? BatchId, string? BatchNumber, string? BatchName, long? ClientId, DateOnly? BatchDate, string? BatchType, long? CurrencyId,
    decimal? TotalNetAmount, string? Remarks);

/// <summary>Page 705 "Invoice Entry" grid row (FCT_INVOICES of the batch).</summary>
public sealed record CrmInvoiceRowDto(
    long InvoiceId, string InvoiceNumber, DateOnly? ClientInvoiceDate, DateOnly? DueDate, decimal GrossAmount, decimal? AdvanceRatePct,
    decimal? AdvanceAmount, string? StatusCode, string? IneligibleReason);

/// <summary>Page 705 "Cheques Entry" grid row (FND_CHEQUE of the batch, CHQ_CHEQUES here).</summary>
public sealed record CrmChequeRowDto(
    long ChequeId, string ChequeNo, DateOnly ChequeDate, DateOnly DueDate, string IssuerBankName, string DrawerNameEn, long? DepositBankAccountId,
    decimal ChequeAmount, string CurrencyCode, string? SourceModule, DateOnly ReceivedDate, DateOnly? DepositDate, string StatusCode);

/// <summary>Page 705 "Summary" region: the client's limit and how much of it the batch uses.</summary>
public sealed record CrmInvoiceSummaryDto(
    string LimitNumber, string ClientName, decimal ApprovedLimit, decimal AvailableAmount, string CurrencyCode, decimal TotalInvoices,
    decimal ApprovedInvoiced, decimal AdvanceRate, decimal UtilizationAmount, int UtilizationPercent);

public sealed record CrmInvoiceEntryOptionsDto(
    IReadOnlyList<CrmOptionDto> Clients, IReadOnlyList<CrmOptionDto> Limits, IReadOnlyList<CrmOptionDto> Currencies, IReadOnlyList<CrmOptionDto> Products);

public sealed record CrmInvoiceEntryDto(
    long DisbursementId, CrmDisbursementTermsDto Terms, CrmInvoiceBatchDto Batch, CrmInvoiceSummaryDto Summary, IReadOnlyList<CrmInvoiceRowDto> Invoices,
    IReadOnlyList<CrmChequeRowDto> Cheques, CrmInvoiceEntryOptionsDto Options);

/// <summary>Apply Changes: both forms and the changed or deleted grid rows.</summary>
public sealed record CrmInvoiceEntrySaveDto(
    CrmDisbursementTermsDto Terms, CrmInvoiceBatchDto Batch, IReadOnlyList<CrmInvoiceRowDto>? Invoices, IReadOnlyList<long>? DeletedInvoiceIds,
    IReadOnlyList<CrmChequeRowDto>? Cheques, IReadOnlyList<long>? DeletedChequeIds);

/// <summary>An uploaded file: its name, MIME type and bytes.</summary>
public sealed record CrmUploadFile(string FileName, string? MimeType, byte[] Content);

public sealed record CrmInvoiceUploadResultDto(CrmInvoiceEntryDto Page, string Message);

/// <summary>
/// The Invoice Upload step of DISBURSEMENT_REQUEST_WF: APEX App 148 page 705 "Invoice Entry &amp; Verification", its terms form, batch
/// header, invoice and cheque uploads (IMPORT_INV_DATA_NEW, FNV_DISBURSEMENT_PROCESS_PKG.sync_invoice_advances / import_cheques) and
/// the two grids. Process Disbursement Request (charges, repayment schedule, GL claim) is not part of this step yet.
/// </summary>
public sealed class CrmInvoiceEntryService(ICapitraDbContext db, ICurrentUser currentUser, IClock clock, IWorkbookReader workbooks)
{
    private const long LegalEntityId = 1;

    private string User => currentUser.UserName ?? "SYSTEM";

    public async Task<CrmInvoiceEntryDto> GetAsync(long disbursementId, CancellationToken cancellationToken)
    {
        var d = await db.Set<FctDisbursement>().AsNoTracking().FirstOrDefaultAsync(x => x.DisbursementId == disbursementId, cancellationToken)
            ?? throw new NotFoundException($"Disbursement request {disbursementId} was not found.");
        var batch = await BatchOfAsync(disbursementId, true, cancellationToken);
        var today = GlTime.Today(clock);

        // INITIALIZE_PAGE_VALUES: the index rate of rate index 1, the business date and the method defaults (here only where none is saved)
        var indexRate = await db.Set<AdmRateIndexValue>().AsNoTracking()
            .Where(x => x.RateIndexId == 1 && x.EffectiveToDate == null).Select(x => (decimal?)x.AnnualRatePct).FirstOrDefaultAsync(cancellationToken);
        var terms = new CrmDisbursementTermsDto(
            d.DisbursementNumber, d.DisbursementDate, d.ClientId, d.LimitId, d.CurrencyId, d.ProductId ?? 1, d.GrossAmount, d.AdvanceRatePct, d.TenorMonth,
            indexRate ?? d.IndexRate, d.SpreadRate, d.BaseDayCountBasis ?? "ACTUAL/360", d.RepaymentStartDate ?? today,
            d.PrincipalRepaymentScheduleType ?? "NORMAL_SCHEDULE", d.PrincipalRepaymentMethod ?? "AT_MATURITY",
            d.PrincipalDistributionMethod ?? "INVOICE_AMOUNT", d.InterestRepaymentMethod ?? "MONTHLY",
            d.PaymentMethod ?? "TRANSFER",
            d.Remarks);

        var batchDto = batch is null
            ? new CrmInvoiceBatchDto(null, null, null, d.ClientId, today, "BULK", d.CurrencyId, null, null)
            : new CrmInvoiceBatchDto(batch.BatchId, batch.BatchNumber, batch.BatchName, batch.ClientId, batch.BatchDate, batch.BatchType, batch.CurrencyId,
                batch.TotalNetAmount, batch.Remarks);

        var invoices = new List<CrmInvoiceRowDto>();
        var cheques = new List<CrmChequeRowDto>();
        if (batch is not null)
        {
            var invoiceRows = await db.Set<FctInvoice>().AsNoTracking().Where(x => x.BatchId == batch.BatchId).OrderBy(x => x.InvoiceId)
                .ToListAsync(cancellationToken);
            invoices = invoiceRows.Select(x => new CrmInvoiceRowDto(
                x.InvoiceId, x.InvoiceNumber, DateOf(x.ClientInvoiceAt), DateOf(x.DueAt), x.GrossAmount, x.AdvanceRatePct, x.AdvanceAmount, x.StatusCode,
                x.IneligibleReason)).ToList();
            var chequeRows = await Cheques(batch.BatchId, disbursementId).AsNoTracking().OrderBy(x => x.ChequeId).ToListAsync(cancellationToken);
            cheques = chequeRows.Select(x => new CrmChequeRowDto(
                x.ChequeId, x.ChequeNo, DateOnly.FromDateTime(x.ChequeAt), x.DueDate, x.IssuerBankName, x.DrawerNameEn, x.DepositBankAccountId, x.ChequeAmount,
                x.CurrencyCode, x.SourceModule, DateOnly.FromDateTime(x.ReceivedAt), DateOf(x.DepositAt), x.StatusCode)).ToList();
        }

        return new CrmInvoiceEntryDto(disbursementId, terms, batchDto, await SummaryAsync(d, cancellationToken), invoices, cheques,
            await OptionsAsync(d.ClientId, cancellationToken));
    }

    /// <summary>The Summary of the client chosen in the terms form (Refresh Summary), before it is saved.</summary>
    public async Task<CrmInvoiceSummaryDto> SummaryAsync(long disbursementId, long? clientId, long? limitId, CancellationToken cancellationToken)
    {
        var d = await db.Set<FctDisbursement>().AsNoTracking().FirstOrDefaultAsync(x => x.DisbursementId == disbursementId, cancellationToken)
            ?? throw new NotFoundException($"Disbursement request {disbursementId} was not found.");
        if (clientId is { } c) d.ClientId = c;
        if (limitId is { } l)
            d.AgreementId = await db.Set<FctCreditLimit>().AsNoTracking().Where(x => x.CreditLimitId == l).Select(x => x.AgreementId).FirstOrDefaultAsync(cancellationToken);
        return await SummaryAsync(d, cancellationToken);
    }

    /// <summary>The client's limits (P705_LIMIT_ID) and SET_ADV_PCT's advance rate of a limit.</summary>
    public async Task<IReadOnlyList<CrmOptionDto>> LimitsAsync(long clientId, CancellationToken cancellationToken) =>
        (await OptionsAsync(clientId, cancellationToken)).Limits;

    public async Task<decimal?> LimitAdvanceRateAsync(long limitId, CancellationToken cancellationToken) =>
        await db.Set<FctCreditLimit>().AsNoTracking().Where(x => x.CreditLimitId == limitId).Select(x => x.AdvanceRatePct).FirstOrDefaultAsync(cancellationToken);

    /// <summary>Apply Changes: the two forms (Process form ...), then the Invoice Entry and Cheques Entry grids.</summary>
    public async Task<CrmInvoiceEntryDto> SaveAsync(long disbursementId, CrmInvoiceEntrySaveDto dto, CancellationToken cancellationToken)
    {
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var d = await SaveFormsAsync(disbursementId, dto.Terms, dto.Batch, cancellationToken);
        var batchId = (await BatchOfAsync(disbursementId, false, cancellationToken))?.BatchId;
        if (batchId is { } id)
        {
            await SaveInvoicesAsync(id, dto.Invoices ?? [], dto.DeletedInvoiceIds ?? [], cancellationToken);
            await SaveChequesAsync(id, d.DisbursementId, dto.Cheques ?? [], dto.DeletedChequeIds ?? [], cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return await GetAsync(disbursementId, cancellationToken);
    }

    /// <summary>
    /// Upload Invoices: the forms, SAVE_BATCH_HEADER_FOR_UPLOAD, then UPLOAD_CSV_FILES (the invoices replace the request's invoices and take
    /// the advance rate; the cheques are added to the batch), and the batch totals.
    /// </summary>
    public async Task<CrmInvoiceUploadResultDto> UploadAsync(
        long disbursementId, CrmInvoiceEntrySaveDto dto, CrmUploadFile? invoiceFile, CrmUploadFile? chequeFile, CancellationToken cancellationToken)
    {
        if (invoiceFile is null && chequeFile is null)
            throw new BusinessRuleException("Select at least one invoice or cheque XLSX/CSV file.");

        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var d = await SaveFormsAsync(disbursementId, dto.Terms, dto.Batch, cancellationToken);
        var batch = await BatchOfAsync(disbursementId, false, cancellationToken)
            ?? throw new BusinessRuleException("Batch ID and Disbursement ID are required before upload.");

        if (invoiceFile is not null)
        {
            await ImportInvoicesAsync(d, batch, invoiceFile, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await SyncInvoiceAdvancesAsync(d, batch.BatchId, cancellationToken);
        }

        if (chequeFile is not null)
            await ImportChequesAsync(d, batch.BatchId, chequeFile, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var invoices = await db.Set<FctInvoice>().Where(x => x.BatchId == batch.BatchId).Select(x => new { x.GrossAmount, x.NetAmount })
            .ToListAsync(cancellationToken);
        var chequeCount = await Cheques(batch.BatchId, disbursementId).CountAsync(cancellationToken);
        var files = new[] { invoiceFile, chequeFile }.Where(x => x is not null).Select(x => x!).ToList();
        batch.TotalInvoices = invoices.Count;
        batch.TotalGrossAmount = invoices.Sum(x => x.GrossAmount);
        batch.TotalNetAmount = invoices.Sum(x => x.NetAmount ?? 0);
        var fileName = string.Join("; ", files.Select(x => x.FileName));
        batch.UploadFileName = fileName.Length > 255 ? fileName[..255] : fileName;
        batch.UploadFileSize = files.Sum(x => x.Content.Length);
        batch.UploadFileType = files.Select(x => x.MimeType).FirstOrDefault(x => x is not null) ?? batch.UploadFileType;
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return new CrmInvoiceUploadResultDto(await GetAsync(disbursementId, cancellationToken),
            $"Upload completed. Invoices: {invoices.Count}, Cheques: {chequeCount}.");
    }

    private async Task<FctDisbursement> SaveFormsAsync(long disbursementId, CrmDisbursementTermsDto t, CrmInvoiceBatchDto b, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        if (t.DisbursementDate is null) errors["terms.disbursementDate"] = ["Disbursement Date must have some value."];
        if (t.ClientId is null) errors["terms.clientId"] = ["Client Id must have some value."];
        if (t.CurrencyId is null) errors["terms.currencyId"] = ["Currency Id must have some value."];
        if (t.GrossAmount is null) errors["terms.grossAmount"] = ["Invoices Gross Amount must have some value."];
        if (string.IsNullOrWhiteSpace(b.BatchNumber)) errors["batch.batchNumber"] = ["Batch Number must have some value."];
        if (b.ClientId is null) errors["batch.clientId"] = ["Client Id must have some value."];
        if (b.BatchDate is null) errors["batch.batchDate"] = ["Batch Date must have some value."];
        if (string.IsNullOrWhiteSpace(b.BatchType)) errors["batch.batchType"] = ["Batch Type must have some value."];
        if (b.CurrencyId is null) errors["batch.currencyId"] = ["Currency Id must have some value."];
        if (errors.Count > 0) throw new BusinessValidationException(errors);

        var d = await db.Set<FctDisbursement>().FirstOrDefaultAsync(x => x.DisbursementId == disbursementId, cancellationToken)
            ?? throw new NotFoundException($"Disbursement request {disbursementId} was not found.");
        if (t.LimitId != d.LimitId && t.LimitId is { } limitId)
            d.AgreementId = await db.Set<FctCreditLimit>().AsNoTracking().Where(x => x.CreditLimitId == limitId).Select(x => x.AgreementId)
                .FirstOrDefaultAsync(cancellationToken);
        d.DisbursementDate = t.DisbursementDate!.Value;
        d.ClientId = t.ClientId!.Value;
        d.LimitId = t.LimitId;
        d.CurrencyId = t.CurrencyId!.Value;
        d.ProductId = t.ProductId;
        d.GrossAmount = t.GrossAmount!.Value;
        d.AdvanceRatePct = t.AdvanceRatePct;
        d.TenorMonth = t.TenorMonth;
        d.IndexRate = t.IndexRate;
        d.SpreadRate = t.SpreadRate;
        d.BaseDayCountBasis = t.BaseDayCount;
        d.RepaymentStartDate = t.RepaymentStartDate;
        d.PrincipalRepaymentScheduleType = t.PrincipalRepaymentScheduleType;
        d.PrincipalRepaymentMethod = t.PrincipalRepaymentMethod;
        d.PrincipalDistributionMethod = t.PrincipalDistributionMethod;
        d.InterestRepaymentMethod = t.InterestRepaymentMethod;
        d.PaymentMethod = t.PaymentMethod;
        d.Remarks = Blank(t.Remarks);

        // SAVE_BATCH_HEADER_FOR_UPLOAD: the request's batch, added when there is none
        var batch = await BatchOfAsync(disbursementId, false, cancellationToken);
        if (batch is null)
        {
            batch = new FctInvoiceBatch { DisbursementId = disbursementId, SubmissionAt = clock.UtcNow, SubmittedBy = User, SubmissionMethod = "FILE_UPLOAD", StatusId = 1, IsActive = true };
            db.Add(batch);
        }

        batch.BatchNumber = b.BatchNumber!.Trim();
        batch.BatchName = Blank(b.BatchName);
        batch.ClientId = b.ClientId!.Value;
        batch.AgreementId = d.AgreementId;
        batch.BatchDate = b.BatchDate!.Value;
        batch.BatchType = b.BatchType!.Trim();
        batch.CurrencyId = b.CurrencyId;
        batch.TotalNetAmount = b.TotalNetAmount;
        batch.Remarks = Blank(b.Remarks);
        await db.SaveChangesAsync(cancellationToken);
        return d;
    }

    private async Task SaveInvoicesAsync(long batchId, IReadOnlyList<CrmInvoiceRowDto> rows, IReadOnlyList<long> deleted, CancellationToken cancellationToken)
    {
        if (deleted.Count > 0)
            db.Set<FctInvoice>().RemoveRange(await db.Set<FctInvoice>().Where(x => x.BatchId == batchId && deleted.Contains(x.InvoiceId)).ToListAsync(cancellationToken));
        if (rows.Count == 0) return;
        var ids = rows.Select(x => x.InvoiceId).ToList();
        var invoices = await db.Set<FctInvoice>().Where(x => x.BatchId == batchId && ids.Contains(x.InvoiceId)).ToDictionaryAsync(x => x.InvoiceId, cancellationToken);
        foreach (var r in rows)
        {
            if (!invoices.TryGetValue(r.InvoiceId, out var i)) continue;
            if (string.IsNullOrWhiteSpace(r.InvoiceNumber)) throw BusinessValidationException.For("invoices", "Invoice Number must have some value.");
            if (r.DueDate is null) throw BusinessValidationException.For("invoices", $"Due Date must have some value ({r.InvoiceNumber}).");
            if (r.ClientInvoiceDate is null) throw BusinessValidationException.For("invoices", $"Client Invoice Date must have some value ({r.InvoiceNumber}).");
            if (string.IsNullOrWhiteSpace(r.StatusCode)) throw BusinessValidationException.For("invoices", $"Eligibility Status must have some value ({r.InvoiceNumber}).");
            i.InvoiceNumber = r.InvoiceNumber.Trim();
            i.ClientInvoiceAt = At(r.ClientInvoiceDate);
            i.DueAt = At(r.DueDate);
            i.GrossAmount = r.GrossAmount;
            i.AdvanceRatePct = r.AdvanceRatePct;
            i.AdvanceAmount = r.AdvanceAmount;
            i.StatusCode = r.StatusCode;
            i.IneligibleReason = Blank(r.IneligibleReason);
        }
    }

    private async Task SaveChequesAsync(long batchId, long disbursementId, IReadOnlyList<CrmChequeRowDto> rows, IReadOnlyList<long> deleted, CancellationToken cancellationToken)
    {
        if (deleted.Count > 0)
            db.Set<ChqCheque>().RemoveRange(await Cheques(batchId, disbursementId).Where(x => deleted.Contains(x.ChequeId)).ToListAsync(cancellationToken));
        if (rows.Count == 0) return;
        var ids = rows.Select(x => x.ChequeId).ToList();
        var cheques = await Cheques(batchId, disbursementId).Where(x => ids.Contains(x.ChequeId)).ToDictionaryAsync(x => x.ChequeId, cancellationToken);
        foreach (var r in rows)
        {
            if (!cheques.TryGetValue(r.ChequeId, out var c)) continue;
            if (string.IsNullOrWhiteSpace(r.ChequeNo) || string.IsNullOrWhiteSpace(r.IssuerBankName) || string.IsNullOrWhiteSpace(r.DrawerNameEn)
                || string.IsNullOrWhiteSpace(r.CurrencyCode) || string.IsNullOrWhiteSpace(r.StatusCode))
                throw BusinessValidationException.For("cheques", $"Every required cheque value must be filled ({r.ChequeNo}).");
            c.ChequeNo = r.ChequeNo.Trim();
            c.ChequeAt = At(r.ChequeDate)!.Value;
            c.DueDate = r.DueDate;
            c.IssuerBankName = r.IssuerBankName.Trim();
            c.DrawerNameEn = r.DrawerNameEn.Trim();
            c.DepositBankAccountId = r.DepositBankAccountId;
            c.ChequeAmount = r.ChequeAmount;
            c.CurrencyCode = r.CurrencyCode.Trim().ToUpperInvariant();
            c.SourceModule = Blank(r.SourceModule);
            c.ReceivedAt = At(r.ReceivedDate)!.Value;
            c.DepositAt = At(r.DepositDate);
            c.StatusCode = r.StatusCode.Trim().ToUpperInvariant();
        }
    }

    /// <summary>
    /// IMPORT_INV_DATA_NEW: the request's invoices are replaced by the file's rows (after the header): INV-line number, the debtor's tax
    /// number (column 2), invoice date (4), due date (5, else invoice date + 30) and gross amount (6), all ELIGIBLE.
    /// </summary>
    private async Task ImportInvoicesAsync(FctDisbursement d, FctInvoiceBatch batch, CrmUploadFile file, CancellationToken cancellationToken)
    {
        if (file.Content.Length == 0) throw new BusinessRuleException("Invoice upload file is empty.");
        db.Set<FctInvoice>().RemoveRange(await db.Set<FctInvoice>().Where(x => x.DisbursementId == d.DisbursementId).ToListAsync(cancellationToken));

        var now = clock.UtcNow;
        var added = 0;
        foreach (var row in Rows(file, 6).Where(r => r.RowNumber > 1))
        {
            var taxId = long.TryParse(new string((row[1] ?? string.Empty).Where(char.IsDigit).ToArray()), out var t) ? t : (long?)null;
            var invoiceDate = InvoiceDate(row[3]);
            var dueDate = InvoiceDate(row[4]);
            var gross = decimal.TryParse(new string((row[5] ?? string.Empty).Where(ch => char.IsDigit(ch) || ch is '.' or '-').ToArray()),
                NumberStyles.Number, CultureInfo.InvariantCulture, out var g) ? g : (decimal?)null;
            if (taxId is null || invoiceDate is null || gross is null) continue;

            var number = $"INV-{row.RowNumber}";
            db.Add(new FctInvoice
            {
                InvoiceNumber = number,
                ClientInvoiceNumber = number,
                ClientInvoiceAt = At(invoiceDate),
                ClientId = batch.ClientId,
                DebtorId = taxId,
                DisbursementId = d.DisbursementId,
                CurrencyId = batch.CurrencyId ?? d.CurrencyId,
                GrossAmount = gross.Value,
                NetAmount = 0,
                DueAt = At(dueDate ?? invoiceDate.Value.AddDays(30)),
                OutstandingAmount = 0,
                SubmissionAt = now,
                BatchId = batch.BatchId,
                StatusId = batch.StatusId ?? 1,
                StatusCode = "ELIGIBLE",
                IsRecourse = true,
                IsActive = true,
                CreditLimitId = d.LimitId,
            });
            added++;
        }

        if (added == 0)
            throw new BusinessRuleException("No invoice rows were imported. Check the CSV column mapping, required values, or duplicate invoice numbers.");
    }

    /// <summary>sync_invoice_advances: each invoice takes the request's advance rate, its advance and its reserve.</summary>
    private async Task SyncInvoiceAdvancesAsync(FctDisbursement d, long batchId, CancellationToken cancellationToken)
    {
        if (d.AdvanceRatePct is not { } rate || rate < 0 || rate > 100)
            throw new BusinessRuleException("ADVANCE_RATE_PCT must be between 0 and 100 on the disbursement request.");
        foreach (var i in await db.Set<FctInvoice>().Where(x => x.BatchId == batchId).ToListAsync(cancellationToken))
        {
            var advance = Math.Round(i.GrossAmount * rate / 100, 3, MidpointRounding.AwayFromZero);
            i.AdvanceRatePct = rate;
            i.AdvanceAmount = advance;
            i.ReserveAmount = Math.Round(i.GrossAmount - i.GrossAmount * rate / 100, 3, MidpointRounding.AwayFromZero);
        }
    }

    /// <summary>import_cheques: the CHEQUE_NO, CHEQUE_DATE, DUE_DATE, ISSUER_BANK_NAME ... template, each row a new RECEIVED cheque of the batch.</summary>
    private async Task ImportChequesAsync(FctDisbursement d, long batchId, CrmUploadFile file, CancellationToken cancellationToken)
    {
        if (file.Content.Length == 0) throw new BusinessRuleException("The cheque upload file is empty.");
        var rows = Rows(file, 18);
        var header = rows.FirstOrDefault(r => r.RowNumber == 1);
        string H(int c) => (header?[c] ?? string.Empty).Trim().ToUpperInvariant();
        if (H(0) != "CHEQUE_NO" || H(1) != "CHEQUE_DATE" || H(2) != "DUE_DATE" || H(3) != "ISSUER_BANK_NAME")
            throw new BusinessRuleException("Invalid cheque template. The first columns must be CHEQUE_NO, CHEQUE_DATE, DUE_DATE, ISSUER_BANK_NAME.");

        var legalEntityId = d.LegalEntityId ?? LegalEntityId;
        var added = new List<ChqCheque>();
        foreach (var r in rows.Where(r => r.RowNumber > 1 && !string.IsNullOrWhiteSpace(r[0])))
        {
            string? C(int c) => string.IsNullOrWhiteSpace(r[c]) ? null : r[c]!.Trim();
            var line = r.RowNumber;
            if (C(0) is null || C(3) is null || C(6) is null || C(10) is null || C(11) is null)
                throw new BusinessRuleException($"Missing mandatory cheque value on upload row {line}.");
            var chequeDate = ChequeDate(C(1));
            var dueDate = ChequeDate(C(2));
            var received = ChequeDate(C(15)) ?? GlTime.Today(clock);
            var amount = ChequeAmount(C(10)!);
            if (chequeDate is null || dueDate is null)
                throw new BusinessRuleException($"CHEQUE_DATE and DUE_DATE are mandatory on upload row {line}.");
            if (dueDate < chequeDate)
                throw new BusinessRuleException($"DUE_DATE cannot be before CHEQUE_DATE on upload row {line}.");
            if (amount <= 0)
                throw new BusinessRuleException($"CHEQUE_AMOUNT must be greater than zero on upload row {line}.");

            var chequeNo = C(0)!;
            var bankCode = C(4);
            if (await db.Set<ChqCheque>().AnyAsync(x => x.ChequeNo == chequeNo && (x.IssuerBankCode ?? "#") == (bankCode ?? "#"), cancellationToken)
                || added.Any(x => x.ChequeNo == chequeNo && x.IssuerBankCode == bankCode))
                throw new BusinessRuleException($"Duplicate cheque number/bank on upload row {line}: {chequeNo}.");

            var cheque = new ChqCheque
            {
                ChequeRef = $"CHQ-{d.DisbursementId}-TMP-{Guid.NewGuid():N}",
                ChequeNo = chequeNo,
                ChequeAt = At(chequeDate)!.Value,
                DueDate = dueDate.Value,
                IssuerBankName = C(3)!,
                IssuerBankCode = bankCode,
                IssuerBankBranch = C(5),
                DrawerNameEn = C(6)!,
                DrawerNameAr = C(7),
                DrawerNationalId = C(8),
                DrawerAccountNo = C(9),
                ChequeAmount = amount,
                CurrencyCode = C(11)!.ToUpperInvariant(),
                CustomerId = C(12),
                CustomerNameEn = C(13),
                CustomerNameAr = C(14),
                SourceModule = "FACTORING",
                SourceRefType = "DISBURSEMENT",
                SourceRefId = d.DisbursementId.ToString(CultureInfo.InvariantCulture),
                ReceivedAt = At(received)!.Value,
                StatusCode = C(16)?.ToUpperInvariant() ?? "RECEIVED",
                LegalEntityId = legalEntityId,
                Notes = C(17),
                BatchId = batchId,
            };
            db.Add(cheque);
            added.Add(cheque);
        }

        if (added.Count == 0) throw new BusinessRuleException("The cheque upload contains no data rows.");
        await db.SaveChangesAsync(cancellationToken);
        foreach (var c in added)
            c.ChequeRef = $"CHQ-{d.DisbursementId}-{c.ChequeId}";
    }

    private async Task<CrmInvoiceSummaryDto> SummaryAsync(FctDisbursement d, CancellationToken cancellationToken)
    {
        var batches = await db.Set<FctInvoiceBatch>().AsNoTracking()
            .Where(x => x.ClientId == d.ClientId && x.IsActive && x.DisbursementId == d.DisbursementId)
            .Select(x => new { x.TotalGrossAmount, x.ApprovedAmount }).ToListAsync(cancellationToken);
        var limit = await db.Set<FctCreditLimit>().AsNoTracking()
            .Where(x => x.ClientId == d.ClientId && x.AgreementId == d.AgreementId)
            .Select(x => new { x.LimitNumber, x.ApprovedAmount, x.AvailableAmount, x.AdvanceRatePct }).FirstOrDefaultAsync(cancellationToken);
        var clientName = await db.Set<CrmClient>().AsNoTracking().Where(x => x.ClientId == d.ClientId).Select(x => x.ClientName).FirstOrDefaultAsync(cancellationToken);
        if (limit is null || clientName is null)
            return new CrmInvoiceSummaryDto("LMT-0000", "No Client Data", 0, 0, "EGP", 0, 0, 0, 0, 0);

        var total = batches.Sum(x => x.TotalGrossAmount ?? 0);
        var used = batches.Sum(x => x.ApprovedAmount ?? 0);
        var rate = limit.AdvanceRatePct ?? 0;
        var percent = limit.ApprovedAmount > 0 ? (int)Math.Min(Math.Round(used / limit.ApprovedAmount * 100, MidpointRounding.AwayFromZero), 100) : 0;
        return new CrmInvoiceSummaryDto(limit.LimitNumber, clientName, limit.ApprovedAmount, limit.AvailableAmount ?? 0, "EGP", total,
            Math.Round(total * rate / 100, 2, MidpointRounding.AwayFromZero), rate, used, percent);
    }

    private async Task<CrmInvoiceEntryOptionsDto> OptionsAsync(long clientId, CancellationToken cancellationToken)
    {
        var clients = await db.Set<CrmClient>().AsNoTracking().OrderBy(x => x.ClientName).Select(x => new { x.ClientId, x.ClientName }).ToListAsync(cancellationToken);
        var limits = await db.Set<FctCreditLimit>().AsNoTracking().Where(x => x.ClientId == clientId && x.IsActive).OrderByDescending(x => x.AvailableAmount)
            .Select(x => new { x.CreditLimitId, x.AvailableAmount }).ToListAsync(cancellationToken);
        var currencies = await db.Set<CrmLkpCurrency>().AsNoTracking().OrderBy(x => x.CurrencyCode).Select(x => new { x.CurrencyId, x.CurrencyCode }).ToListAsync(cancellationToken);
        var products = await db.Set<AdmProduct>().AsNoTracking().Where(x => x.BusinessLineCode == "FACTORING").OrderBy(x => x.ProductNameEn)
            .Select(x => new { x.ProductId, x.ProductNameEn }).ToListAsync(cancellationToken);
        return new CrmInvoiceEntryOptionsDto(
            clients.Select(x => new CrmOptionDto(Id(x.ClientId), x.ClientName)).ToList(),
            limits.Select(x => new CrmOptionDto(Id(x.CreditLimitId), x.AvailableAmount is { } a ? Math.Round(a, 0, MidpointRounding.AwayFromZero).ToString("#,##0", CultureInfo.InvariantCulture) : string.Empty)).ToList(),
            currencies.Select(x => new CrmOptionDto(Id(x.CurrencyId), x.CurrencyCode)).ToList(),
            products.Select(x => new CrmOptionDto(Id(x.ProductId), x.ProductNameEn)).ToList());
    }

    /// <summary>Resolve Disbursement Context: the request's latest batch.</summary>
    private Task<FctInvoiceBatch?> BatchOfAsync(long disbursementId, bool readOnly, CancellationToken cancellationToken)
    {
        var set = readOnly ? db.Set<FctInvoiceBatch>().AsNoTracking() : db.Set<FctInvoiceBatch>();
        return set.Where(x => x.DisbursementId == disbursementId).OrderByDescending(x => x.BatchId).FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>The batch's cheques (FND_CHEQUE.BATCH_ID), only those uploaded for this request: CHQ_CHEQUES.BATCH_ID is shared with other batches.</summary>
    private IQueryable<ChqCheque> Cheques(long batchId, long disbursementId)
    {
        var id = disbursementId.ToString(CultureInfo.InvariantCulture);
        return db.Set<ChqCheque>().Where(x => x.BatchId == batchId && x.SourceRefType == "DISBURSEMENT" && x.SourceRefId == id);
    }

    /// <summary>APEX_DATA_PARSER.parse: the first sheet of an .xlsx file, or the lines of a .csv file, with their line numbers.</summary>
    private IReadOnlyList<WorkbookRow> Rows(CrmUploadFile file, int columns)
    {
        if (file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) || file.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            return Csv(file.Content, columns);
        try
        {
            using var stream = new MemoryStream(file.Content);
            return workbooks.ReadFirstSheet(stream, columns);
        }
        catch (InvalidDataException ex)
        {
            throw new BusinessRuleException($"{file.FileName}: {ex.Message}");
        }
    }

    private static List<WorkbookRow> Csv(byte[] content, int columns)
    {
        var text = Encoding.UTF8.GetString(content).TrimStart('﻿');
        var rows = new List<WorkbookRow>();
        var lineNo = 0;
        foreach (var line in text.Split('\n'))
        {
            lineNo++;
            var cells = new List<string?>();
            var cell = new StringBuilder();
            var quoted = false;
            for (var i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (quoted)
                {
                    if (ch == '"' && i + 1 < line.Length && line[i + 1] == '"') { cell.Append('"'); i++; }
                    else if (ch == '"') quoted = false;
                    else cell.Append(ch);
                }
                else if (ch == '"') quoted = true;
                else if (ch == ',') { cells.Add(cell.ToString()); cell.Clear(); }
                else if (ch != '\r') cell.Append(ch);
            }

            cells.Add(cell.ToString());
            if (cells.All(string.IsNullOrWhiteSpace)) continue;
            rows.Add(new WorkbookRow(lineNo, Enumerable.Range(0, columns)
                .Select(c => c < cells.Count && !string.IsNullOrWhiteSpace(cells[c]) ? cells[c]!.Trim() : null).ToArray()));
        }

        return rows;
    }

    /// <summary>IMPORT_INV_DATA_NEW's dates: YYYY-MM-DD or DD/MM/YYYY, anything else none.</summary>
    private static DateOnly? InvoiceDate(string? value)
    {
        var v = value?.Trim();
        if (v is null) return null;
        if (DateOnly.TryParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var a)) return a;
        if (DateOnly.TryParseExact(v, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var b)) return b;
        return null;
    }

    /// <summary>import_cheques' parse_date: an Excel serial day, YYYY-MM-DD, DD/MM/YYYY or DD-MON-YYYY.</summary>
    private static DateOnly? ChequeDate(string? value)
    {
        var v = value?.Trim();
        if (string.IsNullOrEmpty(v)) return null;
        if (decimal.TryParse(v, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var n) && n is >= 20000 and <= 100000)
            return new DateOnly(1899, 12, 30).AddDays((int)Math.Truncate(n));
        foreach (var format in new[] { "yyyy-MM-dd", "dd/MM/yyyy", "dd-MMM-yyyy" })
            if (DateOnly.TryParseExact(v, format, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var d))
                return d;
        throw new BusinessRuleException($"Invalid date in cheque upload: \"{value}\". Use YYYY-MM-DD or DD/MM/YYYY.");
    }

    /// <summary>import_cheques' parse_number: commas and spaces dropped.</summary>
    private static decimal ChequeAmount(string value) =>
        decimal.TryParse(value.Replace(",", string.Empty).Replace(" ", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out var n)
            ? n
            : throw new BusinessRuleException($"Invalid numeric value in cheque upload: \"{value}\".");

    private static string Id(long id) => id.ToString(CultureInfo.InvariantCulture);

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateOnly? DateOf(DateTime? value) => value is { } v ? DateOnly.FromDateTime(v) : null;

    private static DateTime? At(DateOnly? value) => value?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
}
