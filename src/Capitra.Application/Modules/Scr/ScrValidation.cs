using System.Text.RegularExpressions;

namespace Capitra.Application.Modules.Scr;

/// <summary>The checks shared by parties and inquiries: types, codes and identity number formats.</summary>
public static partial class ScrValidation
{
    public static readonly string[] PartyTypes = ["PERSON", "COMPANY"];
    public static readonly string[] IdentityTypes = ["NATIONAL_ID", "PASSPORT", "COMMERCIAL_REGISTER", "OTHER"];
    public static readonly string[] Channels = ["COURIER", "EMAIL", "HAND", "PORTAL"];
    public static readonly string[] Purposes = ["ONBOARDING", "TRANSACTION", "PERIODIC_REVIEW", "OTHER"];

    [GeneratedRegex("^[0-9]{14}$")]
    private static partial Regex NationalId();

    [GeneratedRegex("^[A-Z0-9]{5,20}$")]
    private static partial Regex Passport();

    [GeneratedRegex("^[0-9]{1,15}$")]
    private static partial Regex Register();

    [GeneratedRegex("^[A-Z]{2}$")]
    private static partial Regex Country();

    public static string Code(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();

    public static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>Adds the identity errors of a type and number: a 14-digit national ID, a passport, or a commercial register number.</summary>
    public static void Identity(Dictionary<string, string[]> e, string partyType, string identityType, string number, string field = "identityNo")
    {
        if (!IdentityTypes.Contains(identityType))
            e["identityTypeCode"] = ["Choose the identity type."];
        else if (partyType == "PERSON" && identityType == "COMMERCIAL_REGISTER")
            e["identityTypeCode"] = ["A person is identified by a national ID, a passport or another document."];
        else if (partyType == "COMPANY" && identityType is "NATIONAL_ID" or "PASSPORT")
            e["identityTypeCode"] = ["A company is identified by its commercial register number."];

        var n = number.Replace(" ", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        var ok = identityType switch
        {
            "NATIONAL_ID" => NationalId().IsMatch(n),
            "PASSPORT" => Passport().IsMatch(n),
            "COMMERCIAL_REGISTER" => Register().IsMatch(n),
            _ => n.Length is >= 3 and <= 30,
        };
        if (!ok)
        {
            e[field] = [identityType switch
            {
                "NATIONAL_ID" => "The national ID is 14 digits.",
                "PASSPORT" => "The passport number is 5 to 20 letters and digits.",
                "COMMERCIAL_REGISTER" => "The commercial register number is up to 15 digits.",
                _ => "Enter the document number.",
            }];
        }
    }

    public static void Nationality(Dictionary<string, string[]> e, string? code)
    {
        if (code is not null && !Country().IsMatch(code))
            e["nationalityCode"] = ["Use the two-letter country code (EG, SA, ...)."];
    }

    public static void Throw(Dictionary<string, string[]> e)
    {
        if (e.Count > 0)
            throw new Common.Exceptions.BusinessValidationException(e);
    }
}
