using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mff.Packages;
using Capitra.Application.Security;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 121, After Submit "Load Loan" (LOAD_LOAN) and "Post Payment" (POST_PAYMENT): MFF_SERVICING_PKG.post_payment (new in C#).</summary>
public sealed class P0121Payments(ICapitraDbContext db, IApexSqlReader reader, MffServicingPackage servicing) : IApexProcess
{
    public int App => 110;

    public int Page => 121;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var req = request.Item("REQUEST");
        if (req is not ("LOAD_LOAN" or "POST_PAYMENT"))
            return Task.FromResult<object>(new { ok = true });
        return MffForm.RunAsync(db, async () =>
        {
            if (req == "LOAD_LOAN")
                return (string.Empty, Long(request, "P121_LOAN_ID") is null ? null : await LoadAsync(reader, 121, request, ct));

            var loanId = Long(request, "P121_LOAN_ID");
            var date = Date(request, "P121_PAYMENT_DATE");
            var amount = Number(request, "P121_AMOUNT") ?? 0;
            if (loanId is null || date is null || amount <= 0)
                throw new ApexProcessException("ORA-20071: Loan, payment date and a positive amount are required.");
            var id = await servicing.PostPaymentAsync(loanId.Value, date.Value, amount, Text(request, "P121_CHANNEL"), Text(request, "P121_REFERENCE_NO"), ct);
            var items = await LoadAsync(reader, 121, request, ct) ?? [];
            items["P121_PAYMENT_ID"] = Id(id);
            return ("Payment posted successfully. Payment ID: " + Id(id), items);
        }, ct);
    }
}
