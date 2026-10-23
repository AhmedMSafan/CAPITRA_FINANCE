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
        Assert.Contains("portfolio-status", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("GET", "/api/mtg/developers", "MTG.SOMETHING.ELSE")]
    [InlineData("POST", "/api/mtg/developers", Permissions.Mtg.MasterDataView)]
    [InlineData("DELETE", "/api/mtg/investors/1?rowVersion=1", Permissions.Mtg.MasterDataView)]
    [InlineData("POST", "/api/adm/lookups/currency", Permissions.Mtg.MasterDataEdit)]
    [InlineData("DELETE", "/api/adm/lookups/currency/EGP?rowVersion=1", "")]
    [InlineData("GET", "/api/mtg/portfolios", Permissions.Mtg.MasterDataView)]
    [InlineData("GET", "/api/mtg/portfolios/1/workspace", "")]
    [InlineData("POST", "/api/mtg/portfolios", Permissions.Mtg.PortfolioView)]
    [InlineData("PUT", "/api/mtg/portfolios/1/financial-terms", Permissions.Mtg.PortfolioView)]
    [InlineData("DELETE", "/api/mtg/portfolios/1?rowVersion=1", Permissions.Mtg.PortfolioView)]
    [InlineData("GET", "/api/mtg/portfolio-accounts?portfolioId=1", Permissions.Mtg.PortfolioEdit)]
    [InlineData("POST", "/api/mtg/portfolio-accounts", Permissions.Mtg.PortfolioView)]
    [InlineData("PUT", "/api/mtg/account-payments/1", Permissions.Mtg.PortfolioView)]
    [InlineData("DELETE", "/api/mtg/account-payments/1?rowVersion=1", Permissions.Mtg.PortfolioView)]
    [InlineData("GET", "/api/mtg/portfolios/1/excel-import", Permissions.Mtg.StepExcelImport)]
    [InlineData("POST", "/api/mtg/portfolios/1/excel-import/validate", Permissions.Mtg.PortfolioView)]
    [InlineData("POST", "/api/mtg/portfolios/1/excel-import/import", Permissions.Mtg.PortfolioEdit)]
    [InlineData("GET", "/api/mtg/portfolios/1/aml-review", Permissions.Mtg.StepAmlReview)]
    [InlineData("POST", "/api/mtg/portfolios/1/aml-review/results", Permissions.Mtg.PortfolioView)]
    [InlineData("PUT", "/api/mtg/portfolios/1/aml-review/deal-decisions", Permissions.Mtg.StepExcelImport)]
    [InlineData("POST", "/api/mtg/portfolios/1/aml-review/decision", Permissions.Mtg.PortfolioEdit)]
    [InlineData("GET", "/api/mtg/portfolios/1/inquiries", Permissions.Mtg.StepInquiries)]
    [InlineData("POST", "/api/mtg/portfolios/1/inquiries/records", Permissions.Mtg.PortfolioView)]
    [InlineData("PUT", "/api/mtg/portfolios/1/inquiries/records/1", Permissions.Mtg.StepAmlReview)]
    [InlineData("DELETE", "/api/mtg/portfolios/1/inquiries/records/1?rowVersion=1", Permissions.Mtg.PortfolioView)]
    [InlineData("PUT", "/api/mtg/portfolios/1/inquiries/deal-decisions", Permissions.Mtg.PortfolioEdit)]
    [InlineData("POST", "/api/mtg/portfolios/1/inquiries/decision", Permissions.Mtg.StepAmlReview)]
    [InlineData("GET", "/api/mtg/portfolios/1/legal-review", Permissions.Mtg.StepLegalReview)]
    [InlineData("PUT", "/api/mtg/portfolios/1/legal-review", Permissions.Mtg.StepInquiries)]
    [InlineData("PUT", "/api/mtg/portfolios/1/legal-review/deal-decisions", Permissions.Mtg.PortfolioView)]
    [InlineData("POST", "/api/mtg/portfolios/1/legal-review/decision", Permissions.Mtg.PortfolioEdit)]
    [InlineData("GET", "/api/mtg/portfolios/1/valuation", Permissions.Mtg.StepValuation)]
    [InlineData("POST", "/api/mtg/portfolios/1/valuation/records", Permissions.Mtg.PortfolioView)]
    [InlineData("PUT", "/api/mtg/portfolios/1/valuation/records/1", Permissions.Mtg.StepLegalReview)]
    [InlineData("DELETE", "/api/mtg/portfolios/1/valuation/records/1?rowVersion=1", Permissions.Mtg.PortfolioEdit)]
    [InlineData("POST", "/api/mtg/portfolios/1/valuation/complete", Permissions.Mtg.PortfolioView)]
    [InlineData("GET", "/api/mtg/portfolios/1/coverage-review", Permissions.Mtg.StepCoverageReview)]
    [InlineData("POST", "/api/mtg/portfolios/1/coverage-review/apply-rule", Permissions.Mtg.StepValuation)]
    [InlineData("PUT", "/api/mtg/portfolios/1/coverage-review/investors/1/decision", Permissions.Mtg.PortfolioEdit)]
    [InlineData("POST", "/api/mtg/portfolios/1/coverage-review/decision", Permissions.Mtg.PortfolioView)]
    [InlineData("GET", "/api/mtg/portfolios/1/cheque-coverage", Permissions.Mtg.StepChequeCoverage)]
    [InlineData("POST", "/api/mtg/portfolios/1/cheque-coverage/cheques", Permissions.Mtg.StepCoverageReview)]
    [InlineData("PUT", "/api/mtg/portfolios/1/cheque-coverage/cheques/1", Permissions.Mtg.PortfolioView)]
    [InlineData("DELETE", "/api/mtg/portfolios/1/cheque-coverage/cheques/1?rowVersion=1", Permissions.Mtg.PortfolioEdit)]
    [InlineData("POST", "/api/mtg/portfolios/1/cheque-coverage/decision", Permissions.Mtg.PortfolioView)]
    [InlineData("GET", "/api/mtg/portfolios/1/financial-review", Permissions.Mtg.StepFinancialReview)]
    [InlineData("POST", "/api/mtg/portfolios/1/financial-review/calculate", Permissions.Mtg.StepChequeCoverage)]
    [InlineData("PUT", "/api/mtg/portfolios/1/financial-review", Permissions.Mtg.PortfolioView)]
    [InlineData("POST", "/api/mtg/portfolios/1/financial-review/decision", Permissions.Mtg.PortfolioView)]
    [InlineData("GET", "/api/mtg/portfolios/1/board-review", Permissions.Mtg.StepBoardReview)]
    [InlineData("POST", "/api/mtg/portfolios/1/board-review/decision", Permissions.Mtg.PortfolioView)]
    [InlineData("GET", "/api/mtg/portfolios/1/purchase-agreement", Permissions.Mtg.StepBoardReview)]
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
        var response = await client.PostAsync(new Uri("/api/adm/lookups/currency", UriKind.Relative), body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.Contains("NameAr", text, StringComparison.Ordinal);
        Assert.Contains("decimalPlaces", text, StringComparison.Ordinal);
    }
}
