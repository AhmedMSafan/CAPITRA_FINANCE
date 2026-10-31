using System.Text.RegularExpressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mff.Packages;

/// <summary>MFF_IMPORT_PKG: the checks of an uploaded Fund file (MFF_IMPORT_STAGINGS) and the counts of its batch.</summary>
public sealed partial class MffImportPackage(ICapitraDbContext db, ICurrentUser user, IClock clock)
{
    /// <summary>VALIDATE_BATCH: marks each row of the batch VALID or ERROR with its messages, then the batch's counts and status.</summary>
    public async Task ValidateBatchAsync(long batchId, CancellationToken ct)
    {
        var rows = await db.Set<MffImportStaging>().Where(x => x.BatchId == batchId).OrderBy(x => x.SourceRowNo).ToListAsync(ct);
        foreach (var r in rows)
        {
            var err = new List<string>();
            if (string.IsNullOrWhiteSpace(r.ApplicantNameAr))
                err.Add("اسم المتعاقد مطلوب.");
            if (string.IsNullOrEmpty(r.ApplicantNationalId))
                err.Add("الرقم القومي مطلوب.");
            else if (r.ApplicantNationalId.Trim().Length > 0 && !NationalId().IsMatch(r.ApplicantNationalId.Trim()))
                err.Add("الرقم القومي يجب أن يتكون من 14 رقمًا.");
            if ((r.MonthlyIncome ?? 0) <= 0)
                err.Add("الدخل الشهري يجب أن يكون أكبر من صفر.");
            if ((r.UnitPrice ?? 0) <= 0)
                err.Add("سعر الوحدة مطلوب.");
            if ((r.CustomerDownpayment ?? 0) < 0 || (r.TotalSubsidy ?? 0) < 0 || (r.FinanceAmount ?? 0) < 0)
                err.Add("قيم المقدم/الدعم/التمويل لا يجوز أن تكون سالبة.");

            // the Fund file balances as: downpayment + subsidy + finance = unit price + maintenance deposit
            var expected = (r.UnitPrice ?? 0) + (r.MaintenanceDeposit ?? 0);
            var actual = (r.CustomerDownpayment ?? 0) + (r.TotalSubsidy ?? 0) + (r.FinanceAmount ?? 0);
            if (Math.Abs(expected - actual) > 1)
                err.Add("إجمالي المقدم + الدعم + التمويل لا يساوي سعر الوحدة + وديعة الصيانة.");

            var id = r.ApplicantNationalId?.Trim();
            if (!string.IsNullOrEmpty(id) && rows.Any(x => x.StagingId != r.StagingId && x.ApplicantNationalId?.Trim() == id))
                err.Add("الرقم القومي مكرر داخل نفس الملف.");

            r.ValidationStatus = err.Count == 0 ? "VALID" : "ERROR";
            r.ErrorMessage = err.Count == 0 ? null : string.Join(" ", err);
        }

        var batch = await db.Set<MffImportBatch>().FirstOrDefaultAsync(x => x.BatchId == batchId, ct);
        if (batch is not null)
        {
            var errors = rows.Count(x => x.ValidationStatus == "ERROR");
            batch.RowCount = rows.Count;
            batch.ValidCount = rows.Count(x => x.ValidationStatus == "VALID");
            batch.ErrorCount = errors;
            batch.BatchStatus = errors == 0 ? "VALIDATED" : "VALIDATED_WITH_ERRORS";
        }

        await db.SaveChangesAsync(ct);
    }

    // NEW (not in Oracle): MFF_IMPORT_PKG.process_batch is called by page 13 but the package in the FINOVA database does
    // not have it. Designed in C# from MFF_IMPORT_STAGINGS and the application tables.

    /// <summary>
    /// NEW process_batch: validates the batch, then creates an application (IMPORTED) for each valid row not created yet:
    /// the applicant (and spouse) found by national ID or created, the parties, the declared income, the unit and its
    /// selection. Lookup codes (marital status, governorate, city, project) are matched by their Arabic names.
    /// The batch's valid count becomes the applications created. Returns the applications created and the errors.
    /// </summary>
    public async Task<(int Ok, int Errors)> ProcessBatchAsync(long batchId, CancellationToken ct)
    {
        var batch = await db.Set<MffImportBatch>().FirstOrDefaultAsync(x => x.BatchId == batchId, ct)
            ?? throw new ApexProcessException("ORA-20064: Import batch not found.");
        if (batch.ProgramVersionId is null || batch.FinancierId is null || batch.FundOrgId is null)
            throw new ApexProcessException("ORA-20065: The batch needs a program version, a financier and a fund.");
        await ValidateBatchAsync(batchId, ct);

        static string Key(string? s) => string.Concat((s ?? string.Empty).Where(c => !char.IsWhiteSpace(c)));
        var marital = (await db.Set<MffLkpMaritalStatus>().AsNoTracking().ToListAsync(ct)).ToDictionary(x => Key(x.NameAr), x => x.Code);
        var govs = (await db.Set<MffLkpGovernorate>().AsNoTracking().ToListAsync(ct)).ToDictionary(x => Key(x.NameAr), x => x.Code);
        var cities = await db.Set<MffLkpCity>().AsNoTracking().ToListAsync(ct);
        var projects = (await db.Set<MffHousingProject>().AsNoTracking().ToListAsync(ct)).GroupBy(x => Key(x.ProjectNameAr)).ToDictionary(g => g.Key, g => g.First().ProjectId);
        var today = DateOnly.FromDateTime(clock.UtcNow);
        var me = user.UserName ?? string.Empty;

        var rows = await db.Set<MffImportStaging>().Where(x => x.BatchId == batchId && x.ValidationStatus == "VALID" && x.ApplicationId == null)
            .OrderBy(x => x.SourceRowNo).ToListAsync(ct);
        foreach (var r in rows)
        {
            var nid = r.ApplicantNationalId!.Trim();
            if (await db.Set<MffApplication>().AnyAsync(a => a.ApplicationNo == "FF-" + batchId + "-" + r.SourceRowNo, ct))
            {
                r.ValidationStatus = "ERROR";
                r.ErrorMessage = "رقم الطلب مستخدم من قبل.";
                continue;
            }

            var person = await db.Set<MffPerson>().FirstOrDefaultAsync(x => x.NationalId == nid, ct);
            if (person is null)
            {
                person = new MffPerson { NationalId = nid };
                db.Add(person);
            }

            person.FullNameAr = r.ApplicantNameAr!.Trim();
            person.MobileNo = r.MobileNo ?? person.MobileNo;
            person.MaritalStatusCode = marital.GetValueOrDefault(Key(r.MaritalStatusRaw)) ?? person.MaritalStatusCode;
            await db.SaveChangesAsync(ct);

            var gov = govs.GetValueOrDefault(Key(r.GovernorateRaw));
            var city = cities.FirstOrDefault(c => Key(c.NameAr) == Key(r.CityRaw) && (gov is null || c.GovernorateCode == gov))?.Code;
            var app = new MffApplication
            {
                ApplicationNo = "FF-" + batchId + "-" + r.SourceRowNo,
                ImportStagingId = r.StagingId,
                ProgramVersionId = batch.ProgramVersionId.Value,
                FinancierId = batch.FinancierId.Value,
                FundOrgId = batch.FundOrgId.Value,
                SourceChannelCode = batch.SourceChannelCode,
                PrimaryPersonId = person.PersonId,
                StatusCode = "IMPORTED",
                ApplicationDate = today,
                RequestedFinanceAmount = r.FinanceAmount,
                TotalSubsidyAmount = r.TotalSubsidy,
                PropertyPrice = r.UnitPrice,
                DownpaymentAmount = r.CustomerDownpayment,
                MonthlyIncomeTotal = r.MonthlyIncome,
            };
            var property = new MffProperty
            {
                ProjectId = projects.TryGetValue(Key(r.ProjectRaw), out var pid) ? pid : null,
                GovernorateCode = gov,
                CityCode = city,
                DistrictAr = r.DistrictRaw,
                NeighborhoodAr = r.NeighborhoodRaw,
                BuildingNo = r.BuildingNo,
                FloorNo = r.FloorNo,
                UnitNo = r.UnitNo,
                AreaSqm = r.AreaSqm,
                AddressAr = r.SiteRaw,
                SalePrice = r.UnitPrice,
                MaintenanceDeposit = r.MaintenanceDeposit,
            };
            db.Add(app);
            db.Add(property);
            await db.SaveChangesAsync(ct);

            var party = new MffApplicationParty { ApplicationId = app.ApplicationId, PersonId = person.PersonId, PartyRoleCode = "APPLICANT", IsIncomeIncluded = true };
            db.Add(party);
            if (!string.IsNullOrWhiteSpace(r.SpouseNameAr) || !string.IsNullOrWhiteSpace(r.SpouseNationalId))
            {
                var snid = r.SpouseNationalId?.Trim();
                var spouse = string.IsNullOrEmpty(snid) ? null : await db.Set<MffPerson>().FirstOrDefaultAsync(x => x.NationalId == snid, ct);
                if (spouse is null)
                {
                    spouse = new MffPerson { NationalId = string.IsNullOrEmpty(snid) ? null : snid, FullNameAr = r.SpouseNameAr?.Trim() ?? snid! };
                    db.Add(spouse);
                    await db.SaveChangesAsync(ct);
                }

                db.Add(new MffApplicationParty { ApplicationId = app.ApplicationId, PersonId = spouse.PersonId, PartyRoleCode = "SPOUSE", IsIncomeIncluded = false });
            }

            await db.SaveChangesAsync(ct);
            db.Add(new MffIncomeSource
            {
                ApplicationPartyId = party.ApplicationPartyId,
                IncomeTypeCode = "OTHER",
                MonthlyNetIncome = r.MonthlyIncome ?? 0,
                AnnualNetIncome = (r.MonthlyIncome ?? 0) * 12,
                IsVerified = false,
                SourceReference = "Fund file batch " + batchId + " row " + r.SourceRowNo,
            });
            db.Add(new MffApplicationProperty
            {
                ApplicationId = app.ApplicationId,
                PropertyId = property.PropertyId,
                RequestedPrice = r.UnitPrice,
                IsSelected = true,
                SelectionDate = today,
            });
            db.Add(new MffApplicationStatusHist
            {
                ApplicationId = app.ApplicationId,
                OldStatusCode = null,
                NewStatusCode = "IMPORTED",
                ChangedAt = clock.UtcNow,
                ChangedBy = me,
                RoleCode = "INTAKE",
                RemarksAr = "إنشاء الطلب من ملف الصندوق",
            });
            r.ApplicationId = app.ApplicationId;
            await db.SaveChangesAsync(ct);
        }

        var all = await db.Set<MffImportStaging>().Where(x => x.BatchId == batchId).ToListAsync(ct);
        var ok = rows.Count(x => x.ApplicationId != null);
        var errors = all.Count(x => x.ValidationStatus == "ERROR");
        batch.RowCount = all.Count;
        batch.ValidCount = all.Count(x => x.ApplicationId != null);
        batch.ErrorCount = errors;
        batch.BatchStatus = errors == 0 ? "PROCESSED" : "PROCESSED_WITH_ERRORS";
        await db.SaveChangesAsync(ct);
        return (ok, errors);
    }

    [GeneratedRegex("^[0-9]{14}$")]
    private static partial Regex NationalId();
}
