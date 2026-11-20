using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mff.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 84, After Submit "Disburse Loan" (DISBURSE): MFF_SERVICING_PKG.generate_installments (new in C#).</summary>
public sealed class P0084Disbursement(ICapitraDbContext db, MffServicingPackage servicing, MffControlPackage control, ICurrentUser user) : IApexProcess
{
    public int App => 110;

    public int Page => 84;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") != "DISBURSE")
            return Task.FromResult<object>(new { ok = true });
        return MffForm.RunAsync(db, async () =>
        {
            if (Long(request, "P84_LOAN_ID") is not null)
                throw new ApexProcessException("ORA-20063: A loan account already exists for this application.");
            const string t = "MFF_LOANS";
            var appId = Req(Long(request, "P84_APPLICATION_ID"), t, "APPLICATION_ID");
            var principal = Req(Number(request, "P84_PRINCIPAL_AMOUNT"), t, "PRINCIPAL_AMOUNT");
            var firstDue = Date(request, "P84_FIRST_DUE_DATE");
            var l = new MffLoan
            {
                ApplicationId = appId,
                ContractId = Req(Long(request, "P84_CONTRACT_ID"), t, "CONTRACT_ID"),
                LoanAccountNo = Req(Text(request, "P84_LOAN_ACCOUNT_NO"), t, "LOAN_ACCOUNT_NO"),
                DisbursementDate = Req(Date(request, "P84_DISBURSEMENT_DATE"), t, "DISBURSEMENT_DATE"),
                PrincipalAmount = principal,
                AnnualRatePct = Number(request, "P84_ANNUAL_RATE_PCT"),
                TermMonths = Req(Int(request, "P84_TERM_MONTHS"), t, "TERM_MONTHS"),
                MaturityDate = Date(request, "P84_MATURITY_DATE"),
                InitialInstallment = Number(request, "P84_INITIAL_INSTALLMENT"),
                InstallmentFrequency = "MONTHLY",
                LoanStatus = "ACTIVE",
                OutstandingPrincipal = principal,
                NextDueDate = firstDue,
                DelinquentDays = 0,
                CoreSystemRef = Text(request, "P84_CORE_SYSTEM_REF"),
            };
            db.Add(l);
            await db.SaveChangesAsync(ct);
            await servicing.GenerateInstallmentsAsync(l.LoanId, firstDue, Number(request, "P84_ANNUAL_GROWTH_PCT") ?? 0, Number(request, "P84_GROWTH_YEARS") ?? 0,
                Number(request, "P84_MONTHLY_SUBSIDY") ?? 0, Int(request, "P84_SUBSIDY_MONTHS") ?? 0, ct);
            var a = await db.Set<MffApplication>().FirstOrDefaultAsync(x => x.ApplicationId == appId, ct);
            if (a is not null)
                a.ApprovedFinanceAmount = principal;
            await db.SaveChangesAsync(ct);
            await control.ChangeStatusCheckedAsync(appId, "DISBURSED", "OPERATIONS", user.UserName, "Loan disbursed and account opened", ct);
            return ("Loan disbursed and installment schedule generated. Loan ID: " + Id(l.LoanId), new Dictionary<string, string?> { ["P84_LOAN_ID"] = Id(l.LoanId) });
        }, ct);
    }
}
