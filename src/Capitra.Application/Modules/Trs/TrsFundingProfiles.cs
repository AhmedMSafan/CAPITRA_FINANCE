using Capitra.Application.Abstractions;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Trs;

/// <summary>What a disbursement is, for the bank rules on a business line, a product or a client segment.</summary>
public sealed record TrsFundingProfile(string BusinessLine, string? ProductCode, string? Segment)
{
    /// <summary>This profile's member of a rule dimension (BUSINESS_LINE, PRODUCT or CLIENT_SEGMENT); null when not known.</summary>
    public string? Member(string dimension) => dimension switch
    {
        "BUSINESS_LINE" => BusinessLine,
        "PRODUCT" => ProductCode,
        "CLIENT_SEGMENT" => Segment,
        _ => null,
    };
}

/// <summary>
/// The bank rule dimensions agreed with Ahmed (6 Nov 2026), from the APEX funding policy: a bank may exclude, or cap the share of, a
/// business line, a product or a client segment. Activity (industry) and asset wait for their lookups.
/// </summary>
public static class TrsFundingProfiles
{
    public static readonly string[] Dimensions = ["BUSINESS_LINE", "PRODUCT", "CLIENT_SEGMENT"];

    public static bool IsDimension(string? subjectTypeCode) => subjectTypeCode is not null && Dimensions.Contains(subjectTypeCode);

    /// <summary>The business line of a factoring agreement type: reverse factoring when it says so, otherwise factoring.</summary>
    public static string BusinessLineOf(string? factoringType) =>
        factoringType?.Contains("REVERSE", StringComparison.OrdinalIgnoreCase) == true ? "REVERSE_FACTORING" : "FACTORING";

    public static async Task<Dictionary<long, TrsFundingProfile>> LoadAsync(ICapitraDbContext db, IEnumerable<long> disbursementIds,
        CancellationToken cancellationToken)
    {
        var ids = disbursementIds.Distinct().ToList();
        if (ids.Count == 0) return [];
        var rows = await (from d in db.Set<FctDisbursement>().AsNoTracking()
                          where ids.Contains(d.DisbursementId)
                          join a in db.Set<FctAgreement>() on d.AgreementId equals a.AgreementId into aj
                          from a in aj.DefaultIfEmpty()
                          join p in db.Set<AdmProduct>() on d.ProductId equals p.ProductId into pj
                          from p in pj.DefaultIfEmpty()
                          join c in db.Set<CrmClient>() on d.ClientId equals c.ClientId into cj
                          from c in cj.DefaultIfEmpty()
                          select new { d.DisbursementId, FactoringType = a == null ? null : a.FactoringType, ProductCode = p == null ? null : p.ProductCode,
                              Segment = c == null ? null : c.ClientSegment })
            .ToListAsync(cancellationToken);
        return rows.ToDictionary(x => x.DisbursementId,
            x => new TrsFundingProfile(BusinessLineOf(x.FactoringType), x.ProductCode, string.IsNullOrWhiteSpace(x.Segment) ? null : x.Segment.Trim()));
    }

    /// <summary>"business line Leasing", "product Invoice Discounting", "client segment SME".</summary>
    public static string Describe(string dimension, string? member, IReadOnlyDictionary<string, string> productNames) => dimension switch
    {
        "BUSINESS_LINE" => $"business line {TrsLimitsService.BusinessLines.FirstOrDefault(x => x.Value == member)?.Label ?? member}",
        "PRODUCT" => $"product {(member is not null && productNames.TryGetValue(member, out var n) ? n : member)}",
        _ => $"client segment {Segment(member)}",
    };

    public static string Segment(string? code) => code switch
    {
        "SME" => "SME",
        "CORPORATE" => "Corporate",
        "LARGE_CORPORATE" => "Large corporate",
        null => "not set",
        _ => code,
    };
}
