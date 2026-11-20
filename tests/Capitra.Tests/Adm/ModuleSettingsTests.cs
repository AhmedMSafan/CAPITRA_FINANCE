using Capitra.Application.Modules.Adm.Settings;
using Capitra.Application.Modules.Crm;
using Capitra.Application.Modules.Fct;
using Capitra.Application.Modules.Trs;
using Xunit;
using C = Capitra.Application.Modules.Adm.Settings.ModuleSettingCatalog.Codes;

namespace Capitra.Tests.Adm;

/// <summary>Administration > Module Settings: the values a change may take, the defaults, and the rules that read them.</summary>
public sealed class ModuleSettingsTests
{
    private static ModuleSettingDefinition D(string code) => ModuleSettingCatalog.Find(code)!;

    [Fact]
    public void Every_setting_has_a_valid_default_and_a_known_module()
    {
        var modules = ModuleSettingCatalog.Modules.Select(x => x.Code).ToHashSet();
        foreach (var d in ModuleSettingCatalog.All)
        {
            Assert.Contains(d.ModuleCode, modules);
            Assert.Equal((d.Default, (string?)null), ModuleSettingCatalog.Normalize(d, d.Default));
            if (d.ShownWhen is not null) Assert.Equal(ModuleSettingType.YesNo, D(d.ShownWhen).Type);
        }
        Assert.Equal(ModuleSettingCatalog.All.Count, ModuleSettingCatalog.All.Select(x => x.Code).Distinct().Count());
    }

    [Theory]
    [InlineData(C.CrmBankAccountMakerChecker, "Yes", "true")]
    [InlineData(C.CrmBankAccountMakerChecker, "false", "false")]
    [InlineData(C.CrmBankAccountWaitingDays, " 5 ", "5")]
    [InlineData(C.FctMinAdvancePct, "62.5", "62.5")]
    [InlineData(C.TrsSuggestOrder, "own_first", "OWN_FIRST")]
    public void A_value_is_stored_in_one_form(string code, string raw, string stored) =>
        Assert.Equal(stored, ModuleSettingCatalog.Normalize(D(code), raw).Value);

    [Theory]
    [InlineData(C.CrmBankAccountMakerChecker, "maybe")]
    [InlineData(C.CrmBankAccountWaitingDays, "0")]
    [InlineData(C.CrmBankAccountWaitingDays, "91")]
    [InlineData(C.CrmBankAccountWaitingDays, "2.5")]
    [InlineData(C.FctMinAdvancePct, "101")]
    [InlineData(C.TrsSuggestOrder, "RANDOM")]
    public void A_value_out_of_its_range_or_list_is_refused(string code, string raw) =>
        Assert.NotNull(ModuleSettingCatalog.Normalize(D(code), raw).Error);

    [Fact]
    public void Waiting_days_default_to_appsettings_and_zero_there_means_no_waiting()
    {
        Assert.Equal("4", ModuleSettings.Defaults(new CrmSettings(4))[C.CrmBankAccountWaitingDays]);
        Assert.Equal("true", ModuleSettings.Defaults(new CrmSettings(4))[C.CrmBankAccountWaiting]);
        Assert.Equal("false", ModuleSettings.Defaults(new CrmSettings(0))[C.CrmBankAccountWaiting]);
    }

    [Fact]
    public async Task No_waiting_period_gives_zero_days_and_the_review_cycle_follows_the_risk_grade()
    {
        var s = new FixedModuleSettings(new Dictionary<string, string> { [C.CrmBankAccountWaiting] = "false", [C.CrmReviewMonthsHigh] = "6" });
        Assert.Equal(0, await s.BankAccountWaitingDaysAsync(default));
        Assert.Equal(6, await s.ReviewMonthsAsync("HIGH", default));
        Assert.Equal(24, await s.ReviewMonthsAsync(null, default));
        Assert.Equal(2, await new FixedModuleSettings().BankAccountWaitingDaysAsync(default));
    }

    [Fact]
    public void The_minimum_advance_rate_is_the_floor_of_the_calculator()
    {
        // 80% base, 30% dilution, 4% risk buffer: 46%, below the floor
        var r = new FctCalculationRequest(1_000_000, null, 90, null, null, "MEDIUM", 80, 30, null, null, 10, 2, null, null, null, null, null, null);
        Assert.Equal(50m, FctApplicationService.Calculate(r, 0.50m).AdvancePct);
        Assert.Equal(46m, FctApplicationService.Calculate(r, 0.40m).AdvancePct);
    }

    [Fact]
    public void Escrow_accounts_pay_only_when_allowed()
    {
        TrsPayingAccount[] accounts =
        [
            new(1, 10, "NBE", "OP", "100", "OPERATIONAL", "EGP", "110102"),
            new(4, 30, "Banque Misr", "ESC", "300", "ESCROW", "EGP", "110107"),
        ];
        Assert.Equal(4, TrsDisburseService.PickAccount(TrsDisburseService.CanPay(accounts, true), 30)?.AccountId);
        Assert.Null(TrsDisburseService.PickAccount(TrsDisburseService.CanPay(accounts, false), 30));
    }
}
