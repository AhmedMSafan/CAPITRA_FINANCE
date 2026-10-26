using System.Globalization;
using System.Text;
using System.Text.Json;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mtg.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

#pragma warning disable CA1862 // upper(INVESTOR_CODE) = upper(:code) runs in the database

/// <summary>App 105 page 208, Before Header "Load Purchased Portfolio Workflow".</summary>
public sealed class P0208PageLoad(ICapitraDbContext db) : IApexProcess
{
    public int App => 105;

    public int Page => 208;

    public string Name => "PAGE_LOAD";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var items = ApexItems.Read(request.X01);
        string? I(string name) => items.TryGetValue(name, out var v) && !string.IsNullOrEmpty(v) ? v : null;
        var set = new Dictionary<string, string?>
        {
            ["P208_BATCH_STATUS"] = I("P208_BATCH_STATUS") ?? "NOT VALIDATED",
            ["P208_TOTAL_ROWS"] = I("P208_TOTAL_ROWS") ?? "0",
            ["P208_VALID_ROWS"] = I("P208_VALID_ROWS") ?? "0",
            ["P208_ERROR_ROWS"] = I("P208_ERROR_ROWS") ?? "0",
            ["P208_IMPORTED_ROWS"] = I("P208_IMPORTED_ROWS") ?? "0",
            ["P208_INV_SUMMARY"] = I("P208_INV_SUMMARY") ?? "0 / 0 / 0 / 0",
            ["P208_UNIT_SUMMARY"] = I("P208_UNIT_SUMMARY") ?? "0 / 0 / 0 / 0",
            ["P208_ACCOUNT_SUMMARY"] = I("P208_ACCOUNT_SUMMARY") ?? "0 / 0 / 0 / 0",
            ["P208_WF_STEP_NAME"] = "Excel Import & Validation",
        };
        if (MtgAjax.Long(I("P208_PORTFOLIO_ID")) is not { } id)
            return new { ok = true, items = set };

        var p = await db.Set<MtgPurchasedPortfolio>().Where(x => x.PortfolioId == id)
            .Select(x => new { x.WorkflowInstanceId, x.WorkflowTaskId, x.PortfolioNo, Project = db.Set<MtgProject>().Where(r => r.ProjectId == x.ProjectId).Select(r => r.ProjectNameEn ?? r.ProjectNameAr).FirstOrDefault() })
            .FirstOrDefaultAsync(ct);
        if (p is null)
            return new { ok = false, message = "ORA-20050: Selected portfolio does not exist.", items = set };
        if (p.WorkflowInstanceId is null)
            return new { ok = false, message = "ORA-20051: Workflow has not been started for this portfolio. Start it from Portfolio Cases first.", items = set };
        set["P208_PORTFOLIO_LABEL"] = p.PortfolioNo + " - " + p.Project;
        set["P208_WF_INSTANCE_ID"] = P0203Html.Num(p.WorkflowInstanceId);
        set["P208_TASK_ID"] = p.WorkflowTaskId is null ? null : P0203Html.Num(p.WorkflowTaskId);
        var staging = await db.Set<MtgPurchasedPortfolioImport>().FirstOrDefaultAsync(s => s.PortfolioId == id && s.WfInstanceId == p.WorkflowInstanceId, ct);
        if (staging is not null)
        {
            set["P208_BATCH_STATUS"] = staging.ValidationStatus;
            set["P208_TOTAL_ROWS"] = P0203Html.Num(staging.TotalRows);
            set["P208_VALID_ROWS"] = P0203Html.Num(staging.ValidCount);
            set["P208_ERROR_ROWS"] = P0203Html.Num(staging.ErrorCount);
        }

        return new { ok = true, items = set };
    }
}

/// <summary>
/// App 105 page 208, After Submit: "Validate Portfolio Workbook" (LOAD_VALIDATE) and "Import Workbook and Complete
/// Workflow" (IMPORT). The workbook is the file of P208_FILE in the user's session; the rows' results go to the
/// collection P208_VALIDATION.
/// </summary>
public sealed class P0208Submit(ICapitraDbContext db, ApexSession session, IWorkbookReader workbooks, MtgWorkflowSteps steps, ICurrentUser user, IClock clock) : IApexProcess
{
    private const string Collection = "P208_VALIDATION";

    public int App => 105;

    public int Page => 208;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var set = new Dictionary<string, string?>();
        await using var tx = await db.BeginTransactionAsync(ct);
        try
        {
            string? message = request.Item("REQUEST") switch
            {
                "LOAD_VALIDATE" => await ValidateAsync(request, set, ct),
                "IMPORT" => await ImportAsync(request, set, ct),
                _ => null,
            };
            await tx.CommitAsync(ct);
            return new { ok = true, message, items = set };
        }
        catch (Exception e) when (e is ApexProcessException or InvalidDataException or Common.Exceptions.BusinessRuleException)
        {
            await tx.RollbackAsync(ct);
            db.DiscardChanges();
            return new { ok = false, message = e.Message };
        }
    }

    private async Task<string> ValidateAsync(ApexRequest r, Dictionary<string, string?> set, CancellationToken ct)
    {
        var portfolioId = MtgAjax.Long(r.Item("P208_PORTFOLIO_ID")) ?? throw new ApexProcessException("ORA-20001: Portfolio is required.");
        var instanceId = MtgAjax.Long(r.Item("P208_WF_INSTANCE_ID")) ?? throw new ApexProcessException("ORA-20002: Portfolio workflow has not been started.");
        var fileName = r.Item("P208_FILE") ?? throw new ApexProcessException("ORA-20003: Excel file is required.");
        set["P208_TEMP_FILE_NAME"] = fileName;
        var file = session.File(fileName) ?? throw new ApexProcessException("ORA-01403: no data found");
        var sheets = Sheets(file);
        var p = await db.Set<MtgPurchasedPortfolio>().Where(x => x.PortfolioId == portfolioId)
            .Select(x => new { x.PortfolioNo, ProjectCode = db.Set<MtgProject>().Where(j => j.ProjectId == x.ProjectId).Select(j => j.ProjectCode).FirstOrDefault() })
            .FirstOrDefaultAsync(ct) ?? throw new ApexProcessException("ORA-01403: no data found");
        session.CreateOrTruncate(Collection);
        int total = 0, valid = 0, errors = 0;
        void AddRow(string sheet, int row, string entity, string? code, string? name, bool ok, string msg)
        {
            session.AddMember(Collection, [sheet, entity, code, name, ok ? "VALID" : "ERROR", ok ? "Row is valid." : msg], [row]);
            total++;
            if (ok)
                valid++;
            else
                errors++;
        }

        string Header(WorkbookRow row)
        {
            var e = string.Empty;
            if ((Trim(row[0]) ?? "#") != (p.PortfolioNo ?? "#"))
                e += "Portfolio number does not match the selected portfolio. ";
            if ((Trim(row[1]) ?? "#") != (p.ProjectCode ?? "#"))
                e += "Project code does not match the selected portfolio project. ";
            return e;
        }

        var invset = new HashSet<string>(StringComparer.Ordinal);
        var unitset = new HashSet<string>(StringComparer.Ordinal);
        var contractset = new HashSet<string>(StringComparer.Ordinal);
        int invTotal = 0, invValid = 0, invErr = 0, unitTotal = 0, unitValid = 0, unitErr = 0, accTotal = 0, accValid = 0, accErr = 0;
        foreach (var row in sheets.Investors)
        {
            var e = Header(row);
            var c = Upper(row[2]);
            invTotal++;
            if (c is null)
                e += "INVESTOR_CODE is required. ";
            else if (!invset.Add(c))
                e += "Duplicate INVESTOR_CODE. ";
            var ok = e.Length == 0;
            AddRow("INVESTORS", row.RowNumber, "Investor", c, row[4] ?? row[3], ok, e.Trim());
            if (ok)
                invValid++;
            else
                invErr++;
        }

        foreach (var row in sheets.Units)
        {
            var e = Header(row);
            var c = Upper(row[2]);
            unitTotal++;
            if (c is null)
                e += "UNIT_CODE is required. ";
            else if (!unitset.Add(c))
                e += "Duplicate UNIT_CODE. ";
            var ok = e.Length == 0;
            AddRow("UNITS", row.RowNumber, "Unit", c, Trim((row[4] ?? string.Empty) + " " + (row[5] ?? string.Empty) + " " + (row[6] ?? string.Empty)), ok, e.Trim());
            if (ok)
                unitValid++;
            else
                unitErr++;
        }

        foreach (var row in sheets.Accounts)
        {
            var e = Header(row);
            var c = Upper(row[2]);
            var inv = Upper(row[3]);
            var unt = Upper(row[4]);
            accTotal++;
            if (c is null)
                e += "SOURCE_CONTRACT_NO is required. ";
            else if (!contractset.Add(c))
                e += "Duplicate SOURCE_CONTRACT_NO. ";
            if (inv is null || !invset.Contains(inv))
                e += "INVESTOR_CODE does not exist in INVESTORS sheet. ";
            if (unt is null || !unitset.Contains(unt))
                e += "UNIT_CODE does not exist in UNITS sheet. ";
            if (await db.Set<MtgPurchasedPortfolioAccount>().AnyAsync(a => a.PortfolioId == portfolioId && a.SellerContractNo!.ToUpper() == c, ct))
                e += "Account number already exists. ";
            var ok = e.Length == 0;
            AddRow("ACCOUNTS", row.RowNumber, "Account", c, inv + " / " + unt, ok, e.Trim());
            if (ok)
                accValid++;
            else
                accErr++;
        }

        var status = errors == 0 && total > 0 ? "VALIDATED" : "ERROR";
        set["P208_BATCH_STATUS"] = status;
        set["P208_TOTAL_ROWS"] = Str(total);
        set["P208_VALID_ROWS"] = Str(valid);
        set["P208_ERROR_ROWS"] = Str(errors);
        set["P208_IMPORTED_ROWS"] = "0";
        set["P208_INV_SUMMARY"] = Str(invTotal) + " / " + Str(invValid) + " / " + Str(invErr) + " / 0";
        set["P208_UNIT_SUMMARY"] = Str(unitTotal) + " / " + Str(unitValid) + " / " + Str(unitErr) + " / 0";
        set["P208_ACCOUNT_SUMMARY"] = Str(accTotal) + " / " + Str(accValid) + " / " + Str(accErr) + " / 0";

        var json = JsonSerializer.Serialize(new { status, totalRows = total, validCount = valid, errorCount = errors });
        var staging = await db.Set<MtgPurchasedPortfolioImport>().FirstOrDefaultAsync(s => s.PortfolioId == portfolioId && s.WfInstanceId == instanceId, ct);
        if (staging is null)
        {
            staging = new MtgPurchasedPortfolioImport { PortfolioId = portfolioId, WfInstanceId = instanceId };
            db.Set<MtgPurchasedPortfolioImport>().Add(staging);
        }

        staging.FileName = file.FileName;
        staging.ValidationStatus = status;
        staging.ValidationJson = json;
        staging.TotalRows = total;
        staging.ValidCount = valid;
        staging.ErrorCount = errors;
        await db.SaveChangesAsync(ct);
        return status == "VALIDATED" ? "Workbook validated successfully. You can now Import & Complete." : "Validation completed with errors. Review the validation results.";
    }

    private async Task<string> ImportAsync(ApexRequest r, Dictionary<string, string?> set, CancellationToken ct)
    {
        if (r.Item("P208_BATCH_STATUS") != "VALIDATED")
            throw new ApexProcessException("ORA-20004: Workbook validation must be successful before import.");
        var fileName = r.Item("P208_TEMP_FILE_NAME") ?? throw new ApexProcessException("ORA-20005: The validated workbook is no longer available. Please choose the file and validate it again.");
        var file = session.File(fileName) ?? throw new ApexProcessException("ORA-20005: The validated workbook is no longer available in the APEX session. Please choose the file and validate it again.");
        var sheets = Sheets(file);
        var portfolioId = MtgAjax.Long(r.Item("P208_PORTFOLIO_ID"));
        var portfolio = await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, ct) ?? throw new ApexProcessException("ORA-01403: no data found");
        var projectId = portfolio.ProjectId;

        foreach (var row in sheets.Investors)
        {
            var code = Upper(row[2]);
            if (await db.Set<MtgInvestor>().AnyAsync(i => i.InvestorCode.ToUpper() == code, ct))
                continue;
            db.Set<MtgInvestor>().Add(new MtgInvestor
            {
                InvestorCode = code ?? throw Null("FNV_INVESTOR", "INVESTOR_CODE"),
                FullNameAr = row[3] ?? throw Null("FNV_INVESTOR", "FULL_NAME_AR"),
                FullNameEn = row[4],
                NationalId = row[5],
                DateOfBirthDate = D(row[6]),
                PhoneNo = row[7],
                EmailAddress = row[8],
                AddressText = row[9],
                EmployerName = row[10],
                DeclaredIncome = N(row[11]),
                IsActive = (row[12] ?? "Y") == "Y",
            });
            await db.SaveChangesAsync(ct);
        }

        foreach (var row in sheets.Units)
        {
            var code = Upper(row[2]);
            if (await db.Set<MtgProjectUnit>().AnyAsync(u => u.ProjectId == projectId && u.UnitCode.ToUpper() == code, ct))
                continue;
            db.Set<MtgProjectUnit>().Add(new MtgProjectUnit
            {
                ProjectId = projectId,
                UnitCode = code ?? throw Null("FNV_PROJECT_UNIT", "UNIT_CODE"),
                UnitTypeCode = row[3],
                BuildingNo = row[4],
                FloorNo = row[5],
                UnitNo = row[6],
                AreaSqm = N(row[7]),
                ContractValue = N(row[8]),
                CurrentMarketValue = N(row[9]),
                LicenseStatus = row[10],
                IsActive = (row[11] ?? "Y") == "Y",
            });
            await db.SaveChangesAsync(ct);
        }

        var imported = 0;
        foreach (var row in sheets.Accounts)
        {
            var invCode = Upper(row[3]);
            var unitCode = Upper(row[4]);
            var invId = await db.Set<MtgInvestor>().Where(i => i.InvestorCode.ToUpper() == invCode).Select(i => (long?)i.InvestorId).FirstOrDefaultAsync(ct)
                ?? throw new ApexProcessException("ORA-01403: no data found");
            var unitId = await db.Set<MtgProjectUnit>().Where(u => u.ProjectId == projectId && u.UnitCode.ToUpper() == unitCode).Select(u => (long?)u.UnitId).FirstOrDefaultAsync(ct)
                ?? throw new ApexProcessException("ORA-01403: no data found");

            // exception when dup_val_on_index then null
            if (await db.Set<MtgPurchasedPortfolioAccount>().AnyAsync(a => a.PortfolioId == portfolio.PortfolioId && a.InvestorId == invId && a.UnitId == unitId, ct))
                continue;
            db.Set<MtgPurchasedPortfolioAccount>().Add(new MtgPurchasedPortfolioAccount
            {
                PortfolioId = portfolio.PortfolioId,
                InvestorId = invId,
                UnitId = unitId,
                SellerContractNo = Upper(row[2]),
                BookingDate = D(row[5]),
                ContractDate = D(row[6]),
                ContractValue = N(row[7]) ?? throw Null("FNV_PURSHASED_PORTFOLIO_ACCOUNT", "CONTRACT_VALUE"),
                PaidValue = N(row[8]) ?? throw Null("FNV_PURSHASED_PORTFOLIO_ACCOUNT", "PAID_VALUE"),
                OutstandingValue = N(row[9]) ?? throw Null("FNV_PURSHASED_PORTFOLIO_ACCOUNT", "OUTSTANDING_VALUE"),
                InstallmentAmount = N(row[10]),
                InstallmentFrequency = row[11],
                LastPaymentDate = D(row[12]),
                RegularPaymentMonths = (int)(N(row[13]) ?? 0),
                ArrearsAmount = N(row[14]) ?? 0,
                AccountStatus = row[15] ?? "ACTIVE",
                DealDecisionCode = "SELECTED",
            });
            await db.SaveChangesAsync(ct);
            imported++;
        }

        var accounts = await db.Set<MtgPurchasedPortfolioAccount>().Where(a => a.PortfolioId == portfolio.PortfolioId).ToListAsync(ct);
        foreach (var a in accounts.Where(a => a.DealDecisionCode is null))
            a.DealDecisionCode = "SELECTED";
        portfolio.FaceValue = accounts.Sum(a => a.ContractValue);
        portfolio.OutstandingValue = accounts.Sum(a => a.OutstandingValue);
        portfolio.SelectedAccountCount = accounts.Count;
        portfolio.SelectedFaceValue = portfolio.FaceValue;
        portfolio.SelectedOutstandingValue = portfolio.OutstandingValue;
        portfolio.SelectedPaidValue = accounts.Sum(a => a.PaidValue);
        set["P208_IMPORTED_ROWS"] = Str(imported);
        var instanceId = MtgAjax.Long(r.Item("P208_WF_INSTANCE_ID"));
        foreach (var s in await db.Set<MtgPurchasedPortfolioImport>().Where(s => s.PortfolioId == portfolio.PortfolioId && s.WfInstanceId == instanceId).ToListAsync(ct))
            s.ValidationStatus = "IMPORTED";
        await db.SaveChangesAsync(ct);

        if (MtgAjax.Long(r.Item("P208_TASK_ID")) is { } taskId)
        {
            var newTask = await steps.DecideAsync(taskId, "COMPLETE", "Portfolio Excel workbook validated and imported successfully.", ct);
            var oldStatus = portfolio.PortfolioStatusCode;
            portfolio.WorkflowTaskId = newTask;
            portfolio.PortfolioStatusCode = "DATA_LOADING_COMPLETED";
            if (oldStatus != "DATA_LOADING_COMPLETED")
            {
                db.Set<MtgPurchasedPortfolioStatusHist>().Add(new MtgPurchasedPortfolioStatusHist
                {
                    PortfolioId = portfolio.PortfolioId,
                    FromStatus = oldStatus,
                    ToStatus = "DATA_LOADING_COMPLETED",
                    ChangeAt = clock.UtcNow,
                    ChangedBy = user.UserName ?? string.Empty,
                    Comments = "Excel portfolio data loaded successfully.",
                });
            }

            await db.SaveChangesAsync(ct);
            set["P208_TASK_ID"] = newTask is null ? null : Str(newTask.Value);
        }

        set["P208_TEMP_FILE_NAME"] = null;
        return "Workbook imported successfully and the Excel workflow step was completed.";
    }

    // apex_data_parser.get_xlsx_worksheets: the tabs named INVESTORS, UNITS and ACCOUNTS
    private (IReadOnlyList<WorkbookRow> Investors, IReadOnlyList<WorkbookRow> Units, IReadOnlyList<WorkbookRow> Accounts) Sheets(ApexTempFile file)
    {
        using var stream = new MemoryStream(file.Content);
        var sheets = workbooks.Read(stream);
        if (!sheets.TryGetValue("INVESTORS", out var investors) || !sheets.TryGetValue("UNITS", out var units) || !sheets.TryGetValue("ACCOUNTS", out var accounts))
            throw new ApexProcessException("ORA-20004: Workbook must contain worksheet tabs named INVESTORS, UNITS and ACCOUNTS. Tabs found: " + (sheets.Count == 0 ? "none" : string.Join(", ", sheets.Keys)));
        return (investors, units, accounts);
    }

    private static ApexProcessException Null(string table, string column) =>
        new("ORA-01400: cannot insert NULL into (\"" + table + "\".\"" + column + "\")");

    private static string Str(long n) => n.ToString(CultureInfo.InvariantCulture);

    private static string? Trim(string? p) => string.IsNullOrWhiteSpace(p) ? null : p.Trim();

    private static string? Upper(string? p) => Trim(p)?.ToUpperInvariant();

    // n(p): to_number(replace(trim(p),',','')), null when it is not a number
    private static decimal? N(string? p) =>
        Trim(p) is { } t && decimal.TryParse(t.Replace(",", string.Empty, StringComparison.Ordinal), NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : null;

    // d(p): YYYY-MM-DD, else the default date format (DD-MON-RR), else null
    private static DateOnly? D(string? p) =>
        Trim(p) is not { } t ? null
        : DateOnly.TryParseExact(t, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d
        : DateOnly.TryParseExact(t, ["dd-MMM-yy", "dd-MMM-yyyy"], CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d
        : null;
}

/// <summary>App 105 page 208, the PL/SQL region "Load Portfolio Data".</summary>
public sealed class P0208LoadPortfolioData(IApexSqlReader reader, ApexSession session) : IApexProcess
{
    public int App => 105;

    public int Page => 208;

    public string Name => "REGION_LOAD_PORTFOLIO_DATA";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct) => new { ok = true, html = await HtmlAsync(request, ct) };

    private async Task<string> HtmlAsync(ApexRequest request, CancellationToken ct)
    {
        var items = request.Items ?? [];
        var batch = (request.Item("P208_BATCH_STATUS") ?? "NOT VALIDATED").ToUpperInvariant();
        var total = MtgAjax.Number(request.Item("P208_TOTAL_ROWS")) ?? 0;
        var valid = MtgAjax.Number(request.Item("P208_VALID_ROWS")) ?? 0;
        var err = MtgAjax.Number(request.Item("P208_ERROR_ROWS")) ?? 0;
        var view = (request.Item("REQUEST") ?? "-") == "VIEW";
        var inboxUrl = ApexPages.Url(105, 920);
        var portfolioId = request.Item("P208_PORTFOLIO_ID");
        if (portfolioId is null)
        {
            return "<div class=\"fxd\"><div class=\"fxd-card\" style=\"max-width:540px;margin:60px auto;text-align:center;padding:40px 28px\"><span class=\"fa fa-file-excel-o\" style=\"font-size:30px;color:var(--teal-400)\" aria-hidden=\"true\"></span>"
                + "<h2>" + T("Choose a portfolio first", "اختر محفظة أولاً") + "</h2><p style=\"color:var(--muted)\">" + T("Open this page from a portfolio or from the data loading task in your inbox.", "افتح هذه الصفحة من محفظة أو من مهمة تحميل البيانات في صندوقك.") + "</p>"
                + "<a class=\"fxd-btn fxd-btn--teal\" href=\"" + Url(201) + "\">" + T("Go to portfolio cases", "الذهاب إلى ملفات المحافظ") + "</a></div></div>\n";
        }

        var head = await reader.RowAsync(MtgApexPageSql.P208_HEADER, items, ct);
        if (head is null)
            return "<div class=\"fxd\"><div class=\"fxd-card\" style=\"max-width:520px;margin:60px auto;text-align:center;padding:40px 28px\"><h2>" + T("Portfolio not found", "المحفظة غير موجودة") + "</h2></div></div>\n";
        var pno = head.Text("PORTFOLIO_NO");
        var project = head.Text("PROJECT");
        var pcode = head.Text("PROJECT_CODE");
        var statusName = head.Text("STATUS_NAME") ?? P0203Html.Nice(head.Text("PORTFOLIO_STATUS_CODE"));
        var pfUrl = ApexPages.Url(105, 203, "P203_PORTFOLIO_ID", portfolioId, null);
        var validation = session.Members("P208_VALIDATION");
        var has = validation.Count;

        var h = new StringBuilder();
        h.AppendLine("<div class=\"fxd\" id=\"ldRoot\" data-batch=\"" + batch + "\">");
        h.AppendLine("<header class=\"fxd-hero\"><nav class=\"wb-crumbs\" aria-label=\"Breadcrumb\"><a href=\"" + Attr(pfUrl) + "\"><span class=\"fa fa-arrow-left\" aria-hidden=\"true\"></span> "
            + T("Back to portfolio", "العودة إلى المحفظة") + "</a><span>/</span><a href=\"" + Attr(inboxUrl) + "\">" + T("My inbox", "صندوق المهام") + "</a><span>/</span><span>" + T("Load portfolio data", "تحميل بيانات المحفظة") + "</span></nav>");
        h.AppendLine("<div class=\"fxd-row\"><div class=\"fxd-anim\"><div class=\"fxd-eyebrow\"><i></i>" + T("Purchased Portfolio Workflow", "سير عمل المحافظ المشتراة") + "</div><h1 class=\"fxd-h1\">" + (view ? T("Portfolio data", "بيانات المحفظة") : T("Load portfolio data", "تحميل بيانات المحفظة")) + "</h1>"
            + "<p class=\"fxd-lead\">" + (view ? T("The accounts loaded into this portfolio and the result of the last workbook validation. Loading itself is done from the workflow task.", "الحسابات المحملة في هذه المحفظة ونتيجة آخر تحقق من الملف. يتم التحميل من مهمة سير العمل.")
                : T("Upload the seller workbook, fix anything the validation finds, then import it. Importing completes this workflow step.", "ارفع ملف البائع، وصحح ما يظهره التحقق، ثم استورده. الاستيراد يُكمل خطوة سير العمل هذه.")) + "</p></div>"
            + "<div class=\"wb-facts fxd-anim d1\"><div><span>" + T("Portfolio", "المحفظة") + "</span><strong>" + E(pno) + "</strong></div><div><span>" + T("Project", "المشروع") + "</span><strong>" + E(project) + "</strong></div>"
            + "<div><span>" + T("Project code", "كود المشروع") + "</span><strong>" + E(pcode) + "</strong></div><div><span>" + T("Status", "الحالة") + "</span><span class=\"wb-stage\">" + E(statusName) + "</span></div></div></div>");
        h.AppendLine("<div class=\"wb-steps\">"
            + Step(1, "Upload workbook", "رفع الملف", batch is "VALIDATED" or "ERROR" or "IMPORTED" ? "done" : "now")
            + Step(2, "Validate & fix", "التحقق والتصحيح", batch is "VALIDATED" or "IMPORTED" ? "done" : batch == "ERROR" ? "now" : null)
            + Step(3, "Import & complete step", "الاستيراد وإكمال الخطوة", batch == "IMPORTED" ? "done" : batch == "VALIDATED" ? "now" : null) + "</div></header>");

        h.AppendLine("<div class=\"fxd-wrap\"><div class=\"wb-grid\"><div class=\"wb-stack\">");
        if (batch != "IMPORTED" && !view)
        {
            h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-cloud-upload\" aria-hidden=\"true\"></span>" + (batch == "NOT VALIDATED" ? T("Upload the workbook", "ارفع الملف") : T("Upload a corrected workbook", "ارفع ملفاً مصححاً")) + "</h2>"
                + "<span class=\"fxd-label\">.xlsx</span></div>"
                + "<div class=\"ld-drop\" id=\"ldDrop\"><span class=\"ic\"><span class=\"fa fa-cloud-upload\" aria-hidden=\"true\"></span></span>"
                + "<b>" + T("Upload the Excel workbook", "ارفع ملف الإكسل") + "</b><small>" + T("Choose the seller workbook (.xlsx) below, then validate it.", "اختر ملف البائع (.xlsx) بالأسفل ثم تحقق منه.") + "</small>"
                + "<div class=\"ld-field\" id=\"ldField\"></div>"
                + "<div class=\"ld-tabs\"><small>" + T("Needs 3 tabs:", "يحتاج 3 أوراق:") + "</small><span>INVESTORS</span><span>UNITS</span><span>ACCOUNTS</span></div></div>"
                + "<div class=\"ld-act\"><button type=\"button\" class=\"wb-link\" id=\"ldFmt\"><span class=\"fa fa-table\" aria-hidden=\"true\"></span>" + T("Workbook format", "تنسيق الملف") + "</button><div class=\"ld-slot\" id=\"ldSlot\"></div></div></section>");
        }

        if (batch == "IMPORTED" || view)
        {
            var k = await reader.RowAsync(MtgApexPageSql.P208_KPIS, items, ct);
            decimal kn = k?.Number("K_N") ?? 0, kcv = k?.Number("K_CV") ?? 0, kpv = k?.Number("K_PV") ?? 0, kar = k?.Number("K_AR") ?? 0;
            h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-database\" aria-hidden=\"true\"></span>" + T("Now in the portfolio", "أصبح الآن في المحفظة") + "</h2>"
                + "<a class=\"fxd-btn fxd-btn--ghost\" style=\"height:36px\" data-open-tab=\"acc\" href=\"" + Attr(pfUrl) + "\">" + T("Open Accounts tab", "فتح تبويب الحسابات") + " &rsaquo;</a></div>"
                + "<div class=\"ld-kpis\"><div><b>" + P0203Html.Num(kn) + "</b><small>" + T("Accounts", "الحسابات") + "</small></div><div><b>" + P0203Html.Num(k?.Number("K_INV")) + "</b><small>" + T("Investors", "المستثمرون") + "</small></div><div><b>" + P0203Html.Num(k?.Number("K_UNIT")) + "</b><small>" + T("Units", "الوحدات") + "</small></div>"
                + "<div><b>" + Money(kcv) + "</b><small>" + T("Contract value", "قيمة العقود") + "</small></div><div><b>" + Money(kpv) + "</b><small>" + T("Paid", "المسدد") + " &middot; " + (kcv > 0 ? P0203Html.Num(P0203Html.Round(kpv / kcv * 100)) + "%" : "-") + "</small></div>"
                + "<div><b>" + Money(k?.Number("K_OV") ?? 0) + "</b><small>" + T("Outstanding", "القائم") + "</small></div><div><b" + (kar > 0 ? " style=\"color:var(--bad)\"" : string.Empty) + ">" + Money(kar) + "</b><small>" + T("Arrears", "المتأخرات") + "</small></div></div>"
                + "<div class=\"wb-tbl-wrap\" style=\"max-height:520px;overflow:auto\"><table class=\"wb-tbl\"><thead><tr><th>" + T("Investor", "المستثمر") + "</th><th>" + T("Unit", "الوحدة") + "</th><th>" + T("Contract", "العقد") + "</th>"
                + "<th class=\"fxd-num\">" + T("Contract value", "قيمة العقد") + "</th><th class=\"fxd-num\">" + T("Paid", "المسدد") + "</th><th class=\"fxd-num\">" + T("Outstanding", "القائم") + "</th><th class=\"fxd-num\">" + T("Arrears", "المتأخرات") + "</th></tr></thead><tbody>");
            var i = 0;
            foreach (var a in await reader.QueryAsync(MtgApexPageSql.P208_ACCOUNTS, items, ct))
            {
                i++;
                var arrears = a.Number("ARREARS_AMOUNT");
                h.AppendLine("<tr><td><strong style=\"font-weight:600\">" + E(a.Text("INV")) + "</strong><br><small style=\"color:var(--muted)\">" + E(a.Text("INVESTOR_CODE")) + "</small></td><td>" + E(a.Text("UNIT_CODE") ?? "-") + "</td><td><span class=\"wb-code\">" + E(a.Text("SELLER_CONTRACT_NO") ?? "-") + "</span></td>"
                    + "<td class=\"fxd-num\">" + ToChar(a.Number("CONTRACT_VALUE"), "FM999G999G999G990") + "</td><td class=\"fxd-num\">" + ToChar(a.Number("PAID_VALUE"), "FM999G999G999G990") + "</td><td class=\"fxd-num\">" + ToChar(a.Number("OUTSTANDING_VALUE"), "FM999G999G999G990") + "</td>"
                    + "<td class=\"fxd-num\"" + (arrears > 0 ? " style=\"color:var(--bad);font-weight:600\"" : string.Empty) + ">" + ToChar(arrears, "FM999G999G999G990") + "</td></tr>");
            }

            if (i == 0)
                h.AppendLine("<tr><td colspan=\"7\" class=\"wb-empty\">" + T("No accounts found for this portfolio.", "لا توجد حسابات لهذه المحفظة.") + "</td></tr>");
            h.AppendLine("</tbody></table></div>" + (kn > i ? "<div class=\"wb-empty\" style=\"padding:12px\">" + T("Showing the first ", "عرض أول ") + i + T(" of ", " من ") + P0203Html.Num(kn) + " &middot; <a data-open-tab=\"acc\" href=\"" + Attr(pfUrl) + "\">" + T("see all in the Accounts tab", "عرض الكل في تبويب الحسابات") + "</a></div>" : string.Empty) + "</section>");
        }

        // validation results (from the P208_VALIDATION collection filled by the Validate process)
        if (has > 0 && batch != "IMPORTED")
        {
            h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-list-alt\" aria-hidden=\"true\"></span>" + T("Validation results", "نتائج التحقق") + "</h2><span class=\"fxd-label\" id=\"ldCount\"></span></div>"
                + "<div class=\"wb-tools\"><label class=\"wb-search\"><span class=\"fa fa-search\" aria-hidden=\"true\"></span><input type=\"search\" id=\"ldQ\" placeholder=\"Search code, name or message\" aria-label=\"Search rows\"></label>"
                + "<div class=\"wb-seg\" role=\"group\" aria-label=\"Status\" id=\"ldSeg\"><button type=\"button\" data-f=\"ALL\" aria-pressed=\"true\">" + T("All", "الكل") + "<em></em></button>"
                + "<button type=\"button\" data-f=\"ERROR\" aria-pressed=\"false\">" + T("Errors", "أخطاء") + "<em></em></button><button type=\"button\" data-f=\"VALID\" aria-pressed=\"false\">" + T("Valid", "صحيح") + "<em></em></button></div>"
                + "<div class=\"wb-seg\" role=\"group\" aria-label=\"Sheet\" id=\"ldSheet\"><button type=\"button\" data-s=\"ALL\" aria-pressed=\"true\">" + T("All sheets", "كل الأوراق") + "</button>"
                + "<button type=\"button\" data-s=\"INVESTORS\" aria-pressed=\"false\">" + T("Investors", "المستثمرون") + "</button><button type=\"button\" data-s=\"UNITS\" aria-pressed=\"false\">" + T("Units", "الوحدات") + "</button>"
                + "<button type=\"button\" data-s=\"ACCOUNTS\" aria-pressed=\"false\">" + T("Accounts", "الحسابات") + "</button></div></div>"
                + "<div class=\"wb-tbl-wrap\" style=\"max-height:560px;overflow:auto\"><table class=\"wb-tbl\"><thead><tr><th>" + T("Sheet", "الورقة") + "</th><th>" + T("Row", "الصف") + "</th><th>" + T("Code", "الكود") + "</th><th>" + T("Name", "الاسم") + "</th>"
                + "<th>" + T("Result", "النتيجة") + "</th><th>" + T("Message", "الرسالة") + "</th></tr></thead><tbody id=\"ldRows\">");
            foreach (var m in validation.OrderBy(m => m.C[4] == "ERROR" ? 0 : 1).ThenBy(m => m.SeqId).Take(3000))
            {
                string? sheet = m.C[0], code = m.C[2], name = m.C[3], status = m.C[4], msg = m.C[5];
                h.AppendLine("<tr class=\"" + (status == "ERROR" ? "err" : string.Empty) + "\" data-f=\"" + status + "\" data-s=\"" + sheet + "\" data-q=\"" + Attr((code + " " + name + " " + msg).ToLowerInvariant()) + "\">"
                    + "<td><span class=\"ld-sh " + sheet + "\">" + E(sheet) + "</span></td><td class=\"fxd-num\">" + P0203Html.Num(m.N[0]) + "</td><td><span class=\"wb-code\">" + E(code ?? "-") + "</span></td><td>" + E(name) + "</td>"
                    + "<td>" + (status == "ERROR" ? "<span class=\"fxd-pill fxd-pill--bad\">" + T("Error", "خطأ") + "</span>" : "<span class=\"fxd-pill fxd-pill--ok\">" + T("Valid", "صحيح") + "</span>") + "</td>"
                    + "<td class=\"msg\">" + E(msg) + "</td></tr>");
            }

            h.AppendLine("</tbody></table></div><div class=\"wb-empty\" id=\"ldNoMatch\" hidden>" + T("No rows match this filter.", "لا توجد صفوف مطابقة.") + "</div></section>");
        }

        h.AppendLine("</div>");

        // right rail
        h.AppendLine("<aside class=\"wb-stack\">");
        if (batch == "IMPORTED")
        {
            h.AppendLine("<section class=\"fxd-card\"><div class=\"ld-done\"><span class=\"ic\"><span class=\"fa fa-check\" aria-hidden=\"true\"></span></span><h3>" + T("Data imported, step completed", "تم الاستيراد واكتملت الخطوة") + "</h3>"
                + "<p>" + T("The accounts are in the portfolio and the workflow moved on to the next step.", "أصبحت الحسابات ضمن المحفظة وانتقل سير العمل إلى الخطوة التالية.") + "</p>"
                + "<div class=\"row\"><a class=\"fxd-btn fxd-btn--teal\" data-open-tab=\"acc\" href=\"" + Attr(pfUrl) + "\">" + T("Open portfolio accounts", "فتح حسابات المحفظة") + "</a><a class=\"fxd-btn fxd-btn--ghost\" href=\"" + Attr(inboxUrl) + "\">" + T("My inbox", "صندوق المهام") + "</a></div></div></section>");
        }

        h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-check\" aria-hidden=\"true\"></span>" + T("Validation", "التحقق") + "</h2>"
            + batch switch
            {
                "VALIDATED" => "<span class=\"fxd-pill fxd-pill--ok\">" + T("Passed", "ناجح") + "</span>",
                "IMPORTED" => "<span class=\"fxd-pill fxd-pill--ok\">" + T("Imported", "تم الاستيراد") + "</span>",
                "ERROR" => "<span class=\"fxd-pill fxd-pill--bad\">" + T("Has errors", "به أخطاء") + "</span>",
                _ => "<span class=\"fxd-pill fxd-pill--mute\">" + T("Not run yet", "لم يتم بعد") + "</span>",
            } + "</div>"
            + "<div class=\"fxd-card-b\"><div class=\"wb-ring-row\"><div class=\"wb-ring\" style=\"--p:" + (total > 0 ? P0203Html.Num(P0203Html.Round(valid / total * 100)) : "0") + ";--ring:" + (err == 0 && total > 0 ? "var(--ok)" : "var(--teal-600)") + "\">"
            + "<span><span><b>" + P0203Html.Num(valid) + "</b>" + T("of", "من") + " " + P0203Html.Num(total) + "</span></span></div><p style=\"margin:0;color:var(--muted);font-size:13.5px\">"
            + (total == 0 ? T("Upload and validate the workbook to see its rows here.", "ارفع الملف وتحقق منه لترى صفوفه هنا.")
                : err == 0 ? T("Every row is valid.", "كل الصفوف صحيحة.")
                : "<b style=\"color:var(--bad)\">" + P0203Html.Num(err) + "</b> " + T("row(s) need fixing in the workbook before you can import.", "صف يحتاج تصحيحاً في الملف قبل الاستيراد.")) + "</p></div></div>"
            + (has > 0 ? "<div class=\"ld-sheets\" style=\"padding-top:0\">" + Sheet(validation, "INVESTORS", "Investors", "المستثمرون") + Sheet(validation, "UNITS", "Units", "الوحدات") + Sheet(validation, "ACCOUNTS", "Accounts", "الحسابات") + "</div>" : string.Empty) + "</section>");
        if (err > 0 && has > 0)
        {
            h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-exclamation-triangle\" aria-hidden=\"true\" style=\"color:var(--bad)\"></span>" + T("Most common problems", "أكثر المشكلات تكراراً") + "</h2></div><ul class=\"ld-errs\">");
            foreach (var g in validation.Where(m => m.C[4] == "ERROR").GroupBy(m => m.C[5]).Select(g => new { Msg = g.Key, N = g.Count() }).OrderByDescending(g => g.N).Take(6))
                h.AppendLine("<li><span class=\"fa fa-times-circle\" aria-hidden=\"true\"></span><span>" + E(g.Msg) + "</span><em>&times;" + g.N + "</em></li>");
            h.AppendLine("</ul></section>");
        }

        if (batch != "IMPORTED" && !view)
        {
            var validText = P0203Html.Num(valid);
            h.AppendLine("<section class=\"fxd-card\"><div class=\"fxd-card-h\"><h2><span class=\"fa fa-database\" aria-hidden=\"true\"></span>" + T("Import & complete", "الاستيراد والإكمال") + "</h2></div><div class=\"ld-next\"><ol>"
                + "<li>" + T("New investors and units are added; existing ones are kept.", "تُضاف المستثمرون والوحدات الجديدة، وتبقى الموجودة كما هي.") + "</li>"
                + "<li>" + T("Every account is added to this portfolio as Selected.", "يُضاف كل حساب إلى هذه المحفظة بحالة مختار.") + "</li>"
                + "<li>" + T("Portfolio totals are recalculated.", "يُعاد احتساب إجماليات المحفظة.") + "</li>"
                + "<li>" + T("This workflow step is completed and the status becomes Data Loading Completed.", "تكتمل خطوة سير العمل وتصبح الحالة اكتمال تحميل البيانات.") + "</li></ol>"
                + "<button type=\"button\" class=\"fxd-btn fxd-btn--ok\" id=\"ldImport\"" + (batch != "VALIDATED" ? " disabled" : string.Empty) + "><span class=\"fa fa-check-circle\" aria-hidden=\"true\"></span>" + T("Import & complete step", "استيراد وإكمال الخطوة") + "</button>"
                + "<small style=\"color:var(--muted);text-align:center\">" + (batch == "VALIDATED" ? T("Ready: " + validText + " rows will be imported.", "جاهز: سيتم استيراد " + validText + " صف.") : T("Available once the workbook passes validation.", "متاح بعد نجاح التحقق من الملف.")) + "</small></div></section>");
        }

        h.AppendLine("</aside></div></div>");

        // workbook format drawer
        h.AppendLine("<aside class=\"wb-drawer\" id=\"wbDrawer\" role=\"dialog\" aria-modal=\"true\" aria-labelledby=\"ldFmtT\" aria-hidden=\"true\"><div class=\"wb-dr-h\"><span class=\"wb-av\"><span class=\"fa fa-table\" aria-hidden=\"true\"></span></span><div class=\"wb-who\"><strong id=\"ldFmtT\">" + T("Workbook format", "تنسيق الملف") + "</strong>"
            + "<small>" + T("Row 1 is a header. Columns must be in this order.", "الصف الأول عناوين. يجب أن تكون الأعمدة بهذا الترتيب.") + "</small></div><button type=\"button\" class=\"wb-x\" data-close-drawer aria-label=\"Close\"><span class=\"fa fa-times\" aria-hidden=\"true\"></span></button></div>"
            + "<div class=\"wb-dr-b ld-fmt\"><p style=\"margin:0;font-size:13px;color:var(--muted)\">" + T("PORTFOLIO_NO and PROJECT_CODE must be ", "يجب أن يكون رقم المحفظة وكود المشروع ") + "<b>" + E(pno) + "</b> / <b>" + E(pcode) + "</b>.</p>\n"
            + "<h4><span class=\"ld-sh INVESTORS\">INVESTORS</span></h4><ol><li><code>PORTFOLIO_NO</code></li><li><code>PROJECT_CODE</code></li><li><code>INVESTOR_CODE</code></li><li><code>FULL_NAME_AR</code></li><li><code>FULL_NAME_EN</code></li><li><code>NATIONAL_ID</code></li><li><code>DATE_OF_BIRTH (YYYY-MM-DD)</code></li><li><code>PHONE_NO</code></li><li><code>EMAIL_ADDRESS</code></li><li><code>ADDRESS</code></li><li><code>EMPLOYER_NAME</code></li><li><code>DECLARED_INCOME</code></li><li><code>ACTIVE_FLAG (Y/N)</code></li></ol>"
            + "<h4><span class=\"ld-sh UNITS\">UNITS</span></h4><ol><li><code>PORTFOLIO_NO</code></li><li><code>PROJECT_CODE</code></li><li><code>UNIT_CODE</code></li><li><code>UNIT_TYPE_CODE</code></li><li><code>BUILDING_NO</code></li><li><code>FLOOR_NO</code></li><li><code>UNIT_NO</code></li><li><code>AREA_SQM</code></li><li><code>CONTRACT_VALUE</code></li><li><code>CURRENT_MARKET_VALUE</code></li><li><code>LICENSE_STATUS</code></li><li><code>ACTIVE_FLAG (Y/N)</code></li></ol>"
            + "<h4><span class=\"ld-sh ACCOUNTS\">ACCOUNTS</span></h4><ol><li><code>PORTFOLIO_NO</code></li><li><code>PROJECT_CODE</code></li><li><code>SOURCE_CONTRACT_NO</code></li><li><code>INVESTOR_CODE</code></li><li><code>UNIT_CODE</code></li><li><code>BOOKING_DATE</code></li><li><code>CONTRACT_DATE</code></li><li><code>CONTRACT_VALUE</code></li><li><code>PAID_VALUE</code></li><li><code>OUTSTANDING_VALUE</code></li><li><code>INSTALLMENT_AMOUNT</code></li><li><code>INSTALLMENT_FREQUENCY</code></li><li><code>LAST_PAYMENT_DATE</code></li><li><code>REGULAR_PAYMENT_MONTHS</code></li><li><code>ARREARS_AMOUNT</code></li><li><code>ACCOUNT_STATUS</code></li></ol></div></aside>");

        // shared overlays: drawer scrim, modal, toasts
        h.AppendLine("<div class=\"wb-scrim\" id=\"wbScrim\"></div>");
        h.AppendLine("<div class=\"wb-mask\" id=\"wbMask\"><div class=\"wb-modal\" role=\"dialog\" aria-modal=\"true\" aria-labelledby=\"wbMT\"><div class=\"wb-m-ico\"></div><h2 id=\"wbMT\"></h2><p></p><div class=\"wb-m-sum\"></div>"
            + "<div class=\"wb-m-act\"><button type=\"button\" class=\"fxd-btn fxd-btn--ghost\" data-m=\"no\">Cancel</button><button type=\"button\" class=\"fxd-btn\" data-m=\"yes\">OK</button></div></div></div>");
        h.AppendLine("<div class=\"wb-toasts\" id=\"wbToasts\" aria-live=\"polite\"></div>");
        h.AppendLine("</div>");
        return h.ToString();
    }

    private static string Step(int n, string en, string ar, string? state) =>
        "<div class=\"wb-step " + state + "\"><b>" + (state == "done" ? "&#10003;" : n.ToString(CultureInfo.InvariantCulture)) + "</b><span>" + T(en, ar) + "</span></div>";

    private static string Sheet(IReadOnlyList<ApexCollectionMember> members, string code, string en, string ar)
    {
        var rows = members.Where(m => m.C[0] == code).ToList();
        int n = rows.Count, v = rows.Count(m => m.C[4] == "VALID"), e = rows.Count(m => m.C[4] == "ERROR");
        return "<div class=\"ld-sheet\"><div class=\"top\"><b>" + T(en, ar) + "</b><span>" + v + " " + T("valid", "صحيح") + (e > 0 ? " &middot; <span style=\"color:var(--bad);font-weight:700\">" + e + " " + T("errors", "أخطاء") + "</span>" : string.Empty) + " &middot; " + n + "</span></div>"
            + "<div class=\"ld-bar\"><i class=\"v\" style=\"width:" + (n > 0 ? P0203Html.Num(P0203Html.Round((decimal)v / n * 100)) : "0") + "%\"></i><i class=\"e\" style=\"width:" + (n > 0 ? P0203Html.Num(P0203Html.Round((decimal)e / n * 100)) : "0") + "%\"></i></div></div>";
    }
}

/// <summary>App 105 page 208, the PL/SQL region "Step Actions": the Import button follows the task's Allowed Actions.</summary>
public sealed class P0208StepActions(MtgWorkflowSteps steps) : IApexProcess
{
    public int App => 105;

    public int Page => 208;

    public string Name => "REGION_STEP_ACTIONS";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (await steps.ActionsAsync(208, request, ct) is not { } codes)
            return new { ok = true, html = string.Empty };
        var pos = "NONE";
        foreach (var code in codes)
        {
            if (code == "COMPLETE")
                pos = "COMPLETE";
            else if (code == "APPROVE" && pos != "COMPLETE")
                pos = "APPROVE";
        }

        var html = codes.Count == 0 || pos == "NONE"
            ? "<style>#ldImport{display:none!important}</style><script>(function(){var x=document.getElementById('ldImport');var h=(x&&x.parentNode)||document.body;var s=document.createElement('span');s.className='wb-hint';s.style.cssText='color:var(--bad,#C43325);display:block;margin-top:8px';s.textContent='No Complete or Approve action is available for this task in the workflow setup (or the task is closed).';h.appendChild(s);})();</script>\n"
            : pos == "APPROVE"
                ? "<script>(function(){var b=document.getElementById('ldImport');if(!b)return;var s=b.querySelector('[data-finova-en]');if(!s)return;"
                    + "s.setAttribute('data-finova-en','Import & approve');s.setAttribute('data-finova-ar','استيراد واعتماد');"
                    + "s.textContent=document.documentElement.dir==='rtl'?'استيراد واعتماد':'Import & approve';})();</script>\n"
                : string.Empty;
        return new { ok = true, html };
    }
}
