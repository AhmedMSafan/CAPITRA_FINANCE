using System.Text.RegularExpressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Domain.Modules.Gl;

namespace Capitra.Application.Modules.Gl;

/// <summary>Checks shared by the GL setup screens.</summary>
internal static partial class GlRules
{
    /// <summary>Checks a value code against its segment: length, format, upper case (APEX pages 2 and 40).</summary>
    public static void CheckValueCode(ValidationErrors errors, string field, string code, GlCoaSegment segment)
    {
        var range = segment.MinLength == segment.MaxLength ? $"{segment.MinLength}" : $"{segment.MinLength} to {segment.MaxLength}";

        if (code.Length < segment.MinLength || code.Length > segment.MaxLength)
            errors.Add(field, $"The code must be {range} characters long (segment {segment.SegmentCode}).");
        else if (segment.ValueFormat == GlCodes.FormatNumeric && !Digits().IsMatch(code))
            errors.Add(field, $"Segment {segment.SegmentCode} is numeric: use digits only.");
        else if (segment.ValueFormat == GlCodes.FormatAlphanumeric && !LettersAndDigits().IsMatch(code))
            errors.Add(field, $"Segment {segment.SegmentCode} accepts letters and digits only.");
    }

    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    /// <summary>Today in Cairo, the default effective date.</summary>
    public static DateOnly Today(IClock clock) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(clock.UtcNow, Cairo));

    public static string? Code(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    public static void CheckDates(ValidationErrors errors, DateOnly? start, DateOnly? end)
    {
        if (start is { } s && end is { } e && e < s)
            errors.Add("effectiveEndDate", "Effective to cannot be before effective from.");
    }

    [GeneratedRegex("^[0-9]+$")]
    private static partial Regex Digits();

    [GeneratedRegex("^[A-Z0-9]+$")]
    private static partial Regex LettersAndDigits();
}
