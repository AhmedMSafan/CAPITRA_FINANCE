using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 22, After Submit "Save Applicant Details" (SAVE).</summary>
public sealed class P0022ApplicantDetails(ICapitraDbContext db, ICurrentUser user, IClock clock) : IApexProcess
{
    public int App => 110;

    public int Page => 22;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") != "SAVE")
            return Task.FromResult<object>(new { ok = true });
        return MffForm.RunAsync(db, async () =>
        {
            var id = Long(request, "P22_APPLICATION_ID");
            var a = await db.Set<MffApplication>().FirstOrDefaultAsync(x => x.ApplicationId == id, ct)
                ?? throw new ApexProcessException("ORA-01403: no data found");
            var old = a.StatusCode;
            var p = await db.Set<MffPerson>().FirstOrDefaultAsync(x => x.PersonId == a.PrimaryPersonId, ct);
            if (p is not null)
            {
                p.FullNameAr = Req(Text(request, "P22_FULL_NAME_AR"), "MFF_PERSONS", "FULL_NAME_AR");
                p.NationalId = Text(request, "P22_NATIONAL_ID");
                p.BirthDate = Date(request, "P22_BIRTH_DATE");
                p.GenderCode = Text(request, "P22_GENDER_CODE");
                p.MaritalStatusCode = Text(request, "P22_MARITAL_STATUS_CODE");
                p.MobileNo = Text(request, "P22_MOBILE_NO");
                p.Email = Text(request, "P22_EMAIL");
                p.AddressAr = Text(request, "P22_ADDRESS_AR");
                p.EmploymentTypeCode = Text(request, "P22_EMPLOYMENT_TYPE_CODE");
                p.EmployerName = Text(request, "P22_EMPLOYER_NAME");
                p.HireDate = Date(request, "P22_HIRE_DATE");
            }

            a.FundFileNo = Text(request, "P22_FUND_FILE_NO");
            a.RequestedFinanceAmount = Number(request, "P22_REQUESTED_FINANCE_AMOUNT");
            a.PropertyPrice = Number(request, "P22_PROPERTY_PRICE");
            a.DownpaymentAmount = Number(request, "P22_DOWNPAYMENT_AMOUNT");
            if (old == "IMPORTED")
            {
                a.StatusCode = "DOCUMENTS_PENDING";
                db.Add(new MffApplicationStatusHist
                {
                    ApplicationId = a.ApplicationId,
                    OldStatusCode = old,
                    NewStatusCode = "DOCUMENTS_PENDING",
                    ChangedAt = clock.UtcNow,
                    ChangedBy = user.UserName ?? string.Empty,
                    // the PL/SQL wrote 'ORIGINATION', which is not in MFF_LKP_USER_ROLES (the save failed on its foreign key): INTAKE
                    RoleCode = "INTAKE",
                    RemarksAr = "استكمال البيانات الأساسية للطلب",
                });
            }

            await db.SaveChangesAsync(ct);
            return ("Applicant and application details saved.", null);
        }, ct);
    }
}
