using Capitra.Api.Security;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Scr;
using Capitra.Application.Security;
using Capitra.Infrastructure.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Scr;

/// <summary>
/// The Screening Portal's pages for a member company's administrator (SCR.COMPANY.USERS): the company's own invoices (with payments, no
/// GL references) and its users' sign-ins. Never another company's.
/// </summary>
[ApiController]
[Route("api/scr/portal")]
[HasPermission(Permissions.Scr.CompanyUsers)]
public sealed class ScrPortalController(ScrInquiryService inquiries, ScrBillingService billing, SignInLog log) : ControllerBase
{
    [HttpGet("invoices")]
    public async Task<IReadOnlyList<ScrInvoiceDto>> Invoices(int? year, CancellationToken ct)
    {
        var company = await inquiries.MyCompanyAsync(ct);
        return [.. (await billing.InvoicesAsync(new ScrInvoiceQuery(company.MemberCompanyId, null, year), ct)).Select(Strip)];
    }

    [HttpGet("invoices/{invoiceId:long}")]
    public async Task<ScrInvoiceDto> Invoice(long invoiceId, CancellationToken ct)
    {
        var company = await inquiries.MyCompanyAsync(ct);
        var invoice = await billing.InvoiceAsync(invoiceId, ct);
        return invoice.MemberCompanyId == company.MemberCompanyId ? Strip(invoice) : throw new NotFoundException("The invoice was not found.");
    }

    [HttpGet("sign-ins")]
    public async Task<SignInPage> SignIns([FromQuery] SignInQuery query, CancellationToken ct)
    {
        var company = await inquiries.MyCompanyAsync(ct);
        return await log.ListAsync(query with { MemberCompanyId = company.MemberCompanyId }, ct);
    }

    [HttpGet("sign-ins/{signInId:long}")]
    public async Task<SessionDto> Session(long signInId, CancellationToken ct)
    {
        var company = await inquiries.MyCompanyAsync(ct);
        var session = await log.SessionAsync(signInId, ct);
        return session.SignIn.MemberCompanyId == company.MemberCompanyId ? session : throw new NotFoundException("The sign-in was not found.");
    }

    /// <summary>A company sees its invoice and payments, not the Federation's GL references or bank accounts' GL codes.</summary>
    private static ScrInvoiceDto Strip(ScrInvoiceDto i) => i with
    {
        GlTranSetId = null, CancelGlTranSetId = null,
        Payments = i.Payments is null ? null : [.. i.Payments.Select(p => p with { GlTranSetId = null, CreatedBy = string.Empty })],
    };
}
