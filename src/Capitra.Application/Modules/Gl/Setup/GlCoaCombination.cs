using System.Text.RegularExpressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Gl.Setup;

/// <summary>
/// C# port of FINOVA gl_coa_combination_pkg: get_or_create_combination (with validate_combination and the
/// combination rules) and assert_posting_allowed. The seven segments are always company, branch, cost centre,
/// natural account, business line, future 1 and future 2, in that order.
/// </summary>
public sealed class GlCoaCombination(ICapitraDbContext db, GlCoaSetup setup, IClock clock)
{
    /// <summary>The seven segment values of a combination, in the fixed order.</summary>
    public sealed record Codes(string? Company, string? Branch, string? CostCenter, string? Account, string? BusinessLine, string? Future1, string? Future2);

    public sealed record Result(long? Id, string Concat, string Status, string? Message);

    private static readonly string[] SegmentOrder = ["COMPANY", "BRANCH", "COST_CENTER", "NATURAL", "BUSINESS_LINE", "FUTURE1", "FUTURE2"];

    private static string? N(string? code) => GlCoaSetup.NormalizeCode(code);

    private DateOnly Today => GlTime.Today(clock);

    private Task<bool> ValueExistsAsync(long coaId, string segmentCode, string? valueCode, CancellationToken ct)
    {
        var code = N(valueCode);
        var today = Today;
        return (from v in db.Set<GlCoaSegmentValue>()
                join s in db.Set<GlCoaSegment>() on v.CoaSegmentId equals s.CoaSegmentId
                where s.CoaId == coaId && s.SegmentCode == segmentCode && v.ValueCode == code && v.IsEnabled && v.IsPostingAllowed
                      && v.EffectiveStartDate <= today && (v.EffectiveEndDate == null || v.EffectiveEndDate >= today)
                select v).AnyAsync(ct);
    }

    private Task<GlCoaSegmentValue?> SegmentValueAsync(long coaId, string segmentCode, string? valueCode, CancellationToken ct)
    {
        var code = N(valueCode);
        return (from v in db.Set<GlCoaSegmentValue>()
                join s in db.Set<GlCoaSegment>() on v.CoaSegmentId equals s.CoaSegmentId
                where s.CoaId == coaId && s.SegmentCode == segmentCode && v.ValueCode == code
                select v).FirstOrDefaultAsync(ct);
    }

    /// <summary>validate_combination: VALID, or ERROR / BLOCKED / INCOMPLETE with the reason.</summary>
    private async Task<(string Status, string? Message)> ValidateAsync(GlCoaStructure coa, Codes c, bool dynamic, CancellationToken ct)
    {
        if (!coa.IsActive)
            return ("BLOCKED", "COA is inactive.");
        if (dynamic && !coa.IsDynamicCombinationsAllowed)
            return ("BLOCKED", "Dynamic account combination creation is not allowed for this COA.");

        (string Segment, string? Value, string Message)[] checks =
        [
            ("COMPANY", c.Company, "Invalid or disabled company segment value."),
            ("BRANCH", c.Branch, "Invalid or disabled branch segment value."),
            ("COST_CENTER", c.CostCenter, "Invalid or disabled cost center segment value."),
            ("NATURAL", c.Account, "Invalid or disabled natural account segment value."),
            ("BUSINESS_LINE", c.BusinessLine, "Invalid or disabled business line segment value."),
            ("FUTURE1", c.Future1 ?? "000", "Invalid or disabled Future1 segment value."),
            ("FUTURE2", c.Future2 ?? "000", "Invalid or disabled Future2 segment value."),
        ];
        foreach (var check in checks)
        {
            if (!await ValueExistsAsync(coa.CoaId, check.Segment, check.Value, ct))
                return ("INCOMPLETE", check.Message);
        }

        var accountCode = N(c.Account);
        var account = await db.Set<GlAccount>().AsNoTracking().FirstOrDefaultAsync(a => a.CoaId == coa.CoaId && a.AccountCode == accountCode, ct);
        if (account is null)
            return ("INCOMPLETE", "Natural account master does not exist: " + c.Account);
        if (!account.IsActive)
            return ("BLOCKED", "Natural account is inactive.");
        if (account.IsSummary || !account.IsPostingAllowed)
            return ("BLOCKED", "Natural account is not allowed for posting.");

        static bool Empty(string? v) => v is null || N(v) is "000" or "ALL";
        if (account.IsBranchRequired && Empty(c.Branch))
            return ("INCOMPLETE", "Branch is required for this natural account.");
        if (account.IsCostCenterRequired && Empty(c.CostCenter))
            return ("INCOMPLETE", "Cost center is required for this natural account.");
        if (account.IsBusinessLineRequired && Empty(c.BusinessLine))
            return ("INCOMPLETE", "Business line is required for this natural account.");

        return await ApplyRulesAsync(coa.CoaId, c with { Future1 = c.Future1 ?? "000", Future2 = c.Future2 ?? "000" }, account.AccountType, ct);
    }

    /// <summary>apply_validation_rules: DENY blocks, REQUIRE makes it incomplete, WARNING adds a message.</summary>
    private async Task<(string Status, string? Message)> ApplyRulesAsync(long coaId, Codes c, string accountType, CancellationToken ct)
    {
        var today = Today;
        var rules = await db.Set<GlAccountCombinationRule>().AsNoTracking()
            .Where(r => r.CoaId == coaId && r.IsActive && r.EffectiveStartDate <= today && (r.EffectiveEndDate == null || r.EffectiveEndDate >= today))
            .OrderBy(r => r.PriorityNo).ThenBy(r => r.RuleId).ToListAsync(ct);
        string? message = null;
        foreach (var rule in rules)
        {
            var text = rule.ErrorMessageEn ?? rule.RuleNameEn;
            int total = 0, matching = 0, optional = 0, optionalMatching = 0, failedRequired = 0;
            foreach (var line in await db.Set<GlAccountCombinationRuleLine>().AsNoTracking().Where(l => l.RuleId == rule.RuleId).OrderBy(l => l.LineNo).ToListAsync(ct))
            {
                total++;
                var match = Compare(RuleValue(line.AttributeName, line.SegmentCode, c, accountType), line.OperatorCode, line.ValueFrom, line.ValueTo, line.ValueList);
                if (line.IncludeExcludeFlag == "EXCLUDE")
                    match = !match;
                if (match)
                    matching++;
                if (rule.RuleType == "REQUIRE" && line.OperatorCode == "IS_NOT_NULL")
                {
                    if (!match)
                        failedRequired++;
                }
                else
                {
                    optional++;
                    if (match)
                        optionalMatching++;
                }
            }

            if (rule.RuleType == "DENY" && total > 0 && total == matching)
                return ("BLOCKED", text);
            if (rule.RuleType == "REQUIRE" && optional == optionalMatching && failedRequired > 0)
                return ("INCOMPLETE", text);
            if (rule.RuleType == "WARNING" && total > 0 && total == matching)
                message = message is null ? text : message + " | " + text;
        }

        return ("VALID", message);
    }

    private static string? RuleValue(string? attribute, string? segment, Codes c, string accountType) =>
        N(attribute) switch
        {
            "ACCOUNT_CODE" or "ACCOUNT" => N(c.Account),
            "ACCOUNT_TYPE" => N(accountType),
            "BUSINESS_LINE" => N(c.BusinessLine),
            "BRANCH" => N(c.Branch),
            "COST_CENTER" => N(c.CostCenter),
            "LEGAL_ENTITY" => N(c.Company),
            _ => N(segment) switch
            {
                "COMPANY" => N(c.Company),
                "BRANCH" => N(c.Branch),
                "COST_CENTER" => N(c.CostCenter),
                "NATURAL" => N(c.Account),
                "BUSINESS_LINE" => N(c.BusinessLine),
                "FUTURE1" => N(c.Future1),
                "FUTURE2" => N(c.Future2),
                _ => null,
            },
        };

    private static bool InList(string? list, string? value) =>
        list is not null && value is not null && ("," + N(list) + ",").Contains("," + N(value) + ",", StringComparison.Ordinal);

    /// <summary>SQL LIKE: % is any text, _ is one character.</summary>
    private static bool Like(string? value, string? pattern) =>
        value is not null && pattern is not null
        && Regex.IsMatch(value, "^" + Regex.Escape(pattern).Replace("%", ".*", StringComparison.Ordinal).Replace("_", ".", StringComparison.Ordinal) + "$",
            RegexOptions.Singleline, TimeSpan.FromSeconds(1));

    private static bool Compare(string? actual, string? op, string? from, string? to, string? list) =>
        N(op) switch
        {
            "EQ" => actual is not null && actual == N(from),
            "NE" => (actual ?? "#NULL#") != (N(from) ?? "#NULL#"),
            "IN" => InList(list, actual),
            "NOT_IN" => !InList(list, actual),
            "LIKE" => Like(actual, N(from)),
            "NOT_LIKE" => actual is not null && from is not null && !Like(actual, N(from)),
            "BETWEEN" => actual is not null && from is not null && to is not null
                         && string.CompareOrdinal(actual, N(from)) >= 0 && string.CompareOrdinal(actual, N(to)) <= 0,
            "IS_NULL" => actual is null,
            "IS_NOT_NULL" => actual is not null && actual is not ("000" or "ALL"),
            _ => false,
        };

    /// <summary>
    /// get_or_create_combination: the existing combination with these values, or a new one after validation.
    /// A combination that fails validation raises its message.
    /// </summary>
    public async Task<Result> GetOrCreateAsync(string coaCode, Codes c, bool manualCreated, bool dynamicCreated, CancellationToken ct)
    {
        var code = N(coaCode);
        var coa = await db.Set<GlCoaStructure>().AsNoTracking().FirstOrDefaultAsync(s => s.CoaCode == code, ct)
            ?? throw new ApexProcessException("COA does not exist: " + coaCode);

        var d = coa.DelimiterChar;
        var concat = string.Join(d, N(c.Company), N(c.Branch), N(c.CostCenter), N(c.Account), N(c.BusinessLine), N(c.Future1 ?? "000"), N(c.Future2 ?? "000"));
        var existing = await db.Set<GlAccountCombination>().AsNoTracking()
            .Where(x => x.CoaId == coa.CoaId && x.ConcatenatedAccount == concat)
            .Select(x => new { x.AccountCombinationId, x.ValidationStatus, x.ValidationMessage }).FirstOrDefaultAsync(ct);
        if (existing is not null)
            return new Result(existing.AccountCombinationId, concat, existing.ValidationStatus, existing.ValidationMessage);

        var (status, message) = await ValidateAsync(coa, c, dynamicCreated, ct);
        if (status is "ERROR" or "BLOCKED" or "INCOMPLETE")
            throw new ApexProcessException(message ?? status);
        if (status == "VALID" && coa.IsCombinationApprovalRequired)
            (status, message) = ("PENDING_APPROVAL", "Combination created and pending approval.");

        var accountCode = N(c.Account);
        var accountId = await db.Set<GlAccount>().Where(a => a.CoaId == coa.CoaId && a.AccountCode == accountCode).Select(a => (long?)a.AccountId).FirstOrDefaultAsync(ct);
        var company = await SegmentValueAsync(coa.CoaId, "COMPANY", c.Company, ct);
        var branch = await SegmentValueAsync(coa.CoaId, "BRANCH", c.Branch, ct);
        var costCenter = await SegmentValueAsync(coa.CoaId, "COST_CENTER", c.CostCenter, ct);
        var natural = await SegmentValueAsync(coa.CoaId, "NATURAL", c.Account, ct);
        var businessLine = await SegmentValueAsync(coa.CoaId, "BUSINESS_LINE", c.BusinessLine, ct);

        var row = new GlAccountCombination
        {
            CoaId = coa.CoaId,
            ConcatenatedAccount = concat,
            DisplayAccount = concat,
            LegalEntityId = company?.ReferenceLegalEntityId,
            BranchId = branch?.ReferenceBranchId,
            CostCenterId = costCenter?.ReferenceCostCenterId,
            BusinessLineCode = N(c.BusinessLine),
            AccountId = accountId ?? 0,
            CompanySegmentValueId = company?.SegmentValueId,
            BranchSegmentValueId = branch?.SegmentValueId,
            CostCenterSegmentValueId = costCenter?.SegmentValueId,
            BusinessLineSegmentValueId = businessLine?.SegmentValueId,
            AccountSegmentValueId = natural?.SegmentValueId,
            IsSummary = false,
            IsPostingAllowed = status == "VALID",
            IsEnabled = true,
            ValidationStatus = status,
            ValidationMessage = message,
            IsManualCreated = manualCreated,
            IsDynamicCreated = dynamicCreated,
            EffectiveStartDate = Today,
            LastValidatedAt = clock.UtcNow,
        };
        db.Add(row);
        await db.SaveChangesAsync(ct);

        string?[] values = [c.Company, c.Branch, c.CostCenter, c.Account, c.BusinessLine, c.Future1 ?? "000", c.Future2 ?? "000"];
        for (var i = 0; i < SegmentOrder.Length; i++)
        {
            var segmentCode = SegmentOrder[i];
            var segmentId = await db.Set<GlCoaSegment>().Where(s => s.CoaId == coa.CoaId && s.SegmentCode == segmentCode).Select(s => (long?)s.CoaSegmentId).FirstOrDefaultAsync(ct);
            var value = await SegmentValueAsync(coa.CoaId, segmentCode, values[i], ct);
            if (segmentId is null || value is null)
                throw new ApexProcessException($"Cannot create combination segment. Missing segment or value: {segmentCode}={values[i]}");
            db.Add(new GlAccountCombinationSegment
            {
                AccountCombinationId = row.AccountCombinationId,
                CoaSegmentId = segmentId.Value,
                SegmentNum = i + 1,
                SegmentValueId = value.SegmentValueId,
                SegmentValueCode = N(values[i])!,
            });
        }

        setup.LogChange("GL_ACCOUNT_COMBINATIONS", concat, "CREATE", "Account combination created with status " + status);
        await db.SaveChangesAsync(ct);
        return new Result(row.AccountCombinationId, concat, status, message);
    }

    /// <summary>assert_posting_allowed: raises why a combination cannot take a posting from this source.</summary>
    public async Task AssertPostingAllowedAsync(long combinationId, string? sourceCode, CancellationToken ct)
    {
        var r = await (from ac in db.Set<GlAccountCombination>().AsNoTracking()
                       join na in db.Set<GlAccount>() on ac.AccountId equals na.AccountId
                       where ac.AccountCombinationId == combinationId
                       select new { ac, na }).FirstOrDefaultAsync(ct)
            ?? throw new ApexProcessException("Account combination does not exist: " + combinationId);

        if (!r.ac.IsEnabled)
            throw new ApexProcessException("Account combination is disabled.");
        if (r.ac.ValidationStatus != "VALID")
            throw new ApexProcessException("Account combination is not valid. Current status: " + r.ac.ValidationStatus);
        if (!r.ac.IsPostingAllowed || r.ac.IsSummary)
            throw new ApexProcessException("Account combination is not allowed for posting.");
        if (r.ac.EffectiveEndDate is not null && Today > r.ac.EffectiveEndDate)
            throw new ApexProcessException("Account combination is expired.");
        if (!r.na.IsActive || !r.na.IsPostingAllowed || r.na.IsSummary)
            throw new ApexProcessException("Natural account is not active or not allowed for posting.");

        var source = N(sourceCode);
        if (source == "MANUAL" && !r.na.IsManualJournalAllowed)
            throw new ApexProcessException("Manual journals are not allowed for this natural account.");
        if (source is not null && source != "MANUAL" && !r.na.IsSubledgerPostingAllowed)
            throw new ApexProcessException("Subledger posting is not allowed for this natural account.");
    }
}
