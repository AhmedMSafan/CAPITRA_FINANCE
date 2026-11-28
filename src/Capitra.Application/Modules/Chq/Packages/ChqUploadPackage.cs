using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl;
using Capitra.Domain.Modules.Chq;
using Capitra.Domain.Modules.Crm;
using Microsoft.EntityFrameworkCore;

// UPPER(TRIM(x)) = UPPER(TRIM(y)) of the PL/SQL is Trim().ToUpper() in the queries, so PostgreSQL runs it as written.
#pragma warning disable CA1862

namespace Capitra.Application.Modules.Chq.Packages;

/// <summary>The batch of create_batch: what the upload knows of the customer and its defaults.</summary>
public sealed record ChqNewBatch
{
    public string? FileName { get; init; }
    public string? MimeType { get; init; }
    public long? LegalEntityId { get; init; }
    public string? CustomerId { get; init; }
    public string? CustomerNameEn { get; init; }
    public string? CustomerNameAr { get; init; }
    public string? DrawerNameEn { get; init; }
    public string? DrawerNameAr { get; init; }
    public string? CurrencyCode { get; init; } = "EGP";
    public string? SourceModule { get; init; } = "FACTORING";
    public string? SourceRefType { get; init; } = "INVOICE";
    public string? SourceRefId { get; init; }
    public DateTime? ChequeAt { get; init; }
    public DateTime? ReceivedAt { get; init; }
    public string? DefaultLocationCode { get; init; } = "COMPANY_VAULT";
    public string? DefaultCustodianUser { get; init; }
    public string? Notes { get; init; }
    public string? CreatedBy { get; init; }
}

/// <summary>
/// FND_CHEQUE_UPLOAD_PKG: cheques come in as an Excel file of one customer. check_file runs every check on the file
/// (nothing is saved); import_file creates the batch and its lines and validates them; submit_batch turns the valid
/// lines into cheques of the wallet, each with its first RECEIVED event.
/// </summary>
public sealed partial class ChqUploadPackage(
    ICapitraDbContext db, ICurrentUser user, IClock clock, IWorkbookReader workbook, ChqEventPackage events, ProductActionPosting posting)
{
    private const int MaxIssues = 15;

    private DateTime Now => DateTime.SpecifyKind(clock.UtcNow, DateTimeKind.Utc);

    private DateTime Today => DateTime.SpecifyKind(clock.UtcNow.Date, DateTimeKind.Utc);

    /// <summary>clean_value: line breaks as spaces, trimmed, empty as null.</summary>
    public static string? CleanValue(string? value)
    {
        var v = value?.Replace('\r', ' ').Replace('\n', ' ').Trim(' ');
        return string.IsNullOrEmpty(v) ? null : v;
    }

    /// <summary>parse_number: without thousands separators, at most 18 digits and 4 decimals; null when it is not a number.</summary>
    public static decimal? ParseNumber(string? value)
    {
        var v = CleanValue(value)?.Replace(",", string.Empty, StringComparison.Ordinal);
        if (v is null || !NumberText().IsMatch(v))
            return null;
        return decimal.Parse(v, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// parse_excel_date: an Excel serial day number, else DD/MM/YYYY, DD-MM-YYYY, YYYY-MM-DD or MM/DD/YYYY (Oracle takes
    /// any separator for / and -); null when none fits.
    /// </summary>
    public static DateOnly? ParseExcelDate(string? value)
    {
        var v = CleanValue(value);
        if (v is null)
            return null;
        if (SerialDay().IsMatch(v))
        {
            var days = Math.Truncate(decimal.Parse(v, CultureInfo.InvariantCulture));
            return days > 2_900_000 ? null : new DateOnly(1899, 12, 30).AddDays((int)days);
        }

        var dmy = DayMonthYear().Match(v);
        if (dmy.Success && Make(dmy.Groups[3].Value, dmy.Groups[2].Value, dmy.Groups[1].Value) is { } d1)
            return d1;
        var ymd = YearMonthDay().Match(v);
        if (ymd.Success && Make(ymd.Groups[1].Value, ymd.Groups[2].Value, ymd.Groups[3].Value) is { } d2)
            return d2;
        if (dmy.Success && Make(dmy.Groups[3].Value, dmy.Groups[1].Value, dmy.Groups[2].Value) is { } d3)
            return d3;
        return null;

        static DateOnly? Make(string y, string m, string d)
        {
            int yy = int.Parse(y, CultureInfo.InvariantCulture), mm = int.Parse(m, CultureInfo.InvariantCulture), dd = int.Parse(d, CultureInfo.InvariantCulture);
            return yy is >= 1 and <= 9999 && mm is >= 1 and <= 12 && dd >= 1 && dd <= DateTime.DaysInMonth(yy, mm) ? new DateOnly(yy, mm, dd) : null;
        }
    }

    /// <summary>get_bank_name: the English name of an active issuer bank, null when it is not one.</summary>
    public async Task<string?> GetBankNameAsync(string? bankCode, CancellationToken ct)
    {
        var code = bankCode?.Trim(' ').ToUpperInvariant();
        return await db.Set<ChqLkpIssuerBank>().Where(b => b.BankCode == code && b.IsActive).Select(b => b.BankNameEn).FirstOrDefaultAsync(ct);
    }

    /// <summary>make_cheque_ref: FCT-CHQ-&lt;year&gt;-&lt;id in 10 digits&gt;.</summary>
    public string MakeChequeRef(long chequeId) => "FCT-CHQ-" + Now.ToString("yyyy", CultureInfo.InvariantCulture) + "-" + chequeId.ToString("D10", CultureInfo.InvariantCulture);

    /// <summary>refresh_batch_counts: the line counts of the batch and its status from them.</summary>
    public async Task RefreshBatchCountsAsync(long batchId, CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
        var lines = await db.Set<ChqUploadLine>().Where(l => l.BatchId == batchId).Select(l => l.LineStatus).ToListAsync(ct);
        int total = lines.Count, valid = lines.Count(s => s == "VALID"), errors = lines.Count(s => s == "ERROR"), sub = lines.Count(s => s == "SUBMITTED");
        var b = await db.Set<ChqUploadBatch>().FirstOrDefaultAsync(x => x.BatchId == batchId, ct);
        if (b is null)
            return;
        b.TotalRows = total;
        b.ValidRows = valid;
        b.ErrorRows = errors;
        b.SubmittedRows = sub;
        b.BatchStatus = total == 0 ? "DRAFT" : sub == total ? "SUBMITTED" : errors > 0 ? "HAS_ERRORS" : "VALIDATED";
        await db.SaveChangesAsync(ct);
    }

    /// <summary>create_batch: a new DRAFT batch, numbered CHQ-UP-&lt;date&gt;-&lt;id in 6 digits&gt;; returns its id.</summary>
    public async Task<long> CreateBatchAsync(ChqNewBatch n, CancellationToken ct)
    {
        if (n.LegalEntityId is null)
            throw new ApexProcessException("ORA-20101: Legal Entity is required.");
        if (n.CurrencyCode is null)
            throw new ApexProcessException("ORA-20101: Currency is required.");
        var chequeAt = n.ChequeAt ?? throw new ApexProcessException("ORA-20102: Cheque Date is required.");
        var receivedAt = n.ReceivedAt ?? throw new ApexProcessException("ORA-20103: Received Date is required.");
        if ((n.SourceModule ?? "FACTORING") is not ("LEASING" or "FACTORING" or "DIRECT" or "MORTGAGE" or "OTHER"))
            throw new ApexProcessException("ORA-20104: Invalid source module.");

        var id = await events.NextIdAsync("CHQ_UPLOAD_BATCHES", "BATCH_ID", ct);
        db.Set<ChqUploadBatch>().Add(new ChqUploadBatch
        {
            BatchId = id,
            BatchNo = "CHQ-UP-" + Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "-" + id.ToString("D6", CultureInfo.InvariantCulture),
            BatchStatus = "DRAFT",
            FileName = n.FileName,
            MimeType = n.MimeType,
            LegalEntityId = n.LegalEntityId.Value,
            CustomerId = n.CustomerId,
            CustomerNameEn = n.CustomerNameEn,
            CustomerNameAr = n.CustomerNameAr,
            DrawerNameEn = n.DrawerNameEn,
            DrawerNameAr = n.DrawerNameAr,
            CurrencyCode = n.CurrencyCode,
            SourceModule = n.SourceModule ?? "FACTORING",
            SourceRefType = n.SourceRefType,
            SourceRefId = n.SourceRefId,
            ChequeAt = chequeAt,
            ReceivedAt = receivedAt,
            DefaultLocationCode = n.DefaultLocationCode,
            DefaultCustodianUser = n.DefaultCustodianUser,
            Notes = n.Notes,
        });
        await db.SaveChangesAsync(ct);
        return id;
    }

    /// <summary>validate_batch: reads every line's raw values and marks it VALID or ERROR with what is wrong.</summary>
    public async Task<(int Valid, int Errors)> ValidateBatchAsync(long? batchId, CancellationToken ct)
    {
        if (batchId is null)
            throw new ApexProcessException("ORA-20120: Batch ID is required.");
        var batch = await db.Set<ChqUploadBatch>().FirstOrDefaultAsync(b => b.BatchId == batchId, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        if (batch.BatchStatus == "SUBMITTED")
            throw new ApexProcessException("ORA-20121: Submitted batch cannot be validated.");

        var lines = await db.Set<ChqUploadLine>().Where(l => l.BatchId == batchId).OrderBy(l => l.LineNo).ToListAsync(ct);
        static string? Key(string? v) => v?.Trim(' ').ToUpperInvariant();
        foreach (var r in lines)
        {
            string? error = null;
            var due = ParseExcelDate(r.RawDueDateText);
            var amount = ParseNumber(r.RawChequeAmountText);
            var chequeNo = CleanValue(r.RawChequeNo);
            var bankCode = CleanValue(r.RawIssuerBankCode)?.ToUpperInvariant();
            var bankName = await GetBankNameAsync(bankCode, ct);
            var clientId = CleanValue(r.RawClientId);
            var taxId = CleanValue(r.RawPartyTaxId) is { } t ? t[..Math.Min(20, t.Length)] : null;

            if (clientId is null)
                error += "Client_ID is required. ";
            if (batch.CustomerId is not null && clientId is not null && clientId != batch.CustomerId)
                error += "Client_ID in file does not match batch customer. ";
            if (chequeNo is null)
                error += "Cheque Number is required. ";
            if (due is null)
                error += "Invalid Cheque Due Date. ";
            if (amount is null or <= 0)
                error += "Cheque Amount must be greater than zero. ";
            if (bankCode is null)
                error += "Issuer Bank Code is required. ";
            else if (bankName is null)
                error += "Issuer Bank Code is not defined or inactive: " + bankCode + ". ";

            if (chequeNo is not null && bankCode is not null)
            {
                if (lines.Any(x => x.LineId != r.LineId && Key(x.RawChequeNo) is { } a && a == Key(r.RawChequeNo)
                                   && Key(x.RawIssuerBankCode) is { } b && b == Key(r.RawIssuerBankCode)))
                    error += "Duplicate cheque number/bank inside upload file. ";
                var no = Key(chequeNo);
                var bank = Key(bankCode ?? "#");
                var same = await db.Set<ChqCheque>().Where(c => c.ChequeNo.Trim().ToUpper() == no)
                    .Select(c => c.IssuerBankCode).ToListAsync(ct);
                if (same.Any(c => Key(c ?? "#") == bank))
                    error += "Cheque already exists in FND_CHEQUE. ";
            }

            r.ClientId = clientId;
            r.PartyTaxId = taxId;
            r.ChequeNo = chequeNo;
            r.DueDate = due;
            r.ChequeAmount = amount;
            r.IssuerBankCode = bankCode;
            r.IssuerBankName = bankName;
            r.LineStatus = error is null ? "VALID" : "ERROR";
            r.ErrorMessage = error;
        }

        await RefreshBatchCountsAsync(batchId.Value, ct);
        return (batch.ValidRows ?? 0, batch.ErrorRows ?? 0);
    }

    /// <summary>
    /// submit_batch: validates the batch again, then makes a cheque of every valid line (status NEW at the client,
    /// then its RECEIVED event to the batch's location when an initial event is asked for); returns how many.
    /// </summary>
    public async Task<int> SubmitBatchAsync(long? batchId, string? submittedBy, string? createInitialEvent, ChqSource source, CancellationToken ct)
    {
        if (batchId is null)
            throw new ApexProcessException("ORA-20130: Batch ID is required.");
        var by = submittedBy ?? user.UserName;
        var (valid, errors) = await ValidateBatchAsync(batchId, ct);
        var batch = await db.Set<ChqUploadBatch>().FirstAsync(b => b.BatchId == batchId, ct);
        if (batch.BatchStatus == "SUBMITTED")
            throw new ApexProcessException("ORA-20131: Batch is already submitted.");
        if (errors > 0)
            throw new ApexProcessException("ORA-20132: Batch has validation errors. Please fix the errors before submit.");
        if (valid == 0)
            throw new ApexProcessException("ORA-20133: No valid cheque rows to submit.");

        var initial = (createInitialEvent ?? "Y").ToUpperInvariant() == "Y";
        var count = 0;
        var lines = await db.Set<ChqUploadLine>().Where(l => l.BatchId == batchId && l.LineStatus == "VALID" && l.TargetChequeId == null)
            .OrderBy(l => l.LineNo).ToListAsync(ct);
        foreach (var r in lines)
        {
            var chequeId = await events.NextIdAsync("CHQ_CHEQUES", "CHEQUE_ID", ct);
            var chequeRef = MakeChequeRef(chequeId);
            db.Set<ChqCheque>().Add(new ChqCheque
            {
                ChequeId = chequeId,
                ChequeRef = chequeRef,
                ChequeNo = r.ChequeNo!,
                ChequeAt = batch.ChequeAt,
                DueDate = r.DueDate!.Value,
                IssuerBankName = r.IssuerBankName!,
                IssuerBankCode = r.IssuerBankCode,
                DrawerNameEn = batch.DrawerNameEn ?? batch.CustomerNameEn ?? "Unknown Drawer",
                DrawerNameAr = batch.DrawerNameAr,
                CustomerId = batch.CustomerId ?? r.ClientId,
                CustomerNameEn = batch.CustomerNameEn,
                CustomerNameAr = batch.CustomerNameAr,
                ChequeAmount = r.ChequeAmount!.Value,
                CurrencyCode = batch.CurrencyCode,
                SourceModule = batch.SourceModule,
                SourceRefType = batch.SourceRefType,
                SourceRefId = batch.SourceRefId,
                ReceivedAt = batch.ReceivedAt,
                StatusCode = initial ? "NEW" : "RECEIVED",
                IsReplaced = false,
                IsGlPosted = false,
                LegalEntityId = batch.LegalEntityId,
                Notes = "Created from cheque upload batch " + batch.BatchNo,
                PartyTaxId = r.PartyTaxId,
                CurrentLocationCode = initial ? "CLIENT" : batch.DefaultLocationCode,
                CurrentCustodianUser = initial ? "CLIENT_REP" : batch.DefaultCustodianUser,
                BatchId = batch.BatchId,
            });
            await db.SaveChangesAsync(ct);

            if (initial)
            {
                // create_initial_received_event
                await events.RegisterEventAsync(chequeId, new ChqEventArgs
                {
                    EventType = "RECEIVED",
                    ToLocationCode = batch.DefaultLocationCode ?? "COMPANY_VAULT",
                    EventAt = batch.ReceivedAt,
                    EffectiveAt = batch.ReceivedAt,
                    EventAmount = r.ChequeAmount,
                    CurrencyCode = batch.CurrencyCode,
                    Notes = "Initial cheque receipt created from upload batch " + batch.BatchNo,
                    MovementRef = batch.BatchNo,
                    ToCustodianUser = batch.DefaultCustodianUser,
                    HandoverBy = by,
                    ReceivedBy = batch.DefaultCustodianUser ?? by,
                    CreatedBy = by,
                    SourceAppId = source.AppId,
                    SourcePageId = source.PageId,
                    SourceSessionId = source.SessionId,
                }, ct);
            }

            r.LineStatus = "SUBMITTED";
            r.TargetChequeId = chequeId;
            r.TargetChequeRef = chequeRef;
            count++;
        }

        await RefreshBatchCountsAsync(batch.BatchId, ct);
        batch.BatchStatus = "SUBMITTED";
        batch.SubmittedBy = by;
        batch.SubmittedAt = Now;
        await db.SaveChangesAsync(ct);

        // GL of the product action "Cheque batch submission" (the rule linked to it in Admin Setup), in the same transaction:
        // when it cannot post, the submission is rolled back with it
        var total = lines.Sum(l => l.ChequeAmount ?? 0);
        var ts = await posting.PostAsync(ProductActions.ChequeBatchSubmit,
            new ProductActionDocument("CHEQUE_UPLOAD_BATCH", batch.BatchId.ToString(CultureInfo.InvariantCulture), batch.BatchNo, GlTime.Today(clock),
                batch.CurrencyCode, $"Cheque batch {batch.BatchNo}: {count} cheque(s) received in the wallet for {batch.CustomerNameEn ?? batch.CustomerId}"),
            code => code == "CHEQUE_AMOUNT" ? total : null, ct);
        batch.GlTranSetId = ts;
        var ids = lines.Select(l => l.TargetChequeId).ToList();
        foreach (var c in await db.Set<ChqCheque>().Where(c => ids.Contains(c.ChequeId)).ToListAsync(ct))
        {
            c.IsGlPosted = true;
            c.GlJournalRef = ts;
            c.GlPostAt = Now;
        }

        await db.SaveChangesAsync(ct);
        return count;
    }

    private sealed class Row
    {
        public int RowNo;
        public string? Sr, ClientId, TaxId, ChequeNo, DueRaw, AmountRaw, BankCode, BankName, Errs;
        public DateOnly? DueDate;
        public decimal? Amount;
    }

    /// <summary>
    /// check_file (analyse_file): reads the first sheet of the Excel file and runs every check: the file, empty cells,
    /// issuer banks, one customer, the customer in CRM, due dates and amounts, duplicates. Nothing is saved.
    /// {ok, allOk, fileName, lineCount, total, firstDue, lastDue, customer, checks[{key, ok, detail, issues, more}], lines}.
    /// </summary>
    public async Task<JsonObject> CheckFileAsync(byte[]? file, string? fileName, CancellationToken ct)
    {
        string[] expect = ["sr", "clientid", "partytaxid", "chequeno", "duedate", "chequeamount", "issuerbankcode"];
        string[] title = ["Sr", "Client ID", "Party tax ID", "Cheque number", "Due date", "Amount", "Bank code"];
        string[] raw = ["sr", "client_id", "party_tax_id", "cheque_no", "due_date", "cheque_amount", "issuer_bank_code"];
        var rows = new List<Row>();
        var checks = new JsonArray();
        var cust = new JsonObject();
        var allOk = true;
        var readOk = true;
        string? readErr = null;
        var header = false;
        string? hdrMsg = null;

        JsonArray? iss = null;
        int cnt = 0, bad = 0;
        void Start()
        {
            iss = [];
            cnt = 0;
            bad = 0;
        }

        void Issue(string text)
        {
            cnt++;
            if (cnt <= MaxIssues)
                iss!.Add(text);
        }

        void RowError(Row r, string text)
        {
            if (r.Errs is null || !r.Errs.Contains(text, StringComparison.Ordinal))
            {
                var s = r.Errs + (r.Errs is not null ? "|" : string.Empty) + text;
                r.Errs = s.Length > 4000 ? s[..4000] : s;
            }
        }

        void AddCheck(string key, bool ok, string detail)
        {
            var o = new JsonObject { ["key"] = key, ["ok"] = ok, ["detail"] = detail };
            if (iss is not null && cnt > 0)
            {
                o["issues"] = iss;
                if (cnt > MaxIssues)
                    o["more"] = cnt - MaxIssues;
            }

            checks.Add(o);
            if (!ok)
                allOk = false;
            iss = null;
            cnt = 0;
        }

        void NotChecked(string key, string why)
        {
            iss = null;
            cnt = 0;
            AddCheck(key, false, "Not checked: " + why);
        }

        static string Plural(decimal n, string word) => Num(n) + " " + word + (n == 1 ? string.Empty : "s");
        static string? Norm(string? v) => v is null ? null : NonAlnum().Replace(RawPrefix().Replace(v.Trim(' '), string.Empty, 1), string.Empty).ToLowerInvariant() is { Length: > 0 } s ? s : null;
        static string? Whole(string? v) => v is not null && WholeNumber().IsMatch(v) ? TrailingZeros().Replace(v, string.Empty) : v;

        // 1. read the file
        if (file is null || file.Length == 0)
        {
            readOk = false;
            readErr = "The file is empty.";
        }
        else if (fileName is null || !fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            readOk = false;
            readErr = "Only Excel .xlsx files can be read. Save the file as \"Excel Workbook (*.xlsx)\" and try again.";
        }
        else
        {
            try
            {
                using var stream = new MemoryStream(file);
                foreach (var line in workbook.ReadFirstSheet(stream, 7).Take(10000))
                {
                    if (!header)
                    {
                        header = true;
                        for (var i = 0; i < 7; i++)
                        {
                            if ((Norm(line[i]) ?? "#") != expect[i])
                            {
                                var m = hdrMsg + (hdrMsg is not null ? "; " : string.Empty) + "column " + (char)('A' + i) + " is \""
                                        + (CleanValue(line[i]) ?? "(empty)") + "\", expected raw_" + raw[i];
                                hdrMsg = m.Length > 4000 ? m[..4000] : m;
                            }
                        }
                    }
                    else if (Enumerable.Range(0, 7).Any(i => CleanValue(line[i]) is not null))
                    {
                        var r = new Row
                        {
                            RowNo = line.RowNumber,
                            Sr = CleanValue(line[0]),
                            ClientId = CleanValue(line[1]),
                            TaxId = CleanValue(line[2]),
                            ChequeNo = CleanValue(line[3]),
                            DueRaw = CleanValue(line[4]),
                            AmountRaw = CleanValue(line[5]),
                            BankCode = CleanValue(line[6])?.ToUpperInvariant(),
                        };
                        // whole numbers read from Excel can arrive as 361.0: keep them as 361
                        r.ClientId = Whole(r.ClientId);
                        r.TaxId = Whole(r.TaxId);
                        r.Sr = Whole(r.Sr);
                        r.DueDate = ParseExcelDate(r.DueRaw is not null && IsoStart().IsMatch(r.DueRaw) ? r.DueRaw[..10] : r.DueRaw);
                        r.Amount = ParseNumber(r.AmountRaw);
                        if (r.BankCode is not null)
                            r.BankName = await GetBankNameAsync(r.BankCode, ct);
                        rows.Add(r);
                    }
                }
            }
            catch (InvalidDataException e)
            {
                readOk = false;
                readErr = "The file could not be read as an Excel workbook (" + (e.InnerException?.Message ?? e.Message) + ").";
            }
        }

        var n = rows.Count;

        // check 1: file
        Start();
        if (!readOk)
            AddCheck("file", false, readErr!);
        else if (!header)
            AddCheck("file", false, "The file is empty: the first sheet has no rows.");
        else if (hdrMsg is not null)
        {
            iss!.Add("First row: " + hdrMsg);
            cnt = 1;
            AddCheck("file", false, "The first row does not hold the template columns. Use the Excel template.");
        }
        else if (n == 0)
            AddCheck("file", false, "The file has the template columns but no cheque lines.");
        else
            AddCheck("file", true, Plural(n, "cheque line") + " read from the first sheet, 7 columns as in the template.");

        decimal total = 0;
        DateOnly? first = null, last = null;
        if (!allOk)
        {
            foreach (var k in new[] { "complete", "banks", "customer", "crm", "values", "dupes" })
                NotChecked(k, "the file could not be read.");
        }
        else
        {
            // check 2: no empty cells
            Start();
            foreach (var r in rows)
            {
                var empty = string.Join(", ", new[] { r.Sr, r.ClientId, r.TaxId, r.ChequeNo, r.DueRaw, r.AmountRaw, r.BankCode }
                    .Select((v, i) => v is null ? title[i] : null).Where(t => t is not null));
                if (empty.Length > 0)
                {
                    bad++;
                    Issue("Row " + r.RowNo + ": " + empty + (empty.Contains(',', StringComparison.Ordinal) ? " are" : " is") + " empty");
                    RowError(r, empty + " empty");
                }
            }

            AddCheck("complete", bad == 0, bad == 0
                ? "All " + (n * 7) + " cells of the " + Plural(n, "line") + " are filled in."
                : Plural(bad, "line") + (bad == 1 ? " has" : " have") + " empty cells.");

            // check 3: issuer banks
            Start();
            var banks = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var r in rows)
            {
                if (r.BankCode is null)
                {
                    bad++;
                    Issue("Row " + r.RowNo + ": no bank code");
                    RowError(r, "No bank code");
                }
                else if (r.BankName is null)
                {
                    bad++;
                    Issue("Row " + r.RowNo + ": bank code " + r.BankCode + " is not an active issuer bank");
                    RowError(r, "Unknown bank " + r.BankCode);
                }
                else
                    banks.Add(Cut(r.BankCode, 400));
            }

            if (bad == 0)
                AddCheck("banks", true, Plural(banks.Count, "bank") + ": " + string.Join(", ", banks) + ".");
            else
                AddCheck("banks", false, Plural(bad, "line") + (bad == 1 ? " has" : " have") + " no bank or a bank that is not defined.");

            // check 4: one customer
            Start();
            var ids = new SortedDictionary<string, int>(StringComparer.Ordinal);
            string? topId = null, oneId = null;
            var topN = 0;
            foreach (var r in rows.Where(r => r.ClientId is not null))
            {
                var key = Cut(r.ClientId!.ToUpperInvariant(), 400);
                ids[key] = ids.TryGetValue(key, out var c) ? c + 1 : 1;
                if (ids[key] > topN)
                {
                    topN = ids[key];
                    topId = key;
                }
            }

            if (ids.Count == 0)
                AddCheck("customer", false, "No line has a Client ID.");
            else if (ids.Count == 1)
            {
                oneId = ids.Keys.First();
                AddCheck("customer", true, "All " + Plural(n, "line") + (n == 1 ? " is" : " are") + " for Client ID " + oneId + ".");
            }
            else
            {
                var txt = string.Join(", ", ids.Keys);
                foreach (var r in rows.Where(r => r.ClientId is not null && r.ClientId.ToUpperInvariant() != topId))
                {
                    Issue("Row " + r.RowNo + ": Client ID " + r.ClientId);
                    RowError(r, "Client ID " + r.ClientId + " (other customer)");
                }

                AddCheck("customer", false, "The file holds " + ids.Count + " different Client IDs: " + txt + ". One file must hold the cheques of one customer.");
                cust["ids"] = "Client IDs " + txt;
            }

            // check 5: customer in CRM (FCT_CLIENTS)
            Start();
            if (oneId is null)
                NotChecked("crm", ids.Count == 0 ? "the file has no Client ID." : "the file holds more than one customer.");
            else if (!Digits().IsMatch(oneId))
                AddCheck("crm", false, "Client ID " + oneId + " is not a number, so it cannot be a CRM customer.");
            else
            {
                var clientId = decimal.Parse(oneId, CultureInfo.InvariantCulture);
                var client = clientId > long.MaxValue ? null : await db.Set<CrmClient>().AsNoTracking().Where(c => c.ClientId == (long)clientId)
                    .Select(c => new { c.ClientName, c.ClientNameAr, c.IsActive, c.IsBlacklisted }).FirstOrDefaultAsync(ct);
                cust["id"] = oneId;
                if (client is null)
                    AddCheck("crm", false, "Client ID " + oneId + " is not defined in CRM.");
                else
                {
                    cust["name"] = client.ClientName;
                    cust["nameAr"] = client.ClientNameAr;
                    if (client.IsBlacklisted)
                        AddCheck("crm", false, "Client " + oneId + " - " + client.ClientName + " is blacklisted in CRM.");
                    else if (!client.IsActive)
                        AddCheck("crm", false, "Client " + oneId + " - " + client.ClientName + " is not active in CRM.");
                    else
                        AddCheck("crm", true, "Client " + oneId + " - " + client.ClientName + " is an active customer in CRM.");
                }
            }

            // check 6: due dates and amounts
            Start();
            foreach (var r in rows)
            {
                if (r.DueRaw is not null && r.DueDate is null)
                {
                    bad++;
                    Issue("Row " + r.RowNo + ": due date \"" + r.DueRaw + "\" is not a valid date");
                    RowError(r, "Invalid due date");
                }

                if (r.AmountRaw is not null && r.Amount is null or <= 0)
                {
                    bad++;
                    Issue("Row " + r.RowNo + ": amount \"" + r.AmountRaw + "\" must be a number greater than zero");
                    RowError(r, "Invalid amount");
                }

                if (r.Amount > 0)
                    total += r.Amount.Value;
                if (r.DueDate is { } d)
                {
                    first = first is null || d < first ? d : first;
                    last = last is null || d > last ? d : last;
                }
            }

            AddCheck("values", bad == 0, bad == 0
                ? "All due dates are valid dates and all amounts are greater than zero."
                : Plural(bad, "value") + " to correct.");

            // check 7: duplicates
            Start();
            var keys = new Dictionary<string, Row>(StringComparer.Ordinal);
            foreach (var r in rows.Where(r => r.ChequeNo is not null && r.BankCode is not null))
            {
                var key = Cut(r.ChequeNo!.Trim(' ').ToUpperInvariant() + "|" + r.BankCode, 400);
                if (keys.TryGetValue(key, out var prev))
                {
                    bad++;
                    Issue("Rows " + prev.RowNo + " and " + r.RowNo + ": cheque " + r.ChequeNo + " / " + r.BankCode + " appears twice");
                    RowError(r, "Duplicate of row " + prev.RowNo);
                }
                else
                    keys[key] = r;

                var no = r.ChequeNo!.Trim(' ').ToUpperInvariant();
                var inWallet = (await db.Set<ChqCheque>().AsNoTracking().Where(c => c.ChequeNo.Trim().ToUpper() == no)
                        .Select(c => new { c.ChequeRef, c.IssuerBankCode }).ToListAsync(ct))
                    .Where(c => (c.IssuerBankCode ?? "#").Trim(' ').ToUpperInvariant() == r.BankCode).Select(c => c.ChequeRef)
                    .OrderByDescending(x => x, StringComparer.Ordinal).FirstOrDefault();
                if (inWallet is not null)
                {
                    bad++;
                    Issue("Row " + r.RowNo + ": cheque " + r.ChequeNo + " / " + r.BankCode + " is already in the wallet (" + inWallet + ")");
                    RowError(r, "Already in the wallet");
                }
                else
                {
                    var inBatch = (await (from l in db.Set<ChqUploadLine>().AsNoTracking()
                                          join b in db.Set<ChqUploadBatch>().AsNoTracking() on l.BatchId equals b.BatchId
                                          where b.BatchStatus != "SUBMITTED" && l.LineStatus != "SUBMITTED" && l.RawChequeNo != null && l.RawChequeNo.Trim().ToUpper() == no
                                          select new { b.BatchNo, l.RawIssuerBankCode }).ToListAsync(ct))
                        .Where(x => x.RawIssuerBankCode?.Trim(' ').ToUpperInvariant() == r.BankCode).Select(x => x.BatchNo)
                        .OrderByDescending(x => x, StringComparer.Ordinal).FirstOrDefault();
                    if (inBatch is not null)
                    {
                        bad++;
                        Issue("Row " + r.RowNo + ": cheque " + r.ChequeNo + " / " + r.BankCode + " is already loaded in batch " + inBatch
                              + " (not submitted yet: submit or delete that batch)");
                        RowError(r, "Already in batch " + inBatch);
                    }
                }
            }

            AddCheck("dupes", bad == 0, bad == 0
                ? "No cheque appears twice in the file, and none is already in the wallet or in another batch."
                : Plural(bad, "cheque") + " already loaded or repeated.");
        }

        var lines = new JsonArray();
        foreach (var r in rows)
        {
            var o = new JsonObject
            {
                ["line"] = r.RowNo,
                ["sr"] = r.Sr,
                ["clientId"] = r.ClientId,
                ["taxId"] = r.TaxId,
                ["chequeNo"] = r.ChequeNo,
                ["dueRaw"] = r.DueRaw,
                ["dueDate"] = r.DueDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["dueStore"] = r.DueDate is { } d ? d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : r.DueRaw,
                ["amountRaw"] = r.AmountRaw,
            };
            if (r.Amount is { } a)
                o["amount"] = Normal(a);
            o["bank"] = r.BankCode;
            o["bankName"] = r.BankName;
            o["errors"] = new JsonArray((r.Errs?.Split('|') ?? []).Select(e => (JsonNode?)e).ToArray());
            lines.Add(o);
        }

        return new JsonObject
        {
            ["ok"] = true,
            ["allOk"] = allOk,
            ["fileName"] = fileName,
            ["lineCount"] = n,
            ["total"] = Normal(total),
            ["firstDue"] = first?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["lastDue"] = last?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["customer"] = cust,
            ["checks"] = checks,
            ["lines"] = lines,
        };
    }

    /// <summary>
    /// import_file: the same checks as the upload page; when all pass, a batch for the customer of the file with its
    /// lines, validated. Returns the batch and its valid and error line counts.
    /// </summary>
    public async Task<(long BatchId, int Valid, int Errors)> ImportFileAsync(byte[]? file, string? fileName, long? legalEntityId, string? currencyCode,
        string? sourceModule, string? defaultLocationCode, string? notes, string? createdBy, CancellationToken ct)
    {
        var doc = await CheckFileAsync(file, fileName, ct);
        var custId = doc["customer"]?["id"]?.GetValue<string>();
        if (doc["allOk"]?.GetValue<bool>() != true || custId is null)
            throw new ApexProcessException("ORA-20150: The file did not pass all the checks, so no batch was created. Check the file again on the upload page.");

        var by = createdBy ?? user.UserName;
        var batchId = await CreateBatchAsync(new ChqNewBatch
        {
            FileName = fileName is null ? null : Cut(fileName, 500),
            MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            LegalEntityId = legalEntityId,
            CustomerId = custId,
            CustomerNameEn = doc["customer"]?["name"]?.GetValue<string>() is { } en ? Cut(en, 300) : null,
            CustomerNameAr = doc["customer"]?["nameAr"]?.GetValue<string>() is { } ar ? Cut(ar, 300) : null,
            CurrencyCode = currencyCode ?? "EGP",
            SourceModule = sourceModule ?? "FACTORING",
            ChequeAt = Today,
            ReceivedAt = Today,
            DefaultLocationCode = defaultLocationCode ?? "COMPANY_VAULT",
            Notes = notes is null ? null : Cut(notes, 2000),
            CreatedBy = by,
        }, ct);

        var rn = 0;
        foreach (var l in doc["lines"]!.AsArray())
        {
            string? S(string key) => l![key]?.GetValue<string>() is { } v ? Cut(v, 100) : null;
            db.Set<ChqUploadLine>().Add(new ChqUploadLine
            {
                BatchId = batchId,
                LineNo = ++rn,
                RawSr = S("sr"),
                RawClientId = S("clientId"),
                RawPartyTaxId = S("taxId"),
                RawChequeNo = S("chequeNo"),
                RawDueDateText = S("dueStore"),
                RawChequeAmountText = S("amountRaw"),
                RawIssuerBankCode = S("bank"),
                LineStatus = "LOADED",
            });
        }

        var batch = await db.Set<ChqUploadBatch>().FirstAsync(b => b.BatchId == batchId, ct);
        batch.BatchStatus = "LOADED";
        await db.SaveChangesAsync(ct);
        await RefreshBatchCountsAsync(batchId, ct);
        var (valid, errors) = await ValidateBatchAsync(batchId, ct);
        return (batchId, valid, errors);
    }

    private static string Cut(string value, int length) => value.Length <= length ? value : value[..length];

    private static string Num(decimal n) => Normal(n).ToString(CultureInfo.InvariantCulture);

    private static decimal Normal(decimal d) => d / 1.000000000000000000000000000000000m;

    [GeneratedRegex(@"^-?(\d{1,18}(\.\d{0,4})?|\.\d{1,4})$")]
    private static partial Regex NumberText();

    [GeneratedRegex(@"^[0-9]+(\.[0-9]+)?$")]
    private static partial Regex SerialDay();

    [GeneratedRegex(@"^(\d{1,2})[^\d](\d{1,2})[^\d](\d{4})$")]
    private static partial Regex DayMonthYear();

    [GeneratedRegex(@"^(\d{4})[^\d](\d{1,2})[^\d](\d{1,2})$")]
    private static partial Regex YearMonthDay();

    [GeneratedRegex(@"^[0-9]{4}-[0-9]{2}-[0-9]{2}")]
    private static partial Regex IsoStart();

    [GeneratedRegex(@"^[0-9]+\.0+$")]
    private static partial Regex WholeNumber();

    [GeneratedRegex(@"\.0+$")]
    private static partial Regex TrailingZeros();

    [GeneratedRegex("^raw_", RegexOptions.IgnoreCase)]
    private static partial Regex RawPrefix();

    [GeneratedRegex("[^A-Za-z0-9]")]
    private static partial Regex NonAlnum();

    [GeneratedRegex("^[0-9]+$")]
    private static partial Regex Digits();
}
