using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.Portfolios;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Mtg.PortfolioSteps.ExcelImport.PortfolioWorkbook;

namespace Capitra.Application.Modules.Mtg.PortfolioSteps.ExcelImport;

/// <summary>What the Excel import step shows for one portfolio.</summary>
public sealed record ExcelImportDto(
    long PortfolioId,
    string PortfolioNo,
    string ProjectCode,
    string ProjectName,
    string PortfolioStatusCode,
    string PortfolioStatusName,
    string ImportStatusCode, // NOT_VALIDATED, VALIDATED, ERROR or IMPORTED
    string? FileName,
    int TotalRowCount,
    int ValidRowCount,
    int ErrorRowCount,
    int ImportedRowCount,
    ImportedFiguresDto Figures,
    IReadOnlyList<ImportedAccountDto> Accounts,
    IReadOnlyList<WorkbookSheetDto> Format);

/// <summary>Totals of the accounts now in the portfolio.</summary>
public sealed record ImportedFiguresDto(
    int AccountCount,
    int InvestorCount,
    int UnitCount,
    decimal ContractAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    decimal ArrearsAmount);

public sealed record ImportedAccountDto(
    long PortfolioAccountId,
    string InvestorCode,
    string InvestorName,
    string UnitCode,
    string? SellerContractNo,
    decimal ContractAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    decimal ArrearsAmount);

/// <summary>One checked workbook row. Valid rows say "Row is valid."; others list every problem found.</summary>
public sealed record WorkbookRowResultDto(
    string Sheet,
    int RowNumber,
    string Entity,
    string? Code,
    string? Name,
    bool IsValid,
    string Message);

public sealed record WorkbookSheetResultDto(string Sheet, int TotalRowCount, int ValidRowCount, int ErrorRowCount);

/// <summary>Result of Validate or Import. When Import finds errors nothing is written and the status is ERROR.</summary>
public sealed record WorkbookResultDto(
    string ImportStatusCode,
    string FileName,
    int TotalRowCount,
    int ValidRowCount,
    int ErrorRowCount,
    int ImportedRowCount,
    IReadOnlyList<WorkbookSheetResultDto> Sheets,
    IReadOnlyList<WorkbookRowResultDto> Rows);

public interface IMtgExcelImportService
{
    Task<ExcelImportDto> GetAsync(long portfolioId, CancellationToken cancellationToken);

    Task<WorkbookResultDto> ValidateAsync(long portfolioId, string fileName, Stream content, CancellationToken cancellationToken);

    Task<WorkbookResultDto> ImportAsync(long portfolioId, string fileName, Stream content, CancellationToken cancellationToken);
}

/// <summary>
/// Workflow step "Excel Import &amp; Validation" (APEX App 105 page 208): check the seller workbook, then load its
/// investors, units and accounts into the portfolio.
/// <para>
/// APEX kept the validated file in the session between Validate and Import. The API keeps no file: Import receives
/// the workbook again and checks it again, so what is imported is always what passed the checks.
/// </para>
/// </summary>
public sealed class MtgExcelImportService(ICapitraDbContext db, IWorkbookReader reader) : IMtgExcelImportService
{
    public const string NotValidatedStatus = "NOT_VALIDATED";
    private const int AccountListLimit = 200;
    private const decimal MaxAmount = 9_999_999_999_999_999m;

    public async Task<ExcelImportDto> GetAsync(long portfolioId, CancellationToken cancellationToken)
    {
        var header = await (
            from p in db.Set<MtgPortfolio>()
            join prj in db.Set<MtgProject>() on p.ProjectId equals prj.ProjectId
            join s in db.Set<MtgLkpPortfolioStatus>() on p.PortfolioStatusCode equals s.Code into statuses
            from s in statuses.DefaultIfEmpty()
            where p.PortfolioId == portfolioId
            select new
            {
                p.PortfolioId, p.PortfolioNo, prj.ProjectCode, ProjectName = prj.ProjectNameEn ?? prj.ProjectNameAr,
                p.PortfolioStatusCode, StatusName = s != null ? s.NameEn : p.PortfolioStatusCode
            }).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Portfolio {portfolioId} was not found.");

        var batch = await db.Set<MtgPortfolioImport>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, cancellationToken);

        var accounts = db.Set<MtgPortfolioAccount>().Where(x => x.PortfolioId == portfolioId);
        var figures = await accounts
            .GroupBy(x => x.PortfolioId)
            .Select(g => new ImportedFiguresDto(
                g.Count(),
                g.Select(x => x.InvestorId).Distinct().Count(),
                g.Select(x => x.ProjectUnitId).Distinct().Count(),
                g.Sum(x => x.ContractAmount),
                g.Sum(x => x.PaidAmount),
                g.Sum(x => x.OutstandingAmount),
                g.Sum(x => x.ArrearsAmount)))
            .FirstOrDefaultAsync(cancellationToken) ?? new ImportedFiguresDto(0, 0, 0, 0, 0, 0, 0);

        var list = await (
            from a in accounts
            join i in db.Set<MtgInvestor>() on a.InvestorId equals i.InvestorId
            join u in db.Set<MtgProjectUnit>() on a.ProjectUnitId equals u.ProjectUnitId
            orderby i.InvestorCode, u.UnitCode
            select new ImportedAccountDto(
                a.PortfolioAccountId, i.InvestorCode, i.FullNameEn ?? i.FullNameAr, u.UnitCode, a.SellerContractNo,
                a.ContractAmount, a.PaidAmount, a.OutstandingAmount, a.ArrearsAmount))
            .Take(AccountListLimit)
            .ToListAsync(cancellationToken);

        return new ExcelImportDto(
            header.PortfolioId, header.PortfolioNo, header.ProjectCode, header.ProjectName, header.PortfolioStatusCode,
            header.StatusName, batch?.ImportStatusCode ?? NotValidatedStatus, batch?.FileName,
            batch?.TotalRowCount ?? 0, batch?.ValidRowCount ?? 0, batch?.ErrorRowCount ?? 0, batch?.ImportedRowCount ?? 0,
            figures, list, Format);
    }

    public async Task<WorkbookResultDto> ValidateAsync(
        long portfolioId, string fileName, Stream content, CancellationToken cancellationToken)
    {
        var plan = await AnalyseAsync(portfolioId, fileName, content, cancellationToken);
        await SaveBatchAsync(portfolioId, fileName, plan, plan.IsValid ? MtgPortfolioImport.ValidatedStatus : MtgPortfolioImport.ErrorStatus, 0, cancellationToken);
        return plan.ToResult(plan.IsValid ? MtgPortfolioImport.ValidatedStatus : MtgPortfolioImport.ErrorStatus, fileName, 0);
    }

    public async Task<WorkbookResultDto> ImportAsync(
        long portfolioId, string fileName, Stream content, CancellationToken cancellationToken)
    {
        var plan = await AnalyseAsync(portfolioId, fileName, content, cancellationToken);
        if (!plan.IsValid)
        {
            await SaveBatchAsync(portfolioId, fileName, plan, MtgPortfolioImport.ErrorStatus, 0, cancellationToken);
            return plan.ToResult(MtgPortfolioImport.ErrorStatus, fileName, 0);
        }

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);

        // Investors and units are matched by code; only the ones not yet in the system are added (APEX MERGE ... when not matched).
        foreach (var investor in plan.NewInvestors)
            db.Add(investor);
        foreach (var unit in plan.NewUnits)
            db.Add(unit);
        await db.SaveChangesAsync(cancellationToken);

        var investorIds = await InvestorIdsAsync(plan.Accounts.Select(a => a.InvestorCode), cancellationToken);
        var unitIds = await UnitIdsAsync(plan.ProjectId, plan.Accounts.Select(a => a.UnitCode), cancellationToken);
        foreach (var a in plan.Accounts)
        {
            a.Account.InvestorId = investorIds[a.InvestorCode];
            a.Account.ProjectUnitId = unitIds[a.UnitCode];
            db.Add(a.Account);
        }
        await db.SaveChangesAsync(cancellationToken);

        await PortfolioTotals.RecalculateAsync(db, portfolioId, cancellationToken);
        await SaveBatchAsync(portfolioId, fileName, plan, MtgPortfolioImport.ImportedStatus, plan.Accounts.Count, cancellationToken);
        await MoveToDataLoadedAsync(portfolioId, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return plan.ToResult(MtgPortfolioImport.ImportedStatus, fileName, plan.Accounts.Count);
    }

    /// <summary>APEX "Validate Portfolio Workbook", plus the checks the APEX import left to the database.</summary>
    private async Task<ImportPlan> AnalyseAsync(long portfolioId, string fileName, Stream content, CancellationToken cancellationToken)
    {
        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            throw BusinessValidationException.For("file", "Choose an Excel workbook (.xlsx).");

        var portfolio = await (
            from p in db.Set<MtgPortfolio>()
            join prj in db.Set<MtgProject>() on p.ProjectId equals prj.ProjectId
            where p.PortfolioId == portfolioId
            select new { p.PortfolioNo, p.ProjectId, prj.ProjectCode }).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Portfolio {portfolioId} was not found.");

        var status = await db.Set<MtgPortfolioImport>()
            .Where(x => x.PortfolioId == portfolioId).Select(x => x.ImportStatusCode).FirstOrDefaultAsync(cancellationToken);
        if (status == MtgPortfolioImport.ImportedStatus)
            throw new ConflictException("The workbook of this portfolio has already been imported.");

        IReadOnlyDictionary<string, IReadOnlyList<WorkbookRow>> sheets;
        try
        {
            sheets = reader.Read(content);
        }
        catch (InvalidDataException ex)
        {
            throw BusinessValidationException.For("file", ex.Message);
        }

        if (!sheets.ContainsKey(InvestorsSheet) || !sheets.ContainsKey(UnitsSheet) || !sheets.ContainsKey(AccountsSheet))
            throw BusinessValidationException.For("file",
                $"Workbook must contain worksheet tabs named INVESTORS, UNITS and ACCOUNTS. Tabs found: {(sheets.Count == 0 ? "none" : string.Join(", ", sheets.Keys))}");

        var plan = new ImportPlan(portfolio.ProjectId);
        var context = new SheetContext(portfolio.PortfolioNo, portfolio.ProjectCode);

        await CheckInvestorsAsync(sheets[InvestorsSheet], context, plan, cancellationToken);
        await CheckUnitsAsync(sheets[UnitsSheet], context, portfolio.ProjectId, plan, cancellationToken);
        await CheckAccountsAsync(sheets[AccountsSheet], context, portfolioId, plan, cancellationToken);
        return plan;
    }

    private async Task CheckInvestorsAsync(
        IReadOnlyList<WorkbookRow> rows, SheetContext context, ImportPlan plan, CancellationToken cancellationToken)
    {
        var codes = rows.Select(r => Code(r, Investor.Code)).OfType<string>().Distinct().ToList();
#pragma warning disable CA1862 // EF Core translates ToUpper to SQL upper(); string.Equals with a comparison type is not translatable.
        var existing = (await db.Set<MtgInvestor>()
            .Where(x => codes.Contains(x.InvestorCode.ToUpper()))
            .Select(x => x.InvestorCode.ToUpper())
            .ToListAsync(cancellationToken)).ToHashSet();
#pragma warning restore CA1862

        foreach (var row in rows)
        {
            var errors = context.CheckCase(row);
            var code = Code(row, Investor.Code);
            var nameAr = Text(row, Investor.NameAr);
            var nameEn = Text(row, Investor.NameEn);

            if (code is null)
                errors.Add("INVESTOR_CODE is required.");
            else if (!plan.InvestorCodes.Add(code))
                errors.Add("Duplicate INVESTOR_CODE.");
            else if (code.Length > 30)
                errors.Add("INVESTOR_CODE is limited to 30 characters.");

            var isNew = code is not null && !existing.Contains(code);
            var birthDate = Date(row, Investor.BirthDate, "DATE_OF_BIRTH", errors);
            var income = Number(row, Investor.DeclaredIncome, "DECLARED_INCOME", errors);
            if (isNew)
            {
                // A new investor is created from this row, so it must satisfy the investor form (page 110).
                if (nameAr is null)
                    errors.Add("FULL_NAME_AR is required for a new investor.");
                MaxLength(errors, "FULL_NAME_AR", nameAr, 300);
                MaxLength(errors, "FULL_NAME_EN", nameEn, 300);
                MaxLength(errors, "NATIONAL_ID", Text(row, Investor.NationalId), 50);
                MaxLength(errors, "PHONE_NO", Text(row, Investor.Phone), 100);
                MaxLength(errors, "EMAIL_ADDRESS", Text(row, Investor.Email), 320);
                MaxLength(errors, "ADDRESS", Text(row, Investor.Address), 1000);
                MaxLength(errors, "EMPLOYER_NAME", Text(row, Investor.Employer), 300);
                NotNegative(errors, "DECLARED_INCOME", income);
            }

            plan.AddRow(InvestorsSheet, row, "Investor", code, nameEn ?? nameAr, errors,
                isNew ? null : "Row is valid. The investor already exists and is kept as it is.");

            if (errors.Count == 0 && isNew)
            {
                plan.NewInvestors.Add(new MtgInvestor
                {
                    InvestorCode = code!,
                    FullNameAr = nameAr!,
                    FullNameEn = nameEn,
                    NationalIdNo = Text(row, Investor.NationalId),
                    BirthDate = birthDate,
                    PhoneNo = Text(row, Investor.Phone),
                    EmailAddress = Text(row, Investor.Email),
                    AddressText = Text(row, Investor.Address),
                    EmployerName = Text(row, Investor.Employer),
                    DeclaredIncomeAmount = income,
                    IsActive = IsActive(row, Investor.ActiveFlag)
                });
            }
        }
    }

    private async Task CheckUnitsAsync(
        IReadOnlyList<WorkbookRow> rows, SheetContext context, long projectId, ImportPlan plan, CancellationToken cancellationToken)
    {
        var codes = rows.Select(r => Code(r, Unit.Code)).OfType<string>().Distinct().ToList();
#pragma warning disable CA1862 // EF Core translates ToUpper to SQL upper().
        var existing = (await db.Set<MtgProjectUnit>()
            .Where(x => x.ProjectId == projectId && codes.Contains(x.UnitCode.ToUpper()))
            .Select(x => x.UnitCode.ToUpper())
            .ToListAsync(cancellationToken)).ToHashSet();
#pragma warning restore CA1862
        var unitTypes = (await db.Set<MtgLkpUnitType>().Where(x => x.IsActive).Select(x => x.Code).ToListAsync(cancellationToken)).ToHashSet();

        foreach (var row in rows)
        {
            var errors = context.CheckCase(row);
            var code = Code(row, Unit.Code);

            if (code is null)
                errors.Add("UNIT_CODE is required.");
            else if (!plan.UnitCodes.Add(code))
                errors.Add("Duplicate UNIT_CODE.");
            else if (code.Length > 50)
                errors.Add("UNIT_CODE is limited to 50 characters.");

            var isNew = code is not null && !existing.Contains(code);
            var type = Code(row, Unit.TypeCode);
            var area = Number(row, Unit.AreaSqm, "AREA_SQM", errors);
            var contract = Number(row, Unit.ContractValue, "CONTRACT_VALUE", errors);
            var market = Number(row, Unit.MarketValue, "CURRENT_MARKET_VALUE", errors);
            if (isNew)
            {
                // A new unit is created in the portfolio's project, so it must satisfy the unit form (page 108).
                if (type is not null && !unitTypes.Contains(type))
                    errors.Add($"UNIT_TYPE_CODE '{type}' is not an active unit type.");
                MaxLength(errors, "BUILDING_NO", Text(row, Unit.BuildingNo), 50);
                MaxLength(errors, "FLOOR_NO", Text(row, Unit.FloorNo), 50);
                MaxLength(errors, "UNIT_NO", Text(row, Unit.UnitNo), 50);
                MaxLength(errors, "LICENSE_STATUS", Text(row, Unit.LicenseStatus), 50);
                NotNegative(errors, "AREA_SQM", area);
                NotNegative(errors, "CONTRACT_VALUE", contract);
                NotNegative(errors, "CURRENT_MARKET_VALUE", market);
            }

            var name = string.Join(' ', new[] { Text(row, Unit.BuildingNo), Text(row, Unit.FloorNo), Text(row, Unit.UnitNo) }.OfType<string>());
            plan.AddRow(UnitsSheet, row, "Unit", code, name.Length == 0 ? null : name, errors,
                isNew ? null : "Row is valid. The unit already exists in the project and is kept as it is.");

            if (errors.Count == 0 && isNew)
            {
                plan.NewUnits.Add(new MtgProjectUnit
                {
                    ProjectId = projectId,
                    UnitCode = code!,
                    UnitTypeCode = type,
                    BuildingNo = Text(row, Unit.BuildingNo),
                    FloorNo = Text(row, Unit.FloorNo),
                    UnitNo = Text(row, Unit.UnitNo),
                    AreaSqm = area,
                    ContractAmount = contract,
                    MarketValueAmount = market,
                    LicenseStatus = Text(row, Unit.LicenseStatus),
                    IsActive = IsActive(row, Unit.ActiveFlag)
                });
            }
        }
    }

    private async Task CheckAccountsAsync(
        IReadOnlyList<WorkbookRow> rows, SheetContext context, long portfolioId, ImportPlan plan, CancellationToken cancellationToken)
    {
#pragma warning disable CA1862 // EF Core translates ToUpper to SQL upper().
        var existing = (await db.Set<MtgPortfolioAccount>()
            .Where(x => x.PortfolioId == portfolioId && x.SellerContractNo != null)
            .Select(x => x.SellerContractNo!.ToUpper())
            .ToListAsync(cancellationToken)).ToHashSet();
#pragma warning restore CA1862
        var contracts = new HashSet<string>();

        foreach (var row in rows)
        {
            var errors = context.CheckCase(row);
            var contractNo = Code(row, Account.SourceContractNo);
            var investor = Code(row, Account.InvestorCode);
            var unit = Code(row, Account.UnitCode);

            if (contractNo is null)
                errors.Add("SOURCE_CONTRACT_NO is required.");
            else if (!contracts.Add(contractNo))
                errors.Add("Duplicate SOURCE_CONTRACT_NO.");
            else if (contractNo.Length > 100)
                errors.Add("SOURCE_CONTRACT_NO is limited to 100 characters.");
            if (investor is null || !plan.InvestorCodes.Contains(investor))
                errors.Add("INVESTOR_CODE does not exist in INVESTORS sheet.");
            if (unit is null || !plan.UnitCodes.Contains(unit))
                errors.Add("UNIT_CODE does not exist in UNITS sheet.");
            if (contractNo is not null && existing.Contains(contractNo))
                errors.Add("Account number already exists.");

            // The account must satisfy the account form (page 205).
            var bookingDate = Date(row, Account.BookingDate, "BOOKING_DATE", errors);
            var contractDate = Date(row, Account.ContractDate, "CONTRACT_DATE", errors);
            var lastPayment = Date(row, Account.LastPaymentDate, "LAST_PAYMENT_DATE", errors);
            var contract = Required(errors, "CONTRACT_VALUE", Number(row, Account.ContractValue, "CONTRACT_VALUE", errors), row, Account.ContractValue);
            var paid = Required(errors, "PAID_VALUE", Number(row, Account.PaidValue, "PAID_VALUE", errors), row, Account.PaidValue);
            var outstanding = Required(errors, "OUTSTANDING_VALUE", Number(row, Account.OutstandingValue, "OUTSTANDING_VALUE", errors), row, Account.OutstandingValue);
            var installment = Number(row, Account.InstallmentAmount, "INSTALLMENT_AMOUNT", errors);
            var months = Number(row, Account.RegularPaymentMonths, "REGULAR_PAYMENT_MONTHS", errors);
            var arrears = Number(row, Account.ArrearsAmount, "ARREARS_AMOUNT", errors);
            NotNegative(errors, "CONTRACT_VALUE", contract);
            NotNegative(errors, "PAID_VALUE", paid);
            NotNegative(errors, "OUTSTANDING_VALUE", outstanding);
            NotNegative(errors, "INSTALLMENT_AMOUNT", installment);
            NotNegative(errors, "ARREARS_AMOUNT", arrears);
            if (months is { } m && (m < 0 || m > int.MaxValue || m != decimal.Truncate(m)))
                errors.Add("REGULAR_PAYMENT_MONTHS must be a whole number of 0 or more.");
            MaxLength(errors, "INSTALLMENT_FREQUENCY", Text(row, Account.InstallmentFrequency), 30);
            MaxLength(errors, "ACCOUNT_STATUS", Text(row, Account.AccountStatus), 30);

            plan.AddRow(AccountsSheet, row, "Account", contractNo, $"{investor} / {unit}", errors, null);

            if (errors.Count == 0)
            {
                plan.Accounts.Add(new PlannedAccount(investor!, unit!, new MtgPortfolioAccount
                {
                    PortfolioId = portfolioId,
                    SellerContractNo = contractNo,
                    BookingDate = bookingDate,
                    ContractDate = contractDate,
                    ContractAmount = contract!.Value,
                    PaidAmount = paid!.Value,
                    OutstandingAmount = outstanding!.Value,
                    InstallmentAmount = installment,
                    InstallmentFrequency = Text(row, Account.InstallmentFrequency),
                    LastPaymentDate = lastPayment,
                    RegularPaymentMonths = (int)(months ?? 0),
                    ArrearsAmount = arrears ?? 0,
                    AccountStatus = Text(row, Account.AccountStatus) ?? "ACTIVE"
                }));
            }
        }
    }

    private static decimal? Required(List<string> errors, string label, decimal? value, WorkbookRow row, int column)
    {
        // Only report "required" when the cell is blank; an unreadable value already has its own message.
        if (value is null && Text(row, column) is null)
            errors.Add($"{label} is required.");
        return value;
    }

    private static void NotNegative(List<string> errors, string label, decimal? value)
    {
        if (value is { } v && (v < 0 || v > MaxAmount))
            errors.Add($"{label} must be 0 or more.");
    }

    private static void MaxLength(List<string> errors, string label, string? value, int maxLength)
    {
        if (value?.Length > maxLength)
            errors.Add($"{label} is limited to {maxLength} characters.");
    }

    private async Task<Dictionary<string, long>> InvestorIdsAsync(IEnumerable<string> codes, CancellationToken cancellationToken)
    {
        var wanted = codes.Distinct().ToList();
#pragma warning disable CA1862 // EF Core translates ToUpper to SQL upper().
        var rows = await db.Set<MtgInvestor>()
            .Where(x => wanted.Contains(x.InvestorCode.ToUpper()))
            .Select(x => new { Code = x.InvestorCode.ToUpper(), x.InvestorId })
            .ToListAsync(cancellationToken);
#pragma warning restore CA1862
        return rows.GroupBy(x => x.Code).ToDictionary(g => g.Key, g => g.Min(x => x.InvestorId));
    }

    private async Task<Dictionary<string, long>> UnitIdsAsync(long projectId, IEnumerable<string> codes, CancellationToken cancellationToken)
    {
        var wanted = codes.Distinct().ToList();
#pragma warning disable CA1862 // EF Core translates ToUpper to SQL upper().
        var rows = await db.Set<MtgProjectUnit>()
            .Where(x => x.ProjectId == projectId && wanted.Contains(x.UnitCode.ToUpper()))
            .Select(x => new { Code = x.UnitCode.ToUpper(), x.ProjectUnitId })
            .ToListAsync(cancellationToken);
#pragma warning restore CA1862
        return rows.GroupBy(x => x.Code).ToDictionary(g => g.Key, g => g.Min(x => x.ProjectUnitId));
    }

    private async Task SaveBatchAsync(
        long portfolioId, string fileName, ImportPlan plan, string status, int imported, CancellationToken cancellationToken)
    {
        var batch = await db.Set<MtgPortfolioImport>().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, cancellationToken);
        if (batch is null)
        {
            batch = new MtgPortfolioImport { PortfolioId = portfolioId };
            db.Add(batch);
        }

        batch.FileName = fileName.Length > 500 ? fileName[..500] : fileName;
        batch.ImportStatusCode = status;
        batch.TotalRowCount = plan.Rows.Count;
        batch.ValidRowCount = plan.Rows.Count(x => x.IsValid);
        batch.ErrorRowCount = plan.Rows.Count(x => !x.IsValid);
        batch.ImportedRowCount = imported;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task MoveToDataLoadedAsync(long portfolioId, CancellationToken cancellationToken)
    {
        var portfolio = await db.Set<MtgPortfolio>().FirstAsync(x => x.PortfolioId == portfolioId, cancellationToken);
        if (portfolio.PortfolioStatusCode == MtgPortfolio.DataLoadingCompletedStatus)
            return;

        db.Add(new MtgPortfolioStatusHistory
        {
            PortfolioId = portfolioId,
            FromStatusCode = portfolio.PortfolioStatusCode,
            ToStatusCode = MtgPortfolio.DataLoadingCompletedStatus,
            Comments = "Excel portfolio data loaded successfully."
        });
        portfolio.PortfolioStatusCode = MtgPortfolio.DataLoadingCompletedStatus;
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>The portfolio number and project code every row must carry.</summary>
    private sealed record SheetContext(string PortfolioNo, string ProjectCode)
    {
        public List<string> CheckCase(WorkbookRow row)
        {
            var errors = new List<string>();
            if (!string.Equals(Text(row, PortfolioWorkbook.PortfolioNo), PortfolioNo, StringComparison.OrdinalIgnoreCase))
                errors.Add("Portfolio number does not match the selected portfolio.");
            if (!string.Equals(Text(row, PortfolioWorkbook.ProjectCode), ProjectCode, StringComparison.OrdinalIgnoreCase))
                errors.Add("Project code does not match the selected portfolio project.");
            return errors;
        }
    }

    private sealed record PlannedAccount(string InvestorCode, string UnitCode, MtgPortfolioAccount Account);

    private sealed class ImportPlan(long projectId)
    {
        public long ProjectId { get; } = projectId;
        public HashSet<string> InvestorCodes { get; } = [];
        public HashSet<string> UnitCodes { get; } = [];
        public List<MtgInvestor> NewInvestors { get; } = [];
        public List<MtgProjectUnit> NewUnits { get; } = [];
        public List<PlannedAccount> Accounts { get; } = [];
        public List<WorkbookRowResultDto> Rows { get; } = [];

        /// <summary>APEX sets VALIDATED only when there is at least one row and no errors.</summary>
        public bool IsValid => Rows.Count > 0 && Rows.All(x => x.IsValid);

        public void AddRow(string sheet, WorkbookRow row, string entity, string? code, string? name, List<string> errors, string? validMessage) =>
            Rows.Add(new WorkbookRowResultDto(
                sheet, row.RowNumber, entity, code, name, errors.Count == 0,
                errors.Count == 0 ? validMessage ?? "Row is valid." : string.Join(' ', errors)));

        public WorkbookResultDto ToResult(string status, string fileName, int imported) =>
            new(status, fileName, Rows.Count, Rows.Count(x => x.IsValid), Rows.Count(x => !x.IsValid), imported,
                [.. new[] { InvestorsSheet, UnitsSheet, AccountsSheet }.Select(sheet =>
                {
                    var rows = Rows.Where(x => x.Sheet == sheet).ToList();
                    return new WorkbookSheetResultDto(sheet, rows.Count, rows.Count(x => x.IsValid), rows.Count(x => !x.IsValid));
                })],
                Rows);
    }
}
