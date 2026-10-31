using System.Globalization;

namespace Capitra.Application.Modules.Apex;

/// <summary>
/// How a copied app shows its stored times. App 122 (Cheques Wallet) ran on the UTC database clock, so its times
/// show as they are stored; the other apps show them in Cairo time.
/// </summary>
public static class ApexTime
{
    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    public static bool AsStored(int app) => app == 122;

    /// <summary>A stored time as the app shows it, in the format given.</summary>
    public static string Stamp(int app, DateTime t, string format) =>
        AsStored(app)
            ? DateTime.SpecifyKind(t, DateTimeKind.Utc).ToString(format, CultureInfo.InvariantCulture)
            : Gl.GlTime.Stamp(t, format)!;

    /// <summary>A time the app's user typed, as it is stored.</summary>
    public static DateTime ToStored(int app, DateTime typed) =>
        AsStored(app)
            ? DateTime.SpecifyKind(typed, DateTimeKind.Utc)
            : TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(typed, DateTimeKind.Unspecified), Cairo);
}
