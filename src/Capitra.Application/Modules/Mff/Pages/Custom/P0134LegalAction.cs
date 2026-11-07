using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 134, After Submit "Load Loan" (LOAD_LEGAL) and "Save Legal Action" (SAVE_LEGAL).</summary>
public sealed class P0134LegalAction(ICapitraDbContext db, IApexSqlReader reader) : IApexProcess
{
    public int App => 110;

    public int Page => 134;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var req = request.Item("REQUEST");
        if (req is not ("LOAD_LEGAL" or "SAVE_LEGAL"))
            return Task.FromResult<object>(new { ok = true });
        var loanId = Long(request, "P134_LOAN_ID");
        return MffForm.RunAsync(db, async () =>
        {
            if (req == "LOAD_LEGAL")
                return (string.Empty, loanId is null ? null : await LoadAsync(reader, 134, request, ct));
            var type = Text(request, "P134_ACTION_TYPE");
            var date = Date(request, "P134_ACTION_DATE");
            var status = Text(request, "P134_STATUS");
            if (loanId is null || type is null || date is null || status is null)
                throw new ApexProcessException("ORA-20077: Loan, action type, date and status are required.");
            db.Add(new MffLegalAction
            {
                LoanId = loanId,
                ApplicationId = Long(request, "P134_APPLICATION_ID"),
                ActionType = type,
                ActionDate = date.Value,
                CaseNo = Text(request, "P134_CASE_NO"),
                LegalActionStatus = status,
                OutcomeAr = Text(request, "P134_OUTCOME"),
                NextActionDate = Date(request, "P134_NEXT_ACTION_DATE"),
            });
            await db.SaveChangesAsync(ct);
            return ("Legal action saved.", null);
        }, ct);
    }
}
