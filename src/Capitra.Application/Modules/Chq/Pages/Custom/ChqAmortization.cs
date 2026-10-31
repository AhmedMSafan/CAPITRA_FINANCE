using System.Globalization;
using System.Text;
using System.Text.Json;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;

namespace Capitra.Application.Modules.Chq.Pages.Custom;

/// <summary>App 122 page 3 "Universal Amortization": the CALCULATE_AMORT callback.</summary>
public sealed class P0003CalculateAmort(IClock clock) : IApexProcess
{
    public int App => ChqAjax.App;

    public int Page => 3;

    public string Name => "CALCULATE_AMORT";

    public string Permission => Permissions.Chq.View;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => Task.FromResult(ChqAmortization.Calculate(request, clock));
}

/// <summary>App 122 page 333 "Universal Calculator": the same CALCULATE_AMORT callback.</summary>
public sealed class P0333CalculateAmort(IClock clock) : IApexProcess
{
    public int App => ChqAjax.App;

    public int Page => 333;

    public string Name => "CALCULATE_AMORT";

    public string Permission => Permissions.Chq.View;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => Task.FromResult(ChqAmortization.Calculate(request, clock));
}

/// <summary>
/// The PL/SQL of CALCULATE_AMORT (pages 3 and 333): reads the inputs with apex_json, builds the schedule and its
/// summary, and prints the JSON text as it wrote it (numbers with the to_char masks it used).
/// </summary>
internal static class ChqAmortization
{
    public static object Calculate(ApexRequest request, IClock clock)
    {
        string text;
        try
        {
            text = Build(request.Json ?? [], clock);
        }
        catch (Exception e)
        {
            var message = e switch
            {
                DivideByZeroException => "ORA-01476: divisor is equal to zero",
                OverflowException => "ORA-01426: numeric overflow",
                _ => e.Message,
            };
            text = "{\"success\":false,\"error\":\"" + ChqAjax.Substr(message, 300)!.Replace("\"", "\\\"", StringComparison.Ordinal) + "\"}";
        }

        try
        {
            using var doc = JsonDocument.Parse(text);
            return doc.RootElement.Clone();
        }
        catch (JsonException)
        {
            using var fatal = JsonDocument.Parse("{\"success\":false,\"error\":\"Fatal\"}");
            return fatal.RootElement.Clone();
        }
    }

    private static string Build(Dictionary<string, JsonElement> json, IClock clock)
    {
        // nvl(apex_json.get_number(k), 0), 0 when it is not a number
        decimal Gn(string k)
        {
            if (!json.TryGetValue(k, out var v))
                return 0;
            if (v.ValueKind == JsonValueKind.Number)
                return v.GetDecimal();
            return v.ValueKind == JsonValueKind.String && v.GetString() is { Length: > 0 } s
                && decimal.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var n) ? n : 0;
        }

        // nvl(apex_json.get_varchar2(k), d): an empty text is null
        string? Gv(string k, string? d = null)
        {
            if (!json.TryGetValue(k, out var v))
                return d;
            var s = v.ValueKind switch
            {
                JsonValueKind.String => v.GetString(),
                JsonValueKind.Number => v.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => null,
            };
            return string.IsNullOrEmpty(s) ? d : s;
        }

        var asset = Gn("aAsset");
        var down = Gn("aDown");
        var residual = Gn("aResidual");
        var term = Gn("aTerm");
        var rateAnn = Gn("aRateAnn");
        var resetM = Gn("aResetM");
        if (resetM == 0)
            resetM = 12;
        var rateInc = Gn("aRateInc");
        var adminPct = Gn("aAdminPct");
        var adminFix = Gn("aAdminFix");
        var insPct = Gn("aInsPct");
        var vat = Gn("aVat");
        var graceM = Gn("aGraceM");
        var freq = Gv("aFreq", "Monthly");
        var timing = Gv("aTiming", "Arrear");
        var rateType = Gv("aRateTyp", "Fixed");
        var method = Gv("aMethod", "Equal Installment");
        var graceFlg = Gv("aGraceFlg", "No");
        var graceTrt = Gv("aGraceTr", "Capitalize");
        DateOnly? start, firstPmt;
        try
        {
            start = ToDate(Gv("aStart"));
            firstPmt = ToDate(Gv("aFirstPmt"));
        }
        catch (FormatException)
        {
            var today = DateOnly.FromDateTime(ChqAjax.Now(clock));
            start = today;
            firstPmt = today.AddDays(30);
        }

        var net = asset - down;
        if (net <= 0)
            return "{\"success\":false,\"error\":\"Net must be positive\"}";
        if (residual < 0 || residual >= net)
            return "{\"success\":false,\"error\":\"Residual out of range\"}";
        if (term <= 0)
            return "{\"success\":false,\"error\":\"Term must be > 0\"}";
        if (firstPmt < start)
            return "{\"success\":false,\"error\":\"First pmt must be >= start\"}";

        // Derive
        decimal ppy = freq switch { "Monthly" => 12, "Bi-Monthly" => 6, "Quarterly" => 4, "Semi-Annual" => 2, "Annual" => 1, _ => 12 };
        var mpp = 12 / ppy;
        var totPrd = Round(term / mpp, 0);
        var perRate = rateAnn / ppy;
        var grace = graceFlg == "Yes";
        var grPrd = grace ? Round(graceM / mpp, 0) : 0;
        var repPrd = grace && graceTrt == "Pay Interest Only" ? totPrd - grPrd : totPrd;
        var adjPrin = grace && graceTrt == "Capitalize" ? net * Power(1 + perRate, grPrd) : net;
        var advance = timing == "Advance" ? 1 : 0;
        var pmt = method is "Equal Installment" or "Balloon" ? FnPmt(perRate, repPrd, -adjPrin, residual, advance) : 0;
        var eqPrin = method == "Equal Principal" ? (adjPrin - residual) / repPrd : 0;
        var upFees = net * adminPct + adminFix;
        var insPrd = asset * insPct / ppy;

        // Build JSON
        var j = new StringBuilder("{");
        var sep = "";
        void Ja(string k, string? v) { j.Append(sep).Append('"').Append(k).Append("\":\"").Append(v).Append('"'); sep = ","; }
        void Jan(string k, decimal? v) { j.Append(sep).Append('"').Append(k).Append("\":").Append(v is null ? "null" : Fixed(v.Value, 6)); sep = ","; }

        Jan("success", 1);
        Jan("net_financing", net);
        Ja("start_date", Iso(start));
        j.Append(sep).Append("\"params\":{");
        sep = "";
        Jan("total_periods", totPrd);
        Jan("periods_per_year", ppy);
        Jan("periodic_rate", perRate);
        Jan("upfront_fees", upFees);
        j.Append('}');
        sep = ",";

        // Schedule
        j.Append(sep).Append("\"schedule\":[");
        decimal sumInst = 0, sumInt = 0, sumPrin = 0, sumFee = 0, sumTax = 0, sumAll = 0;
        var bal = net;
        var cf = new List<decimal> { -(net - upFees) };
        var dt = new List<DateOnly?> { start };
        for (var p = 1; p <= totPrd; p++)
        {
            var open = bal;
            var isGr = grace && p <= grPrd;
            var rate = rateType == "Fixed" ? perRate : (rateAnn + Math.Floor((p - 1) / (resetM / mpp)) * rateInc) / ppy;
            decimal inst;
            if (isGr)
                inst = graceTrt == "Pay Interest Only" ? open * rate : 0;
            else if (method == "Bullet")
                inst = p == totPrd ? open + open * rate : 0;
            else if (method == "Equal Principal")
                inst = advance == 1 ? eqPrin + (open - eqPrin) * rate : eqPrin + open * rate;
            else
                inst = FnPmt(rate, totPrd - p + 1, -open, residual, advance);
            var interest = isGr ? open * rate : advance == 1 ? Math.Max(0, open - inst) * rate : open * rate;
            var prin = isGr ? 0 : inst - interest;
            var fee = insPrd;
            var tax = interest * vat;
            var close = isGr ? (graceTrt is "Capitalize" or "Defer" ? open + interest : open) : open - prin;
            var total = inst + fee + tax;
            var date = AddMonths(firstPmt, (p - 1) * mpp);
            if (p > 1)
                j.Append(',');
            j.Append("{\"p\":").Append(p).Append(",\"dt\":\"").Append(Iso(date)).Append('"')
                .Append(",\"ty\":\"").Append(isGr ? "Grace" : "Regular").Append('"')
                .Append(",\"op\":").Append(Jn(open)).Append(",\"rt\":").Append(Jn6(rate)).Append(",\"inst\":").Append(Jn(inst))
                .Append(",\"int\":").Append(Jn(interest)).Append(",\"prin\":").Append(Jn(prin)).Append(",\"fee\":").Append(Jn(fee))
                .Append(",\"vat\":").Append(Jn(tax)).Append(",\"tot\":").Append(Jn(total)).Append(",\"cl\":").Append(Jn(close)).Append('}');
            sumInst += inst;
            sumInt += interest;
            sumPrin += prin;
            sumFee += fee;
            sumTax += tax;
            sumAll += total;
            cf.Add(total + (p == totPrd ? residual : 0));
            dt.Add(date);
            bal = close;
        }
        j.Append(']');
        sep = ",";

        // Summary
        var xirr = FnXirr(cf, dt);
        j.Append(sep).Append("\"summary\":{");
        sep = "";
        Ja("maturity_date", Iso(AddMonths(firstPmt, (totPrd - 1) * mpp)));
        Jan("total_installments", sumInst);
        Jan("total_interest", sumInt);
        Jan("total_principal", sumPrin);
        Jan("total_fees", sumFee + upFees);
        Jan("total_vat", sumTax);
        Jan("total_cashflow", sumAll + upFees + residual);
        Jan("standard_pmt", pmt);
        Jan("cost_of_borrowing", sumInt + sumFee + upFees);
        Jan("cost_borrowing_pct", Round((sumInt + sumFee + upFees) / net * 100, 4));
        Jan("effective_yield", Round((xirr ?? 0) * 100, 4));
        Jan("apr", Round((xirr ?? 0) * 100, 4));
        j.Append('}');
        j.Append('}');
        return j.ToString();
    }

    /// <summary>fn_pmt: the payment of an annuity (Excel PMT); 0 when it fails.</summary>
    private static decimal FnPmt(decimal r, decimal n, decimal pv, decimal fv, decimal tp)
    {
        try
        {
            if (n == 0)
                return 0;
            if (r == 0)
                return -(pv + fv) / n;
            var v = Power(1 + r, n);
            return -(pv * v + fv) * r / ((v - 1) * (1 + r * tp));
        }
        catch (Exception e) when (e is ArithmeticException)
        {
            return 0;
        }
    }

    /// <summary>fn_xirr: Newton's method on the dated cash flows, from 10%, at most 100 steps.</summary>
    private static decimal? FnXirr(List<decimal> cf, List<DateOnly?> dt)
    {
        if (cf.Count < 2)
            return null;
        var d0 = dt[0];
        var r = 0.1;
        for (var i = 1; i <= 100; i++)
        {
            double f = 0, df = 0;
            if (r <= -0.999999)
                r = -0.999999;
            for (var k = 0; k < cf.Count; k++)
            {
                // (dt(j) - d0) / 365: null when a date is null, and then so is the sum
                if (dt[k] is not { } d || d0 is not { } first)
                    return null;
                var t = (d.DayNumber - first.DayNumber) / 365.0;
                f += (double)cf[k] * Math.Pow(1 + r, -t);
                df += (double)cf[k] * -t * Math.Pow(1 + r, -t - 1);
            }
            if (!double.IsFinite(f) || !double.IsFinite(df))
                throw new OverflowException();
            if (df == 0)
                return null;
            var n = r - f / df;
            if (Math.Abs(n - r) < 0.00000001)
                return Math.Round((decimal)n, 10, MidpointRounding.AwayFromZero);
            r = n;
        }
        return null;
    }

    /// <summary>power(x, n) for a whole n.</summary>
    private static decimal Power(decimal x, decimal n)
    {
        if (n != Math.Floor(n))
            return (decimal)Math.Pow((double)x, (double)n);
        var e = (long)Math.Abs(n);
        decimal result = 1, b = x;
        while (e > 0)
        {
            if ((e & 1) == 1)
                result *= b;
            e >>= 1;
            if (e > 0)
                b *= b;
        }
        return n < 0 ? 1 / result : result;
    }

    private static decimal Round(decimal v, int places) => Math.Round(v, places, MidpointRounding.AwayFromZero);

    /// <summary>to_char(v, 'FM999999999999990.000...') with the given decimals.</summary>
    private static string Fixed(decimal v, int places) =>
        Round(v, places).ToString("0." + new string('0', places), CultureInfo.InvariantCulture);

    /// <summary>jn: two decimals, 0 for null.</summary>
    private static string Jn(decimal v) => Fixed(v, 2);

    /// <summary>jn6: six decimals.</summary>
    private static string Jn6(decimal v) => Fixed(v, 6);

    private static DateOnly? ToDate(string? text) =>
        text is null ? null : DateOnly.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string? Iso(DateOnly? d) => d?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>add_months: the last day of a month stays the last day; a day past the end of the month moves back to it.</summary>
    private static DateOnly? AddMonths(DateOnly? d, decimal months)
    {
        if (d is not { } date)
            return null;
        var result = date.AddMonths((int)Math.Truncate(months));
        if (date.Day == DateTime.DaysInMonth(date.Year, date.Month))
            result = new DateOnly(result.Year, result.Month, DateTime.DaysInMonth(result.Year, result.Month));
        return result;
    }
}
