using Capitra.Application.Modules.Lnd;
using Capitra.Domain.Modules.Lnd;
using Xunit;
using C = Capitra.Application.Modules.Lnd.LndParameterCatalog.Codes;
using S = Capitra.Application.Modules.Lnd.LndParameterCatalog.Statuses;

namespace Capitra.Tests.Lnd;

/// <summary>Retail Lending > Parameters: the catalog, the value in force on a day and the go-live check.</summary>
public sealed class LndParameterTests
{
    private const string Mf = LndParameterCatalog.Microfinance;
    private const string Cf = LndParameterCatalog.ConsumerFinance;
    private static readonly DateOnly Day = new(2026, 12, 1);

    private static long _id;

    private static LndParameterValue V(string code, string scope, string value, DateOnly from, string status = S.Approved, int decidedMinute = 0) =>
        new()
        {
            ParameterValueId = ++_id, ParameterCode = code, ScopeCode = scope, ValueText = value, EffectiveFromDate = from, StatusCode = status,
            RequestedBy = "maker", RequestReason = "test", DecidedBy = "checker", DecidedAt = new DateTime(2026, 11, 1, 10, decidedMinute, 0, DateTimeKind.Utc),
        };

    /// <summary>Every consumer finance parameter with its suggested value, approved from a day before Day.</summary>
    private static List<LndParameterValue> SuggestedConsumerFinance() =>
        [.. LndParameterCatalog.Parameters.Where(d => d.Scopes.Contains(Cf)).Select(d =>
            V(d.Code, Cf, d.Suggested?.GetValueOrDefault(Cf) ?? (d.Code == C.NonAccrualDays ? "90" : d.Code == C.WriteOffDays ? "360" : "1"), Day.AddDays(-10)))];

    [Fact]
    public void Every_parameter_has_a_unique_code_a_known_licence_and_valid_suggestions()
    {
        var scopes = LndParameterCatalog.Scopes.Select(x => x.Code).ToHashSet();
        Assert.Equal(LndParameterCatalog.Parameters.Count, LndParameterCatalog.Parameters.Select(x => x.Code).Distinct().Count());
        foreach (var d in LndParameterCatalog.Parameters)
        {
            Assert.StartsWith("LND.", d.Code, StringComparison.Ordinal);
            Assert.NotEmpty(d.Scopes);
            Assert.All(d.Scopes, s => Assert.Contains(s, scopes));
            Assert.False(string.IsNullOrWhiteSpace(d.Source), d.Code);
            foreach (var (scope, value) in d.Suggested ?? new Dictionary<string, string>())
            {
                Assert.Contains(scope, d.Scopes);
                Assert.Equal((value, (string?)null), LndParameterCatalog.Normalize(d, value));
            }
        }
    }

    [Theory]
    [InlineData(C.MaxFinancingAmount, "292,000.00", "292000")]
    [InlineData(C.OtpAtSigning, "Yes", "true")]
    [InlineData(C.InsuranceMaxAge, " 65 ", "65")]
    public void A_value_is_stored_in_one_form(string code, string raw, string stored) =>
        Assert.Equal(stored, LndParameterCatalog.Normalize(LndParameterCatalog.Find(code)!, raw).Value);

    [Theory]
    [InlineData(C.MaxInstalmentIncomePct, "101")]
    [InlineData(C.InsuranceMaxAge, "64.5")]
    [InlineData(C.OtpAtUse, "maybe")]
    [InlineData(C.GroupMinMembers, "1")]
    public void A_value_out_of_its_range_is_refused(string code, string raw) =>
        Assert.NotNull(LndParameterCatalog.Normalize(LndParameterCatalog.Find(code)!, raw).Error);

    [Fact]
    public void The_value_in_force_is_the_latest_approved_one_that_has_started()
    {
        List<LndParameterValue> rows =
        [
            V(C.MaxFinancingAmount, Mf, "266000", new(2025, 4, 1)),
            V(C.MaxFinancingAmount, Mf, "292000", new(2026, 1, 15)),
            V(C.MaxFinancingAmount, Mf, "350000", new(2026, 11, 20), S.Pending),
            V(C.MaxFinancingAmount, Mf, "300000", new(2026, 11, 25), S.Rejected),
            V(C.MaxFinancingAmount, Mf, "320000", new(2027, 1, 1)),
            V(C.MaxFinancingAmount, Cf, "999", new(2025, 1, 1)),
        ];
        Assert.Equal("266000", LndParameterCatalog.InForce(rows, C.MaxFinancingAmount, Mf, new(2026, 1, 14))!.ValueText);
        Assert.Equal("292000", LndParameterCatalog.InForce(rows, C.MaxFinancingAmount, Mf, Day)!.ValueText);
        Assert.Equal("320000", LndParameterCatalog.Next(rows, C.MaxFinancingAmount, Mf, Day)!.ValueText);
        Assert.Equal("320000", LndParameterCatalog.InForce(rows, C.MaxFinancingAmount, Mf, new(2027, 1, 1))!.ValueText);
        Assert.Null(LndParameterCatalog.InForce(rows, C.MaxFinancingAmount, Mf, new(2025, 3, 31)));
    }

    [Fact]
    public void Two_values_from_the_same_day_use_the_one_approved_last()
    {
        List<LndParameterValue> rows = [V(C.OtpAtUse, Cf, "false", Day, decidedMinute: 30), V(C.OtpAtUse, Cf, "true", Day, decidedMinute: 5)];
        Assert.Equal("false", LndParameterCatalog.InForce(rows, C.OtpAtUse, Cf, Day)!.ValueText);
    }

    [Fact]
    public void A_licence_with_every_value_approved_passes_the_go_live_check() =>
        Assert.Empty(LndParameterCatalog.Problems(SuggestedConsumerFinance(), Cf, Day));

    [Fact]
    public void A_missing_or_not_yet_started_value_blocks_go_live()
    {
        var rows = SuggestedConsumerFinance();
        rows.RemoveAll(x => x.ParameterCode == C.CashMaxAmount);
        rows.Single(x => x.ParameterCode == C.OtpAtUse).EffectiveFromDate = Day.AddDays(1);
        var problems = LndParameterCatalog.Problems(rows, Cf, Day);
        Assert.Equal(2, problems.Count);
        Assert.Contains(problems, p => p.StartsWith("Maximum cash financing per customer", StringComparison.Ordinal));

        // Microfinance has no approved values at all: every one of its parameters is listed
        Assert.Equal(LndParameterCatalog.Parameters.Count(d => d.Scopes.Contains(Mf)), LndParameterCatalog.Problems(rows, Mf, Day).Count);
    }

    [Fact]
    public void Buckets_must_rise_and_write_off_comes_after_non_accrual()
    {
        var rows = SuggestedConsumerFinance();
        rows.Add(V(C.Bucket3MaxDays, Cf, "60", Day));
        rows.Add(V(C.WriteOffDays, Cf, "30", Day));
        var problems = LndParameterCatalog.Problems(rows, Cf, Day);
        Assert.Equal(2, problems.Count); // bucket 3 (60) ends before bucket 2 (90); write-off (30) before non-accrual (90)
        Assert.Contains(problems, p => p == "Bucket 3 must end later than bucket 2.");
        Assert.Contains(problems, p => p.StartsWith("Write-off", StringComparison.Ordinal));
    }

    [Fact]
    public void Values_are_shown_in_words() =>
        Assert.Equal(["EGP 292,000", "50%", "Yes", "65 years"],
            new[] { (C.MaxFinancingAmount, "292000"), (C.MaxInstalmentIncomePct, "50"), (C.OtpAtSigning, "true"), (C.InsuranceMaxAge, "65") }
                .Select(x => LndParameterCatalog.Display(LndParameterCatalog.Find(x.Item1)!, x.Item2)));
}
