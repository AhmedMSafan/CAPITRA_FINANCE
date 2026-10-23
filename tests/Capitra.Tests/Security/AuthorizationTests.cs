using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using Capitra.Api.Controllers.Sec;
using Capitra.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capitra.Tests.Security;

public sealed class AuthorizationTests : IClassFixture<CapitraApiFactory>
{
    private readonly CapitraApiFactory _factory;

    public AuthorizationTests(CapitraApiFactory factory) => _factory = factory;

    [Fact]
    public void Only_login_allows_anonymous_access()
    {
        var anonymous = typeof(Program).Assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => t.IsDefined(typeof(AllowAnonymousAttribute)) || m.IsDefined(typeof(AllowAnonymousAttribute)))
                .Select(m => $"{t.Name}.{m.Name}"))
            .ToList();

        Assert.Equal([$"{nameof(SecAuthController)}.{nameof(SecAuthController.Login)}"], anonymous);
    }

    [Fact]
    public async Task Requests_without_a_token_are_rejected()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync(new Uri("/api/adm/lookups", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Tampered_tokens_are_rejected()
    {
        using var client = _factory.CreateClient();
        var token = _factory.IssueToken(Permissions.All);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token[..^2] + "xx");
        var response = await client.GetAsync(new Uri("/api/adm/lookups", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Missing_permission_is_forbidden()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.IssueToken("MTG.SOMETHING.ELSE"));
        var response = await client.GetAsync(new Uri("/api/adm/lookups", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(Permissions.Adm.LookupView)]
    [InlineData(Permissions.All)]
    public async Task Granted_permission_is_allowed(string permission)
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.IssueToken(permission));
        var response = await client.GetAsync(new Uri("/api/adm/lookups", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("currency", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task View_permission_cannot_edit()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.IssueToken(Permissions.Adm.LookupView));
        using var body = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync(new Uri("/api/adm/lookups/currency", UriKind.Relative), body);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
