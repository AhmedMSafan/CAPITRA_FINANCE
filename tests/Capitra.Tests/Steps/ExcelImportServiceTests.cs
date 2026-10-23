using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Mtg.PortfolioSteps.ExcelImport;
using Capitra.Domain.Modules.Mtg;
using Capitra.Infrastructure.Files;
using Capitra.Infrastructure.Persistence;
using Capitra.Infrastructure.Persistence.Interceptors;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Capitra.Tests.Steps;

/// <summary>Workflow step "Excel Import &amp; Validation" (APEX page 208) against an in-memory database and real .xlsx files.</summary>
public sealed class ExcelImportServiceTests
{
    private const string PortfolioNo = "PF-2026-000001";
    private const string ProjectCode = "PRJ1";

    private static readonly string[] InvestorHeader =
        ["PORTFOLIO_NO", "PROJECT_CODE", "INVESTOR_CODE", "FULL_NAME_AR", "FULL_NAME_EN", "NATIONAL_ID", "DATE_OF_BIRTH",
         "PHONE_NO", "EMAIL_ADDRESS", "ADDRESS", "EMPLOYER_NAME", "DECLARED_INCOME", "ACTIVE_FLAG"];

    private static readonly string[] UnitHeader =
        ["PORTFOLIO_NO", "PROJECT_CODE", "UNIT_CODE", "UNIT_TYPE_CODE", "BUILDING_NO", "FLOOR_NO", "UNIT_NO", "AREA_SQM",
         "CONTRACT_VALUE", "CURRENT_MARKET_VALUE", "LICENSE_STATUS", "ACTIVE_FLAG"];

    private static readonly string[] AccountHeader =
        ["PORTFOLIO_NO", "PROJECT_CODE", "SOURCE_CONTRACT_NO", "INVESTOR_CODE", "UNIT_CODE", "BOOKING_DATE", "CONTRACT_DATE",
         "CONTRACT_VALUE", "PAID_VALUE", "OUTSTANDING_VALUE", "INSTALLMENT_AMOUNT", "INSTALLMENT_FREQUENCY",
         "LAST_PAYMENT_DATE", "REGULAR_PAYMENT_MONTHS", "ARREARS_AMOUNT", "ACCOUNT_STATUS"];

    private sealed class FakeUser : ICurrentUser
    {
        public string? UserId => "u1";
        public string? UserName => "loader";
        public bool IsAuthenticated => true;
    }

    private sealed class FakeClock : IClock
    {
        public DateTime UtcNow => new(2026, 10, 23, 8, 0, 0, DateTimeKind.Utc);
    }

    private static CapitraDbContext CreateContext(string database) =>
        new(new DbContextOptionsBuilder<CapitraDbContext>()
            .UseInMemoryDatabase(database)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .AddInterceptors(new AuditInterceptor(new FakeUser(), new FakeClock()))
            .Options);

    /// <summary>A draft portfolio on project PRJ1, with investor "Inv1" and unit U-1 already in the system.</summary>
    private static async Task<(string Database, long PortfolioId)> SeedAsync()
    {
        var database = Guid.NewGuid().ToString();
        await using var db = CreateContext(database);
        var developer = new MtgDeveloper { DeveloperCode = "DEV1", LegalNameAr = "مطور" };
        db.Add(developer);
        db.Add(new MtgLkpUnitType { Code = "APT", NameAr = "شقة", NameEn = "Apartment" });
        await db.SaveChangesAsync();
        var project = new MtgProject { DeveloperId = developer.DeveloperId, ProjectCode = ProjectCode, ProjectNameAr = "مشروع" };
        db.Add(project);
        db.Add(new MtgInvestor { InvestorCode = "Inv1", FullNameAr = "مستثمر قائم" });
        await db.SaveChangesAsync();
        db.Add(new MtgProjectUnit { ProjectId = project.ProjectId, UnitCode = "U-1" });
        var portfolio = new MtgPortfolio
        {
            PortfolioNo = PortfolioNo, DeveloperId = developer.DeveloperId, ProjectId = project.ProjectId,
            RequestDate = new DateOnly(2026, 10, 1), CurrencyCode = "EGP"
        };
        db.Add(portfolio);
        await db.SaveChangesAsync();
        return (database, portfolio.PortfolioId);
    }

    private static MemoryStream Workbook(object?[][] investors, object?[][] units, object?[][] accounts)
    {
        using var workbook = new XLWorkbook();
        AddSheet(workbook, "INVESTORS", InvestorHeader, investors);
        AddSheet(workbook, "Units", UnitHeader, units);
        AddSheet(workbook, "ACCOUNTS", AccountHeader, accounts);
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    private static void AddSheet(XLWorkbook workbook, string name, string[] header, object?[][] rows)
    {
        var sheet = workbook.AddWorksheet(name);
        for (var c = 0; c < header.Length; c++)
            sheet.Cell(1, c + 1).Value = header[c];
        for (var r = 0; r < rows.Length; r++)
            for (var c = 0; c < rows[r].Length; c++)
                sheet.Cell(r + 2, c + 1).Value = XLCellValue.FromObject(rows[r][c]);
    }

    private static MemoryStream ValidWorkbook() => Workbook(
        [
            [PortfolioNo, ProjectCode, "INV1", null, null],
            [PortfolioNo, ProjectCode, "inv2", "مستثمر جديد", "New Investor", "290", new DateTime(1990, 1, 1), null, null, null, null, 120000]
        ],
        [
            [PortfolioNo, ProjectCode, "u-1"],
            [PortfolioNo, ProjectCode, "U-2", "APT", "B1", "2", "201", 110.5, 2000000]
        ],
        [
            [PortfolioNo, ProjectCode, "SC-1", "INV1", "U-1", "2024-01-10", new DateTime(2024, 2, 1), 1500000, 500000, 1000000, 25000, "MONTHLY", null, 12, 0],
            [PortfolioNo, ProjectCode, "SC-2", "INV2", "U-2", null, null, "2,000,000", 400000, 1600000, null, null, null, null, 30000, "LATE"]
        ]);

    [Fact]
    public async Task Imports_a_valid_workbook_and_completes_the_step()
    {
        var (database, portfolioId) = await SeedAsync();

        await using (var db = CreateContext(database))
        {
            var service = new MtgExcelImportService(db, new ExcelWorkbookReader());
            using var file = ValidWorkbook();
            var result = await service.ImportAsync(portfolioId, "seller.xlsx", file, CancellationToken.None);

            Assert.Equal(MtgPortfolioImport.ImportedStatus, result.ImportStatusCode);
            Assert.Equal(0, result.ErrorRowCount);
            Assert.Equal(2, result.ImportedRowCount);
        }

        await using (var db = CreateContext(database))
        {
            // Existing investor and unit are matched by code whatever the case; only the new ones are added.
            Assert.Equal(["INV2", "Inv1"], (await db.Set<MtgInvestor>().Select(x => x.InvestorCode).ToListAsync()).Order(StringComparer.Ordinal));
            Assert.Equal(["U-1", "U-2"], (await db.Set<MtgProjectUnit>().Select(x => x.UnitCode).ToListAsync()).Order());
            var newInvestor = await db.Set<MtgInvestor>().SingleAsync(x => x.InvestorCode == "INV2");
            Assert.Equal(new DateOnly(1990, 1, 1), newInvestor.BirthDate);
            Assert.Equal(120000m, newInvestor.DeclaredIncomeAmount);

            var accounts = await db.Set<MtgPortfolioAccount>().OrderBy(x => x.SellerContractNo).ToListAsync();
            Assert.Equal(["SC-1", "SC-2"], accounts.Select(x => x.SellerContractNo));
            Assert.Equal(new DateOnly(2024, 1, 10), accounts[0].BookingDate);
            Assert.Equal(12, accounts[0].RegularPaymentMonths);
            Assert.Equal(2000000m, accounts[1].ContractAmount);
            Assert.Equal(0, accounts[1].RegularPaymentMonths);
            Assert.Equal("LATE", accounts[1].AccountStatus);

            var portfolio = await db.Set<MtgPortfolio>().SingleAsync();
            Assert.Equal(3500000m, portfolio.FaceValueAmount);
            Assert.Equal(2600000m, portfolio.OutstandingAmount);
            Assert.Equal(MtgPortfolio.DataLoadingCompletedStatus, portfolio.PortfolioStatusCode);

            var history = await db.Set<MtgPortfolioStatusHistory>().SingleAsync();
            Assert.Equal(MtgPortfolio.DraftStatus, history.FromStatusCode);
            Assert.Equal(MtgPortfolio.DataLoadingCompletedStatus, history.ToStatusCode);
            Assert.Equal("loader", history.CreatedBy);

            var batch = await db.Set<MtgPortfolioImport>().SingleAsync();
            Assert.Equal(MtgPortfolioImport.ImportedStatus, batch.ImportStatusCode);
            Assert.Equal(6, batch.TotalRowCount);
            Assert.Equal(2, batch.ImportedRowCount);
        }

        await using (var db = CreateContext(database))
        {
            var service = new MtgExcelImportService(db, new ExcelWorkbookReader());
            using var again = ValidWorkbook();
            await Assert.ThrowsAsync<ConflictException>(() => service.ValidateAsync(portfolioId, "seller.xlsx", again, CancellationToken.None));
        }
    }

    [Fact]
    public async Task A_workbook_with_errors_is_not_imported()
    {
        var (database, portfolioId) = await SeedAsync();
        using var file = Workbook(
            [
                [PortfolioNo, ProjectCode, "INV9", null],
                ["PF-OTHER", ProjectCode, "INV8", "اسم"]
            ],
            [[PortfolioNo, ProjectCode, "U-9", "VILLA"]],
            [
                [PortfolioNo, ProjectCode, "SC-1", "INV9", "U-9", "31/02/2024", null, "abc", -1, 100],
                [PortfolioNo, ProjectCode, "SC-1", "NOPE", "U-1", null, null, 1, 1, 1, null, null, null, 1.5]
            ]);

        await using var db = CreateContext(database);
        var service = new MtgExcelImportService(db, new ExcelWorkbookReader());
        var result = await service.ImportAsync(portfolioId, "seller.xlsx", file, CancellationToken.None);

        Assert.Equal(MtgPortfolioImport.ErrorStatus, result.ImportStatusCode);
        Assert.Equal(0, result.ImportedRowCount);
        string MessageOf(string sheet, int row) => result.Rows.Single(r => r.Sheet == sheet && r.RowNumber == row).Message;
        Assert.Contains("FULL_NAME_AR is required for a new investor.", MessageOf("INVESTORS", 2), StringComparison.Ordinal);
        Assert.Contains("Portfolio number does not match", MessageOf("INVESTORS", 3), StringComparison.Ordinal);
        Assert.Contains("UNIT_TYPE_CODE 'VILLA' is not an active unit type.", MessageOf("UNITS", 2), StringComparison.Ordinal);
        Assert.Contains("BOOKING_DATE is not a date", MessageOf("ACCOUNTS", 2), StringComparison.Ordinal);
        Assert.Contains("CONTRACT_VALUE is not a number.", MessageOf("ACCOUNTS", 2), StringComparison.Ordinal);
        Assert.Contains("PAID_VALUE must be 0 or more.", MessageOf("ACCOUNTS", 2), StringComparison.Ordinal);
        Assert.Contains("Duplicate SOURCE_CONTRACT_NO.", MessageOf("ACCOUNTS", 3), StringComparison.Ordinal);
        Assert.Contains("INVESTOR_CODE does not exist in INVESTORS sheet.", MessageOf("ACCOUNTS", 3), StringComparison.Ordinal);
        Assert.Contains("UNIT_CODE does not exist in UNITS sheet.", MessageOf("ACCOUNTS", 3), StringComparison.Ordinal);
        Assert.Contains("REGULAR_PAYMENT_MONTHS must be a whole number", MessageOf("ACCOUNTS", 3), StringComparison.Ordinal);

        Assert.Empty(await db.Set<MtgPortfolioAccount>().ToListAsync());
        Assert.Single(await db.Set<MtgInvestor>().ToListAsync());
        Assert.Equal(MtgPortfolio.DraftStatus, (await db.Set<MtgPortfolio>().SingleAsync()).PortfolioStatusCode);
        Assert.Equal(MtgPortfolioImport.ErrorStatus, (await db.Set<MtgPortfolioImport>().SingleAsync()).ImportStatusCode);
    }

    [Fact]
    public async Task A_workbook_without_the_three_tabs_is_refused()
    {
        var (database, portfolioId) = await SeedAsync();
        using var workbook = new XLWorkbook();
        workbook.AddWorksheet("Sheet1");
        using var file = new MemoryStream();
        workbook.SaveAs(file);
        file.Position = 0;

        await using var db = CreateContext(database);
        var service = new MtgExcelImportService(db, new ExcelWorkbookReader());
        var error = await Assert.ThrowsAsync<BusinessValidationException>(
            () => service.ValidateAsync(portfolioId, "seller.xlsx", file, CancellationToken.None));
        Assert.Contains("INVESTORS, UNITS and ACCOUNTS", error.Errors["file"][0], StringComparison.Ordinal);
    }
}
