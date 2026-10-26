using System.Text.RegularExpressions;
using Capitra.Application.Abstractions;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mff.Packages;

/// <summary>MFF_IMPORT_PKG: the checks of an uploaded Fund file (MFF_IMPORT_STAGINGS) and the counts of its batch.</summary>
public sealed partial class MffImportPackage(ICapitraDbContext db)
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

    [GeneratedRegex("^[0-9]{14}$")]
    private static partial Regex NationalId();
}
