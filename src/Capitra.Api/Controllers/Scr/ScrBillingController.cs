using Capitra.Api.Security;
using Capitra.Application.Modules.Scr;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Scr;

/// <summary>
/// Screening service billing: price plans (EGP fee per user per month, fee per inquiry), each company's billing at a glance, and
/// invoices per calendar period (preview, issue, cancel; never deleted).
/// </summary>
[ApiController]
[Route("api/scr/billing")]
[HasPermission(Permissions.Scr.BillingView)]
public sealed class ScrBillingController(ScrBillingService billing) : ControllerBase
{
    [HttpGet("plans")]
    public Task<IReadOnlyList<ScrPricePlanDto>> Plans(CancellationToken ct) => billing.PlansAsync(ct);

    [HttpPost("plans")]
    [HasPermission(Permissions.Scr.BillingManage)]
    public Task<ScrPricePlanDto> CreatePlan(ScrPricePlanRequest request, CancellationToken ct) => billing.SavePlanAsync(null, request, ct);

    [HttpPut("plans/{pricePlanId:long}")]
    [HasPermission(Permissions.Scr.BillingManage)]
    public Task<ScrPricePlanDto> UpdatePlan(long pricePlanId, ScrPricePlanRequest request, CancellationToken ct) => billing.SavePlanAsync(pricePlanId, request, ct);

    /// <summary>Every company: its plan, its current period so far and what it has been invoiced.</summary>
    [HttpGet("overview")]
    public Task<IReadOnlyList<ScrBillingCompanyDto>> Overview(CancellationToken ct) => billing.OverviewAsync(ct);

    [HttpGet("invoices")]
    public Task<IReadOnlyList<ScrInvoiceDto>> Invoices([FromQuery] ScrInvoiceQuery query, CancellationToken ct) => billing.InvoicesAsync(query, ct);

    [HttpGet("invoices/{invoiceId:long}")]
    public Task<ScrInvoiceDto> Invoice(long invoiceId, CancellationToken ct) => billing.InvoiceAsync(invoiceId, ct);

    /// <summary>What the company would be invoiced for the period (nothing saved). periodFrom empty = the current period.</summary>
    [HttpGet("companies/{memberCompanyId:long}/preview")]
    public Task<ScrInvoiceDto> Preview(long memberCompanyId, [FromQuery] DateOnly? periodFrom, CancellationToken ct) =>
        billing.PreviewAsync(memberCompanyId, periodFrom, ct);

    [HttpPost("companies/{memberCompanyId:long}/invoices")]
    [HasPermission(Permissions.Scr.BillingManage)]
    public Task<ScrInvoiceDto> Issue(long memberCompanyId, ScrInvoiceRequest request, CancellationToken ct) => billing.IssueAsync(memberCompanyId, request, ct);

    [HttpPost("invoices/{invoiceId:long}/cancel")]
    [HasPermission(Permissions.Scr.BillingManage)]
    public Task<ScrInvoiceDto> Cancel(long invoiceId, ScrCancelInvoiceRequest request, CancellationToken ct) => billing.CancelAsync(invoiceId, request, ct);
}
