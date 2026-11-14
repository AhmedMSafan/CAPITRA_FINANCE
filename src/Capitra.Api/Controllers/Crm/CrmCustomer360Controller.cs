using Capitra.Api.Security;
using Capitra.Application.Modules.Crm;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Api.Controllers.Crm;

/// <summary>Customer 360: compliance, roles and bank accounts of one customer. Verifying needs CRM.VERIFY (four eyes).</summary>
[ApiController]
[Route("api/crm/clients/{clientId:long}")]
public sealed class CrmCustomer360Controller(CrmCustomer360Service service) : ControllerBase
{
    [HttpGet("360")]
    [HasPermission(Permissions.Crm.View)]
    public Task<CrmCustomer360Dto> Get(long clientId, CancellationToken cancellationToken) => service.GetAsync(clientId, cancellationToken);

    [HttpPut("compliance")]
    [HasPermission(Permissions.Crm.Edit)]
    public Task SaveCompliance(long clientId, CrmComplianceDto dto, CancellationToken cancellationToken) =>
        service.SaveComplianceAsync(clientId, dto, cancellationToken);

    [HttpPost("compliance/verify")]
    [HasPermission(Permissions.Crm.Verify)]
    public Task VerifyCompliance(long clientId, CrmVerifyDto dto, CancellationToken cancellationToken) =>
        service.VerifyComplianceAsync(clientId, dto, cancellationToken);

    [HttpPost("roles")]
    [HasPermission(Permissions.Crm.Edit)]
    public Task AddRole(long clientId, CrmAddRoleDto dto, CancellationToken cancellationToken) => service.AddRoleAsync(clientId, dto, cancellationToken);

    [HttpPost("roles/{roleId:long}/activate")]
    [HasPermission(Permissions.Crm.Edit)]
    public Task ActivateRole(long clientId, long roleId, CancellationToken cancellationToken) =>
        service.ActivateRoleAsync(clientId, roleId, cancellationToken);

    [HttpPost("roles/{roleId:long}/suspend")]
    [HasPermission(Permissions.Crm.Edit)]
    public Task SuspendRole(long clientId, long roleId, CrmVerifyDto dto, CancellationToken cancellationToken) =>
        service.SuspendRoleAsync(clientId, roleId, dto, cancellationToken);

    [HttpPost("bank-accounts")]
    [HasPermission(Permissions.Crm.Edit)]
    public Task<long> AddBankAccount(long clientId, CrmBankAccountFormDto dto, CancellationToken cancellationToken) =>
        service.SaveBankAccountAsync(clientId, null, dto, cancellationToken);

    [HttpPut("bank-accounts/{bankAccountId:long}")]
    [HasPermission(Permissions.Crm.Edit)]
    public Task<long> UpdateBankAccount(long clientId, long bankAccountId, CrmBankAccountFormDto dto, CancellationToken cancellationToken) =>
        service.SaveBankAccountAsync(clientId, bankAccountId, dto, cancellationToken);

    [HttpPost("bank-accounts/{bankAccountId:long}/verify")]
    [HasPermission(Permissions.Crm.Verify)]
    public Task VerifyBankAccount(long clientId, long bankAccountId, CrmVerifyDto dto, CancellationToken cancellationToken) =>
        service.VerifyBankAccountAsync(clientId, bankAccountId, dto, cancellationToken);

    [HttpPost("bank-accounts/{bankAccountId:long}/suspend")]
    [HasPermission(Permissions.Crm.Edit)]
    public Task SuspendBankAccount(long clientId, long bankAccountId, CrmVerifyDto dto, CancellationToken cancellationToken) =>
        service.SuspendBankAccountAsync(clientId, bankAccountId, dto, cancellationToken);

    [HttpPost("bank-accounts/{bankAccountId:long}/reactivate")]
    [HasPermission(Permissions.Crm.Edit)]
    public Task ReactivateBankAccount(long clientId, long bankAccountId, CrmVerifyDto dto, CancellationToken cancellationToken) =>
        service.ReactivateBankAccountAsync(clientId, bankAccountId, dto, cancellationToken);
}
