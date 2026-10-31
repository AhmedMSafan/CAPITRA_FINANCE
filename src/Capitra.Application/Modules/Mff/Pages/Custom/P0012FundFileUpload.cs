using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>
/// App 110 page 12, After Submit "Upload and Parse Fund Excel" (UPLOAD): the first worksheet after its header row
/// (apex_data_parser, p_skip_rows 1), one staging row per row with an applicant name (column 2).
/// </summary>
public sealed class P0012FundFileUpload(ICapitraDbContext db, ApexSession session, IWorkbookReader workbooks, IClock clock) : IApexProcess
{
    public int App => 110;

    public int Page => 12;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") != "UPLOAD")
            return Task.FromResult<object>(new { ok = true });
        return MffForm.RunAsync(db, async () =>
        {
            var file = session.File(Text(request, "P12_FILE"))
                ?? throw new ApexProcessException("ORA-20060: The selected upload file was not found in APEX temporary files.");
            IReadOnlyList<WorkbookRow> rows;
            try
            {
                using var stream = new MemoryStream(file.Content);
                rows = workbooks.Read(stream).Values.FirstOrDefault() ?? [];
            }
            catch (InvalidDataException)
            {
                throw new ApexProcessException("ORA-20999: The file is not a valid Excel workbook.");
            }

            var batch = new MffImportBatch
            {
                SourceChannelCode = "FUND_FILE",
                FileName = file.FileName,
                ReceivedAt = clock.UtcNow,
                ProgramVersionId = Long(request, "P12_PROGRAM_VERSION_ID"),
                FinancierId = Long(request, "P12_FINANCIER_ID"),
                FundOrgId = Long(request, "P12_FUND_ORG_ID"),
                BatchStatus = "STAGED",
                Notes = Text(request, "P12_NOTES"),
            };
            db.Add(batch);
            await db.SaveChangesAsync(ct);

            var n = 0;
            foreach (var r in rows.Where(r => !string.IsNullOrWhiteSpace(r[1])))
            {
                string? T(int c) => string.IsNullOrWhiteSpace(r[c]) ? null : r[c]!.Trim();
                db.Add(new MffImportStaging
                {
                    BatchId = batch.BatchId,
                    SourceRowNo = (int?)Num(r[0]) ?? r.RowNumber,
                    ApplicantNameAr = T(1),
                    ApplicantNationalId = T(2),
                    SpouseNameAr = T(3),
                    SpouseNationalId = T(4),
                    MaritalStatusRaw = T(5),
                    MonthlyIncome = Num(r[6]),
                    CustomerDownpayment = Num(r[7]),
                    TotalSubsidy = Num(r[8]),
                    UnitPrice = Num(r[9]),
                    MaintenanceDeposit = Num(r[10]),
                    FinanceAmount = Num(r[11]),
                    AnnouncementRaw = T(12),
                    ProjectRaw = T(13),
                    GovernorateRaw = T(14),
                    CityRaw = T(15),
                    DistrictRaw = T(16),
                    AreaSqm = Num(r[17]),
                    NeighborhoodRaw = T(18),
                    BuildingNo = T(19),
                    FloorNo = T(20),
                    UnitNo = T(21),
                    SiteRaw = T(22),
                    MobileNo = T(23),
                    ValidationStatus = "VALID",
                });
                n++;
            }

            batch.RowCount = n;
            batch.ValidCount = n;
            batch.BatchStatus = "READY";
            await db.SaveChangesAsync(ct);
            return ("Fund file loaded successfully. Batch ID: " + Id(batch.BatchId), new Dictionary<string, string?> { ["P12_BATCH_ID"] = Id(batch.BatchId) });
        }, ct);
    }

    /// <summary>num(): a number with thousands separators, null when empty or not a number.</summary>
    private static decimal? Num(string? p) =>
        string.IsNullOrWhiteSpace(p) ? null
        : decimal.TryParse(p.Trim().Replace(",", string.Empty, StringComparison.Ordinal), NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : null;
}
