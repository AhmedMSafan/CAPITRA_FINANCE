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
    public async Task Any_signed_in_user_can_read_lookup_definitions()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.IssueToken());
        var response = await client.GetAsync(new Uri("/api/adm/lookups", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("country", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("POST", "/api/adm/lookups/country", Permissions.Gl.SetupView)]
    [InlineData("GET", "/api/adm/products", "")]
    [InlineData("POST", "/api/adm/products", Permissions.Adm.ProductView)]
    [InlineData("POST", "/api/adm/charges", Permissions.Adm.ProductView)]
    [InlineData("PUT", "/api/adm/product-event-rules/1", Permissions.Adm.ProductView)]
    [InlineData("POST", "/api/adm/product-event-rules/1/charges", Permissions.Adm.LookupEdit)]
    [InlineData("POST", "/api/apex/120/600/IR_ACCOUNTING_EVENT_SOURCES_CATALOGUE", "GL.SOMETHING.ELSE")]
    [InlineData("POST", "/api/apex/120/601/FORM_DML_ACCOUNTING_EVENT_SOURCE_DETAILS", Permissions.Gl.SetupView)]
    public async Task Missing_permission_is_forbidden(string method, string url, string permission)
    {
        using var client = _factory.CreateClient();
        var token = permission.Length == 0 ? _factory.IssueToken() : _factory.IssueToken(permission);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var request = new HttpRequestMessage(new HttpMethod(method), new Uri(url, UriKind.Relative))
        {
            Content = method != "POST" ? null
                // Upload endpoints only match multipart requests.
                : url.Contains("excel-import", StringComparison.Ordinal) ? new MultipartFormDataContent()
                : new StringContent("{}", System.Text.Encoding.UTF8, "application/json"),
        };
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(Permissions.Adm.LookupEdit)]
    [InlineData(Permissions.All)]
    public async Task Granted_permission_reaches_the_service(string permission)
    {
        // The request passes authorization and is rejected by validation, before any database call.
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.IssueToken(permission));
        using var body = new StringContent("""{"code":"bad code","nameAr":"","nameEn":"x","displayOrder":0,"isActive":true,"extras":{}}""",
            System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync(new Uri("/api/adm/lookups/country", UriKind.Relative), body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.Contains("NameAr", text, StringComparison.Ordinal);
    }
}
