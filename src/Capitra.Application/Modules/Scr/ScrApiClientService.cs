using System.Security.Cryptography;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Scr;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Scr;

public sealed record ScrApiClientDto(
    long ApiClientId, long MemberCompanyId, string ClientId, string ClientName, string SecretHint, DateTime SecretCreatedAt, string StatusCode,
    DateTime? RevokedAt, string? RevokedBy, DateTime? LastTokenAt, string? LastIpAddress, int InquiryCount, string? Notes, string CreatedBy, DateTime CreatedAt);

/// <summary>A client with its secret, shown this once (on creation or a new secret).</summary>
public sealed record ScrApiClientSecretDto(ScrApiClientDto Client, string ClientSecret);

public sealed record ScrApiClientRequest(string? ClientName, string? Notes);

/// <summary>
/// The Screening API's clients: the Federation creates them for a member company, gives a new secret or revokes them. The secret is made
/// here, shown once and kept only as a PBKDF2 hash.
/// </summary>
public sealed class ScrApiClientService(ICapitraDbContext db, ICurrentUser currentUser, IClock clock)
{
    private const int Iterations = 100_000;

    public async Task<IReadOnlyList<ScrApiClientDto>> ListAsync(long companyId, CancellationToken ct)
    {
        var rows = await db.Set<ScrApiClient>().AsNoTracking().Where(c => c.MemberCompanyId == companyId).OrderBy(c => c.StatusCode).ThenBy(c => c.ApiClientId)
            .ToListAsync(ct);
        var ids = rows.Select(r => (long?)r.ApiClientId).ToList();
        var counts = await db.Set<ScrInquiry>().AsNoTracking().Where(i => ids.Contains(i.ApiClientId)).GroupBy(i => i.ApiClientId)
            .Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key!.Value, x => x.Count, ct);
        return [.. rows.Select(r => ToDto(r, counts.GetValueOrDefault(r.ApiClientId)))];
    }

    public async Task<ScrApiClientSecretDto> CreateAsync(long companyId, ScrApiClientRequest r, CancellationToken ct)
    {
        var company = await db.Set<ScrMemberCompany>().AsNoTracking().FirstOrDefaultAsync(c => c.MemberCompanyId == companyId, ct)
            ?? throw new NotFoundException("The member company was not found.");
        var name = ScrValidation.Clean(r.ClientName) ?? throw BusinessValidationException.For("clientName", "Enter what the client is for, e.g. Core banking onboarding.");
        if (name.Length > 150)
            throw BusinessValidationException.For("clientName", "Use at most 150 characters.");
        var secret = NewSecret();
        var client = new ScrApiClient
        {
            MemberCompanyId = companyId, ClientId = $"scr_{Slug(company.CompanyCode)}_{RandomText(8)}", ClientName = name, Notes = ScrValidation.Clean(r.Notes),
            SecretHash = Hash(secret), SecretHint = secret[^4..], SecretCreatedAt = clock.UtcNow,
        };
        db.Add(client);
        await db.SaveChangesAsync(ct);
        return new ScrApiClientSecretDto(ToDto(client, 0), secret);
    }

    /// <summary>A new secret: the old one stops working at once (tokens already given keep working until they expire, at most an hour).</summary>
    public async Task<ScrApiClientSecretDto> NewSecretAsync(long companyId, long apiClientId, CancellationToken ct)
    {
        var client = await ActiveAsync(companyId, apiClientId, ct);
        var secret = NewSecret();
        client.SecretHash = Hash(secret);
        client.SecretHint = secret[^4..];
        client.SecretCreatedAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);
        return new ScrApiClientSecretDto(ToDto(client, 0), secret);
    }

    /// <summary>Revokes the client: it gets no token, and the tokens it has stop working at once. Kept, with its inquiries.</summary>
    public async Task<ScrApiClientDto> RevokeAsync(long companyId, long apiClientId, CancellationToken ct)
    {
        var client = await ActiveAsync(companyId, apiClientId, ct);
        client.StatusCode = "REVOKED";
        client.RevokedAt = clock.UtcNow;
        client.RevokedBy = currentUser.UserName ?? "SYSTEM";
        await db.SaveChangesAsync(ct);
        return ToDto(client, 0);
    }

    /// <summary>The active client whose secret this is, or null (the same answer for an unknown client, a wrong secret or a revoked client).</summary>
    public async Task<ScrApiClient?> VerifyAsync(string? clientId, string? secret, string? ipAddress, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrEmpty(secret))
            return null;
        var client = await db.Set<ScrApiClient>().FirstOrDefaultAsync(c => c.ClientId == clientId.Trim(), ct);
        if (client is null || client.StatusCode != "ACTIVE" || !Matches(secret, client.SecretHash))
            return null;
        client.LastTokenAt = clock.UtcNow;
        client.LastIpAddress = ipAddress is { Length: > 64 } ? ipAddress[..64] : ipAddress;
        await db.SaveChangesAsync(ct);
        return client;
    }

    private async Task<ScrApiClient> ActiveAsync(long companyId, long apiClientId, CancellationToken ct)
    {
        var client = await db.Set<ScrApiClient>().FirstOrDefaultAsync(c => c.ApiClientId == apiClientId && c.MemberCompanyId == companyId, ct)
            ?? throw new NotFoundException("The API client was not found.");
        if (client.StatusCode != "ACTIVE")
            throw new BusinessRuleException("The API client is revoked. Create a new one.");
        return client;
    }

    // ---- secrets ------------------------------------------------------------------------------------------------------------------

    private static string NewSecret() => "scs_" + RandomText(40);

    private static string RandomText(int length)
    {
        const string chars = "abcdefghijkmnpqrstuvwxyz23456789";
        return string.Create(length, chars, (span, set) =>
        {
            for (var i = 0; i < span.Length; i++) span[i] = set[RandomNumberGenerator.GetInt32(set.Length)];
        });
    }

    private static string Slug(string code)
    {
        var s = new string([.. code.ToLowerInvariant().Where(char.IsAsciiLetterOrDigit)]);
        return s.Length == 0 ? "co" : s.Length > 12 ? s[..12] : s;
    }

    public static string Hash(string secret)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(secret, salt, Iterations, HashAlgorithmName.SHA256, 32);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Matches(string secret, string stored)
    {
        var parts = stored.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var n))
            return false;
        var expected = Convert.FromBase64String(parts[2]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(secret, Convert.FromBase64String(parts[1]), n, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static ScrApiClientDto ToDto(ScrApiClient c, int inquiries) => new(
        c.ApiClientId, c.MemberCompanyId, c.ClientId, c.ClientName, c.SecretHint, c.SecretCreatedAt, c.StatusCode, c.RevokedAt, c.RevokedBy, c.LastTokenAt,
        c.LastIpAddress, inquiries, c.Notes, c.CreatedBy, c.CreatedAt);
}
