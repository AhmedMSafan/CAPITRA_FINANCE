using Microsoft.AspNetCore.Authorization;

namespace Capitra.Api.Security;

/// <summary>Requires a permission code such as ADM.LOOKUP.EDIT (granted to roles as role claims).</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute(string permission)
    : AuthorizeAttribute(PermissionPolicyProvider.PolicyPrefix + permission);
