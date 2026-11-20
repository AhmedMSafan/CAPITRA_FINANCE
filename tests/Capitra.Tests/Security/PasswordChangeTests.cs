using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Capitra.Api.Security;
using Capitra.Application.Security;
using Capitra.Infrastructure.Identity;

namespace Capitra.Tests.Security;

/// <summary>Step 1 of company logins: temporary passwords made by the system, and a forced change at first sign-in.</summary>
public sealed class PasswordChangeTests : IClassFixture<CapitraApiFactory>
{
    private readonly CapitraApiFactory _factory;

    public PasswordChangeTests(CapitraApiFactory factory) => _factory = factory;

    [Fact]
    public void Temporary_passwords_meet_the_password_policy_and_are_readable()
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < 2000; i++)
        {
            var p = TemporaryPassword.Generate();
            Assert.Equal(TemporaryPassword.Length, p.Length);
            Assert.Contains(p, char.IsUpper);
            Assert.Contains(p, char.IsLower);
            Assert.Contains(p, char.IsDigit);
            Assert.Contains(p, c => !char.IsLetterOrDigit(c));
            Assert.DoesNotContain(p, c => "0O1lI".Contains(c, StringComparison.Ordinal));
            Assert.True(seen.Add(p), "temporary passwords must not repeat");
        }
    }

    [Theory]
    [InlineData("/api/scr/registry")]
    [InlineData("/api/scr/inquiries/mine")]
    [InlineData("/api/adm/lookups")]
    public async Task A_temporary_password_token_is_refused_everywhere_else(string path)
    {
        using var client = Client(_factory.IssueTemporaryPasswordToken(Permissions.All));
        var response = await client.GetAsync(new Uri(path, UriKind.Relative));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(PasswordChangeGate.Code, body.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task A_temporary_password_token_can_reach_change_password()
    {
        using var client = Client(_factory.IssueTemporaryPasswordToken());
        var response = await client.PostAsJsonAsync(new Uri("/api/sec/auth/change-password", UriKind.Relative),
            new { currentPassword = "Temp#Pass2345", newPassword = "Mine#Pass6789" });

        // the gate lets it through (no database here, so the call itself does not succeed)
        Assert.NotEqual(PasswordChangeGate.Code, await CodeOf(response));
    }

    [Fact]
    public async Task An_ordinary_token_is_not_held_by_the_gate()
    {
        using var client = Client(_factory.IssueToken(Permissions.All));
        var response = await client.GetAsync(new Uri("/api/scr/registry", UriKind.Relative));
        Assert.NotEqual(PasswordChangeGate.Code, await CodeOf(response));
    }

    private HttpClient Client(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<string?> CodeOf(HttpResponseMessage response)
    {
        try
        {
            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return body.RootElement.TryGetProperty("code", out var c) ? c.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
