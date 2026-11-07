using System.Globalization;
using Capitra.Application.Modules.Crm;

namespace Capitra.Application.Modules.Adm.Settings;

/// <summary>The kind of value: yes / no, a whole number, a number with decimals, or one of a list.</summary>
public enum ModuleSettingType
{
    YesNo,
    WholeNumber,
    Number,
    Choice,
}

/// <summary>
/// One setting of Administration > Module Settings. ShownWhen names a yes/no setting of the same module: the setting is shown, and
/// used, only while that one is Yes (the waiting days only matter when there is a waiting period).
/// </summary>
public sealed record ModuleSettingDefinition(
    string ModuleCode, string Code, string Group, string Label, string Help, ModuleSettingType Type, string Default,
    decimal? Min = null, decimal? Max = null, string? Unit = null, IReadOnlyList<CrmOptionDto>? Choices = null, string? ShownWhen = null);

public sealed record ModuleDefinition(string Code, string Title, string Description, string Icon);

/// <summary>
/// The module settings (Ahmed, 7 Nov 2026: the fixed rules of CRM, Treasury and Factoring become settings an administrator can change;
/// each change needs a second user's approval). Defaults are the rules as they were before, so nothing changes until a setting is changed.
/// </summary>
public static class ModuleSettingCatalog
{
    public static class Codes
    {
        public const string CrmComplianceMakerChecker = "CRM.COMPLIANCE_MAKER_CHECKER";
        public const string CrmBankAccountMakerChecker = "CRM.BANK_ACCOUNT_MAKER_CHECKER";
        public const string CrmBankAccountWaiting = "CRM.BANK_ACCOUNT_WAITING";
        public const string CrmBankAccountWaitingDays = "CRM.BANK_ACCOUNT_WAITING_DAYS";
        public const string CrmKycExpiryWarningDays = "CRM.KYC_EXPIRY_WARNING_DAYS";
        public const string CrmReviewMonthsLow = "CRM.REVIEW_MONTHS_LOW";
        public const string CrmReviewMonthsMedium = "CRM.REVIEW_MONTHS_MEDIUM";
        public const string CrmReviewMonthsHigh = "CRM.REVIEW_MONTHS_HIGH";

        public const string TrsBankRuleMakerChecker = "TRS.BANK_RULE_MAKER_CHECKER";
        public const string TrsOverrideSecondUser = "TRS.OVERRIDE_SECOND_USER";
        public const string TrsSuggestOrder = "TRS.SUGGEST_ORDER";
        public const string TrsAllowEscrowPayment = "TRS.ALLOW_ESCROW_PAYMENT";

        public const string FctMinAdvancePct = "FCT.MIN_ADVANCE_PCT";
        public const string FctMultipleOpenOffers = "FCT.MULTIPLE_OPEN_OFFERS";
    }

    public static readonly IReadOnlyList<ModuleDefinition> Modules =
    [
        new("CRM", "CRM", "Customer compliance, bank accounts and reviews.", "fa-users"),
        new("TRS", "Treasury", "Bank rules, overrides and the source of funds.", "fa-university"),
        new("FCT", "Factoring", "The pricing calculator and the client's offers.", "fa-file-text-o"),
    ];

    public static readonly IReadOnlyList<ModuleSettingDefinition> All =
    [
        new("CRM", Codes.CrmComplianceMakerChecker, "Compliance", "Compliance profile needs a maker and a checker",
            "Yes: a second user must verify every change to a customer's KYC, AML, sanctions and risk grade. No: the user who made the change can verify it.",
            ModuleSettingType.YesNo, "true"),
        new("CRM", Codes.CrmKycExpiryWarningDays, "Compliance", "Warn before KYC expires",
            "A customer whose KYC expires within this many days shows Due for review.",
            ModuleSettingType.WholeNumber, "30", 0, 365, "days"),
        new("CRM", Codes.CrmReviewMonthsLow, "Compliance", "Review cycle, low risk",
            "When a compliance profile is verified without a next review date, the next review is set this many months ahead.",
            ModuleSettingType.WholeNumber, "36", 1, 120, "months"),
        new("CRM", Codes.CrmReviewMonthsMedium, "Compliance", "Review cycle, medium risk",
            "Also used when the customer has no risk grade.",
            ModuleSettingType.WholeNumber, "24", 1, 120, "months"),
        new("CRM", Codes.CrmReviewMonthsHigh, "Compliance", "Review cycle, high risk", "",
            ModuleSettingType.WholeNumber, "12", 1, 120, "months"),
        new("CRM", Codes.CrmBankAccountMakerChecker, "Bank accounts", "Bank account needs a maker and a checker",
            "Yes: a second user must verify a new or changed bank account before it can be paid. No: the user who made the change can verify it. We recommend Yes.",
            ModuleSettingType.YesNo, "true"),
        new("CRM", Codes.CrmBankAccountWaiting, "Bank accounts", "Wait after a bank account change",
            "Yes: a new or changed bank account cannot be paid until the waiting days have passed, even once verified.",
            ModuleSettingType.YesNo, "true"),
        new("CRM", Codes.CrmBankAccountWaitingDays, "Bank accounts", "Waiting days",
            "How many days after the last change the account can be paid.",
            ModuleSettingType.WholeNumber, "2", 1, 90, "days", ShownWhen: Codes.CrmBankAccountWaiting),

        new("TRS", Codes.TrsBankRuleMakerChecker, "Bank rules", "Bank rule needs a maker and a checker",
            "Yes: a new or changed bank rule is checked only after a second user verifies it. No: the user who saved it can verify it.",
            ModuleSettingType.YesNo, "true"),
        new("TRS", Codes.TrsOverrideSecondUser, "Bank rules", "Override needs a second user",
            "Yes: when a disbursement breaks a bank rule marked Needs override, another user must approve the override. No: the user who chose the sources can approve it.",
            ModuleSettingType.YesNo, "true"),
        new("TRS", Codes.TrsSuggestOrder, "Source of funds", "Suggest fills first",
            "The order the Suggest button on Treasury Disburse uses to share the net amount.",
            ModuleSettingType.Choice, "BANK_FIRST",
            Choices: [new("BANK_FIRST", "Bank lines first, then own funds"), new("OWN_FIRST", "Own funds first, then bank lines")]),
        new("TRS", Codes.TrsAllowEscrowPayment, "Source of funds", "Escrow accounts may pay",
            "Yes: when we have no other account at a bank, its escrow account pays the client. No: an escrow account never pays a disbursement.",
            ModuleSettingType.YesNo, "true"),

        new("FCT", Codes.FctMinAdvancePct, "Pricing", "Minimum advance rate",
            "The pricing calculator never gives an advance rate below this, whatever the dilution and risk buffer.",
            ModuleSettingType.Number, "50", 0, 100, "%"),
        new("FCT", Codes.FctMultipleOpenOffers, "Offers", "A client can have several open offers",
            "Yes: Use In Offer always creates a new offer. No: a client has one open offer at a time (draft, submitted or accepted); Use In Offer loads the open draft.",
            ModuleSettingType.YesNo, "false"),
    ];

    public static ModuleSettingDefinition? Find(string code) => All.FirstOrDefault(x => x.Code == code);

    /// <summary>The value in its stored form, or the reason it is not valid.</summary>
    public static (string? Value, string? Error) Normalize(ModuleSettingDefinition d, string? raw)
    {
        var s = raw?.Trim() ?? "";
        switch (d.Type)
        {
            case ModuleSettingType.YesNo:
                return s.ToLowerInvariant() switch { "true" or "yes" => ("true", null), "false" or "no" => ("false", null), _ => (null, "Choose Yes or No.") };
            case ModuleSettingType.WholeNumber:
                if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)) return (null, "Enter a whole number.");
                return InRange(d, i) ?? (i.ToString(CultureInfo.InvariantCulture), null);
            case ModuleSettingType.Number:
                if (!decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var m)) return (null, "Enter a number.");
                m = Math.Round(m, 4);
                return InRange(d, m) ?? (m.ToString("0.####", CultureInfo.InvariantCulture), null);
            default:
                var c = s.ToUpperInvariant();
                return d.Choices?.Any(x => x.Value == c) == true ? (c, null) : (null, "Choose from the list.");
        }
    }

    private static (string?, string?)? InRange(ModuleSettingDefinition d, decimal v) =>
        (d.Min is { } min && v < min) || (d.Max is { } max && v > max)
            ? (null, $"Enter a value from {d.Min:0.##} to {d.Max:0.##}.")
            : null;

    /// <summary>The kind as the page knows it: BOOL, INT, DECIMAL or CHOICE.</summary>
    public static string TypeCode(ModuleSettingType t) => t switch
    {
        ModuleSettingType.YesNo => "BOOL",
        ModuleSettingType.WholeNumber => "INT",
        ModuleSettingType.Number => "DECIMAL",
        _ => "CHOICE",
    };

    /// <summary>The value in words, as the page and the history show it.</summary>
    public static string Display(ModuleSettingDefinition d, string? value) => value is null ? "" : d.Type switch
    {
        ModuleSettingType.YesNo => value == "true" ? "Yes" : "No",
        ModuleSettingType.Choice => d.Choices?.FirstOrDefault(x => x.Value == value)?.Label ?? value,
        _ => d.Unit is null ? value : d.Unit == "%" ? $"{value}%" : $"{value} {d.Unit}",
    };
}
