using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Capitra.Application.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Capitra.Api.Security;

public sealed record IssuedToken(string Token, DateTime ExpiresAt);

public sealed class JwtTokenService(IOptions<JwtOptions> options, IClock clock)
{
    private readonly JwtOptions _options = options.Value;

    public IssuedToken Issue(IEnumerable<Claim> claims) => Issue(claims, _options.Audience, _options.ExpiryMinutes);

    /// <summary>A token for another audience (the Screening API's), valid for the minutes given.</summary>
    public IssuedToken Issue(IEnumerable<Claim> claims, string audience, int minutes)
    {
        var expiresAt = clock.UtcNow.AddMinutes(minutes);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: audience,
            claims: claims,
            notBefore: clock.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return new IssuedToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
