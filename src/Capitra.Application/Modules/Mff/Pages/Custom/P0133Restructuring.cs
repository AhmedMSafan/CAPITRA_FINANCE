using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 133, After Submit "Load Loan" (LOAD_RESTRUCT) and "Save Request" (SAVE_RESTRUCT).</summary>
public sealed class P0133Restructuring(ICapitraDbContext db, IApexSqlReader reader) : IApexProcess
{
    public int App => 110;

    public int Page => 133;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var req = request.Item("REQUEST");
        if (req is not ("LOAD_RESTRUCT" or "SAVE_RESTRUCT"))
            return Task.FromResult<object>(new { ok = true });
        var loanId = Long(request, "P133_LOAN_ID");
        return MffForm.RunAsync(db, async () =>
        {
            if (req == "LOAD_RESTRUCT")
                return (string.Empty, loanId is null ? null : await LoadAsync(reader, 133, request, ct));
            var date = Date(request, "P133_REQUEST_DATE");
            var reason = Text(request, "P133_REASON");
            if (loanId is null || date is null || reason is null)
                throw new ApexProcessException("ORA-20076: Loan, request date and reason are required.");
            db.Add(new MffLoanRestructuring
            {
                LoanId = loanId.Value,
                RequestDate = date.Value,
                EffectiveDate = Date(request, "P133_EFFECTIVE_DATE"),
                ReasonAr = reason,
                OldTermMonths = Int(request, "P133_OLD_TERM_MONTHS"),
                NewTermMonths = Int(request, "P133_NEW_TERM_MONTHS"),
                OldRatePct = Number(request, "P133_OLD_RATE_PCT"),
                NewRatePct = Number(request, "P133_NEW_RATE_PCT"),
                OldInstallment = Number(request, "P133_OLD_INSTALLMENT"),
                NewInstallment = Number(request, "P133_NEW_INSTALLMENT"),
                CapitalizedArrears = Number(request, "P133_CAPITALIZED_ARREARS") ?? 0,
                LoanRestructuringStatus = "REQUESTED",
                NotesAr = Text(request, "P133_NOTES"),
            });
            await db.SaveChangesAsync(ct);
            return ("Restructuring request saved for approval.", null);
        }, ct);
    }
}
