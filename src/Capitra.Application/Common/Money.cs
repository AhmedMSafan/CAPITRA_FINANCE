namespace Capitra.Application.Common;

/// <summary>
/// The one rounding rule for money, rates and percentages: half away from zero (0.125 to 0.13), as Oracle's ROUND did in the
/// systems CAPITRA replaces. .NET's own <see cref="Math.Round(decimal, int)"/> rounds half to even (0.125 to 0.12), so the
/// decimal overloads of Math.Round without a <see cref="MidpointRounding"/> are banned in the source projects (BannedSymbols.txt).
/// </summary>
public static class Money
{
    /// <summary>The decimals of an amount when its currency's own (ADM currency DecimalPlaces) is not at hand.</summary>
    public const int DefaultDecimals = 2;

    /// <summary>The decimals of GL accounted amounts (entered amount times the conversion rate), as the GL engine keeps them.</summary>
    public const int AccountedDecimals = 5;

    public static decimal Round(decimal value, int decimals = DefaultDecimals) =>
        Math.Round(value, decimals, MidpointRounding.AwayFromZero);

    public static decimal? Round(decimal? value, int decimals = DefaultDecimals) =>
        value is { } v ? Round(v, decimals) : null;
}
