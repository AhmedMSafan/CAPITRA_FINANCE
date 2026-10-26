using System.Globalization;
using Capitra.Application.Abstractions;

namespace Capitra.Application.Modules.Gl;

/// <summary>FINOVA ran in Cairo time: trunc(sysdate) and to_char(timestamp) are read in Africa/Cairo.</summary>
public static class GlTime
{
    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    /// <summary>trunc(sysdate).</summary>
    public static DateOnly Today(IClock clock) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(clock.UtcNow, Cairo));

    /// <summary>to_char(timestamp, format) in Cairo time, e.g. "dd-MMM-yyyy HH:mm" for 'DD-Mon-YYYY HH24:MI'.</summary>
    public static string? Stamp(DateTime? utc, string format) =>
        utc is null ? null : TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc.Value, DateTimeKind.Utc), Cairo)
            .ToString(format, CultureInfo.InvariantCulture);
}
