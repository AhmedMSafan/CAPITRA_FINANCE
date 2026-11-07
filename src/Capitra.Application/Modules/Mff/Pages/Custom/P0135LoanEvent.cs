using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 135, After Submit "Load Loan" (LOAD_EVENT_LOAN) and "Save Loan Event" (SAVE_EVENT).</summary>
public sealed class P0135LoanEvent(ICapitraDbContext db, IApexSqlReader reader) : IApexProcess
{
    public int App => 110;

    public int Page => 135;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var req = request.Item("REQUEST");
        if (req is not ("LOAD_EVENT_LOAN" or "SAVE_EVENT"))
            return Task.FromResult<object>(new { ok = true });
        var loanId = Long(request, "P135_LOAN_ID");
        return MffForm.RunAsync(db, async () =>
        {
            if (req == "LOAD_EVENT_LOAN")
                return (string.Empty, loanId is null ? null : await LoadAsync(reader, 135, request, ct));
            var type = Text(request, "P135_EVENT_TYPE_CODE");
            var date = Date(request, "P135_EVENT_DATE");
            var reported = Date(request, "P135_REPORTED_DATE");
            var status = Text(request, "P135_STATUS");
            if (loanId is null || type is null || date is null || reported is null || status is null)
                throw new ApexProcessException("ORA-20078: Loan, event type, event date, reported date and status are required.");
            db.Add(new MffLoanEvent
            {
                LoanId = loanId.Value,
                EventTypeCode = type,
                EventDate = date.Value,
                ReportedDate = reported.Value,
                LoanEventStatus = status,
                DetailsAr = Text(request, "P135_DETAILS"),
                ResolutionAr = Text(request, "P135_RESOLUTION"),
            });
            await db.SaveChangesAsync(ct);
            return ("Loan event saved.", null);
        }, ct);
    }
}
