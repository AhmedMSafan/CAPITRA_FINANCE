using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Adm.Products;

public sealed record EventTypeOptionDto(long EventTypeId, string EventTypeCode, string EventTypeNameEn);

public sealed record ProductEventRuleDto(
    long ProductEventRuleId, long ProductId, string ProductCode, string ProductNameEn, long EventTypeId, string EventTypeCode,
    string EventTypeNameEn, string RuleCode, string RuleNameEn, string? RuleNameAr, int PriorityNo, DateOnly EffectiveFromDate,
    DateOnly? EffectiveToDate, bool IsActive, int ChargeCount, int GlLineCount, int RowVersion);

public sealed record SaveProductEventRuleRequest(
    long? ProductId, long? EventTypeId, string? RuleCode, string? RuleNameEn, string? RuleNameAr, int? PriorityNo,
    DateOnly? EffectiveFromDate, DateOnly? EffectiveToDate, bool IsActive, int? RowVersion);

public sealed record RuleChargeDto(
    long RuleChargeId, long ProductEventRuleId, long ChargeId, string ChargeCode, string ChargeNameEn, int SequenceNo,
    string CalculationMethodCode, decimal? FixedAmount, decimal? ChargeRate, decimal? PerUnitAmount, string? AmountBaseCode,
    string IncomeRecognitionCode, decimal SpreadRate, int? DayCountBasis,
    decimal? MinimumAmount, decimal? MaximumAmount, int RoundingScale, string ChargeTimingCode, int DueDays, decimal TaxRate,
    DateOnly EffectiveFromDate, DateOnly? EffectiveToDate, bool IsActive, int RowVersion);

public sealed record SaveRuleChargeRequest(
    long? ChargeId, int? SequenceNo, string? CalculationMethodCode, decimal? FixedAmount, decimal? ChargeRate, decimal? PerUnitAmount,
    string? AmountBaseCode, string? IncomeRecognitionCode, decimal? SpreadRate, int? DayCountBasis, decimal? MinimumAmount, decimal? MaximumAmount, int? RoundingScale, string? ChargeTimingCode,
    int? DueDays, decimal? TaxRate, DateOnly? EffectiveFromDate, DateOnly? EffectiveToDate, bool IsActive, int? RowVersion);

public sealed record GlAccountOptionDto(long AccountId, string AccountCode, string AccountNameEn);

public sealed record RuleGlLineDto(
    long RuleGlLineId, long ProductEventRuleId, long? RuleChargeId, string? ChargeCode, int LineNo, int JournalNo, string DebitCreditCode,
    string AmountCode, string AccountSourceCode, long? AccountId, string? AccountCode, string? AccountNameEn, string? DescriptionTemplate, bool IsActive,
    int RowVersion);

public sealed record SaveRuleGlLineRequest(
    long? RuleChargeId, int? LineNo, int? JournalNo, string? DebitCreditCode, string? AmountCode, string? AccountSourceCode, long? AccountId,
    string? DescriptionTemplate, bool IsActive, int? RowVersion);

/// <summary>
/// ADM_PRODUCT_EVENT_RULES with their charges and GL entry lines, the Product Event Rules screen of Admin Setup (FINOVA_FINANCEING
/// ProductEventRulesPage and ProductEventRuleService). GL lines post to chart-of-accounts accounts directly, where
/// FINOVA_FINANCEING went through account roles and their mappings.
/// </summary>
public sealed class AdmProductEventRuleService(ICapitraDbContext db)
{
    public static readonly string[] CalculationMethods = ["FIXED", "PERCENTAGE", "PER_UNIT", "TIERED", "INDEX_PLUS_SPREAD", "DAILY_INTEREST"];

    public static readonly string[] ChargeTimings = ["DUE_SEPARATELY", "DEDUCT_FROM_DISBURSEMENT", "FINANCE_WITH_PRINCIPAL", "PAID_UPFRONT"];

    private static readonly int[] DayCountBases = [360, 365, 366];

    public async Task<IReadOnlyList<ProductEventRuleDto>> ListAsync(CancellationToken cancellationToken) =>
        await (from r in db.Set<AdmProductEventRule>().AsNoTracking()
               join p in db.Set<AdmProduct>() on r.ProductId equals p.ProductId
               join t in db.Set<GlAccountingEventType>() on r.EventTypeId equals t.EventTypeId
               orderby r.RuleNameEn
               select new ProductEventRuleDto(r.ProductEventRuleId, r.ProductId, p.ProductCode, p.ProductNameEn, r.EventTypeId, t.EventTypeCode,
                   t.EventTypeNameEn, r.RuleCode, r.RuleNameEn, r.RuleNameAr, r.PriorityNo, r.EffectiveFromDate, r.EffectiveToDate, r.IsActive,
                   r.Charges.Count, r.GlLines.Count, r.RowVersion))
            .ToListAsync(cancellationToken);

    /// <summary>Active GL accounting events, the events a rule can be set up for.</summary>
    public async Task<IReadOnlyList<EventTypeOptionDto>> ListEventTypesAsync(CancellationToken cancellationToken) =>
        await db.Set<GlAccountingEventType>().AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.EventTypeCode)
            .Select(x => new EventTypeOptionDto(x.EventTypeId, x.EventTypeCode, x.EventTypeNameEn))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// The rule a posting uses for a product and business event on a date: the active rule in effect with the lowest
    /// priority number, its charges in effect and its active GL lines; null when there is none.
    /// </summary>
    public async Task<ProductEventPlan?> PlanAsync(string productCode, string eventCode, DateOnly date, CancellationToken cancellationToken)
    {
        var rule = await (from r in db.Set<AdmProductEventRule>().AsNoTracking()
                          join p in db.Set<AdmProduct>() on r.ProductId equals p.ProductId
                          join t in db.Set<GlAccountingEventType>() on r.EventTypeId equals t.EventTypeId
                          where p.ProductCode == productCode && t.EventTypeCode == eventCode && r.IsActive
                                && r.EffectiveFromDate <= date && (r.EffectiveToDate == null || r.EffectiveToDate >= date)
                          orderby r.PriorityNo, r.ProductEventRuleId
                          select new { r.ProductEventRuleId, r.RuleCode }).FirstOrDefaultAsync(cancellationToken);
        if (rule is null)
            return null;

        var charges = await (from rc in db.Set<AdmProductEventRuleCharge>().AsNoTracking()
                             join c in db.Set<AdmCharge>() on rc.ChargeId equals c.ChargeId
                             where rc.ProductEventRuleId == rule.ProductEventRuleId && rc.IsActive
                                   && rc.EffectiveFromDate <= date && (rc.EffectiveToDate == null || rc.EffectiveToDate >= date)
                             orderby rc.SequenceNo
                             select new ProductEventCharge(rc.RuleChargeId, rc.SequenceNo, c.ChargeCode, c.ChargeNameEn, c.ChargeNameAr,
                                 rc.CalculationMethodCode, rc.FixedAmount, rc.ChargeRate, rc.PerUnitAmount, rc.AmountBaseCode, rc.IncomeRecognitionCode,
                                 rc.MinimumAmount, rc.MaximumAmount, rc.RoundingScale, rc.ChargeTimingCode, rc.TaxRate)).ToListAsync(cancellationToken);
        var lines = await (from l in db.Set<AdmProductEventRuleGlLine>().AsNoTracking()
                           join a in db.Set<GlAccount>() on l.AccountId equals a.AccountId into accs
                           from a in accs.DefaultIfEmpty()
                           where l.ProductEventRuleId == rule.ProductEventRuleId && l.IsActive
                           select new { l, AccountCode = a == null ? null : a.AccountCode }).ToListAsync(cancellationToken);

        var byId = charges.ToDictionary(x => x.RuleChargeId);
        return new ProductEventPlan(rule.ProductEventRuleId, rule.RuleCode, eventCode, charges,
            [.. lines.Where(x => x.l.RuleChargeId is null || byId.ContainsKey(x.l.RuleChargeId.Value))
                .Select(x =>
                {
                    var charge = x.l.RuleChargeId is { } id ? byId[id] : null;
                    return new ProductEventGlLine(x.l.JournalNo, x.l.LineNo, charge?.SequenceNo, charge?.ChargeCode, x.l.DebitCreditCode, x.l.AmountCode,
                        x.l.AccountSourceCode == AdmProductEventRuleGlLine.AccountSourceBankAccount, x.AccountCode, x.l.DescriptionTemplate);
                })]);
    }

    public async Task<ProductEventRuleDto> GetAsync(long id, CancellationToken cancellationToken) =>
        (await ListAsync(cancellationToken)).FirstOrDefault(x => x.ProductEventRuleId == id)
            ?? throw new NotFoundException($"Product event rule {id} was not found.");

    public async Task<ProductEventRuleDto> CreateAsync(SaveProductEventRuleRequest request, CancellationToken cancellationToken)
    {
        var entity = new AdmProductEventRule();
        await ApplyAsync(entity, request, cancellationToken);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(entity.ProductEventRuleId, cancellationToken);
    }

    public async Task<ProductEventRuleDto> UpdateAsync(long id, SaveProductEventRuleRequest request, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (request.RowVersion != entity.RowVersion)
            throw ChangedByAnotherUser("product event rule");

        await ApplyAsync(entity, request, cancellationToken);
        await SaveAsync("product event rule", cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    /// <summary>Deletes the rule with its charges, as FINOVA_FINANCEING did.</summary>
    public async Task DeleteAsync(long id, int rowVersion, CancellationToken cancellationToken)
    {
        var entity = await db.Set<AdmProductEventRule>().Include(x => x.Charges).FirstOrDefaultAsync(x => x.ProductEventRuleId == id, cancellationToken)
            ?? throw new NotFoundException($"Product event rule {id} was not found.");
        if (rowVersion != entity.RowVersion)
            throw ChangedByAnotherUser("product event rule");

        db.Remove(entity);
        await SaveAsync("product event rule", cancellationToken);
    }

    public async Task<IReadOnlyList<RuleChargeDto>> ListChargesAsync(long ruleId, CancellationToken cancellationToken)
    {
        await FindAsync(ruleId, cancellationToken);
        return await (from x in db.Set<AdmProductEventRuleCharge>().AsNoTracking()
                      join c in db.Set<AdmCharge>() on x.ChargeId equals c.ChargeId
                      where x.ProductEventRuleId == ruleId
                      orderby x.SequenceNo, c.ChargeNameEn
                      select new RuleChargeDto(x.RuleChargeId, x.ProductEventRuleId, x.ChargeId, c.ChargeCode, c.ChargeNameEn, x.SequenceNo,
                          x.CalculationMethodCode, x.FixedAmount, x.ChargeRate, x.PerUnitAmount, x.AmountBaseCode, x.IncomeRecognitionCode, x.SpreadRate, x.DayCountBasis, x.MinimumAmount,
                          x.MaximumAmount, x.RoundingScale, x.ChargeTimingCode, x.DueDays, x.TaxRate, x.EffectiveFromDate, x.EffectiveToDate,
                          x.IsActive, x.RowVersion))
            .ToListAsync(cancellationToken);
    }

    public async Task<RuleChargeDto> CreateChargeAsync(long ruleId, SaveRuleChargeRequest request, CancellationToken cancellationToken)
    {
        await FindAsync(ruleId, cancellationToken);
        var entity = new AdmProductEventRuleCharge { ProductEventRuleId = ruleId };
        await ApplyChargeAsync(entity, request, cancellationToken);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return await GetChargeAsync(ruleId, entity.RuleChargeId, cancellationToken);
    }

    public async Task<RuleChargeDto> UpdateChargeAsync(long ruleId, long id, SaveRuleChargeRequest request, CancellationToken cancellationToken)
    {
        var entity = await FindChargeAsync(ruleId, id, cancellationToken);
        if (request.RowVersion != entity.RowVersion)
            throw ChangedByAnotherUser("rule charge");

        await ApplyChargeAsync(entity, request, cancellationToken);
        await SaveAsync("rule charge", cancellationToken);
        return await GetChargeAsync(ruleId, id, cancellationToken);
    }

    public async Task DeleteChargeAsync(long ruleId, long id, int rowVersion, CancellationToken cancellationToken)
    {
        var entity = await FindChargeAsync(ruleId, id, cancellationToken);
        if (rowVersion != entity.RowVersion)
            throw ChangedByAnotherUser("rule charge");

        db.Remove(entity);
        await SaveAsync("rule charge", cancellationToken);
    }

    /// <summary>Active posting accounts of the chart of accounts, the accounts a GL line can post to.</summary>
    public async Task<IReadOnlyList<GlAccountOptionDto>> ListGlAccountsAsync(CancellationToken cancellationToken) =>
        await db.Set<GlAccount>().AsNoTracking().Where(x => x.IsActive && x.IsPostingAllowed && !x.IsSummary).OrderBy(x => x.AccountCode)
            .Select(x => new GlAccountOptionDto(x.AccountId, x.AccountCode, x.AccountNameEn))
            .ToListAsync(cancellationToken);

    /// <summary>The rule's own lines first, then each charge's lines, in journal and line order.</summary>
    public async Task<IReadOnlyList<RuleGlLineDto>> ListGlLinesAsync(long ruleId, CancellationToken cancellationToken)
    {
        await FindAsync(ruleId, cancellationToken);
        var rows = await (from l in db.Set<AdmProductEventRuleGlLine>().AsNoTracking()
                          join a in db.Set<GlAccount>() on l.AccountId equals a.AccountId into accs
                          from a in accs.DefaultIfEmpty()
                          join rc in db.Set<AdmProductEventRuleCharge>() on l.RuleChargeId equals rc.RuleChargeId into rcs
                          from rc in rcs.DefaultIfEmpty()
                          join c in db.Set<AdmCharge>() on rc.ChargeId equals c.ChargeId into cs
                          from c in cs.DefaultIfEmpty()
                          where l.ProductEventRuleId == ruleId
                          select new { l, AccountCode = a == null ? null : a.AccountCode, AccountNameEn = a == null ? null : a.AccountNameEn, ChargeCode = c == null ? null : c.ChargeCode, Seq = rc == null ? (int?)null : rc.SequenceNo })
            .ToListAsync(cancellationToken);
        return [.. rows.OrderBy(x => x.Seq.HasValue).ThenBy(x => x.Seq).ThenBy(x => x.l.JournalNo).ThenBy(x => x.l.LineNo)
            .Select(x => new RuleGlLineDto(x.l.RuleGlLineId, x.l.ProductEventRuleId, x.l.RuleChargeId, x.ChargeCode, x.l.LineNo, x.l.JournalNo, x.l.DebitCreditCode,
                x.l.AmountCode, x.l.AccountSourceCode, x.l.AccountId, x.AccountCode, x.AccountNameEn, x.l.DescriptionTemplate, x.l.IsActive, x.l.RowVersion))];
    }

    public async Task<RuleGlLineDto> CreateGlLineAsync(long ruleId, SaveRuleGlLineRequest request, CancellationToken cancellationToken)
    {
        await FindAsync(ruleId, cancellationToken);
        var entity = new AdmProductEventRuleGlLine { ProductEventRuleId = ruleId };
        await ApplyGlLineAsync(entity, request, cancellationToken);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return (await ListGlLinesAsync(ruleId, cancellationToken)).First(x => x.RuleGlLineId == entity.RuleGlLineId);
    }

    public async Task<RuleGlLineDto> UpdateGlLineAsync(long ruleId, long id, SaveRuleGlLineRequest request, CancellationToken cancellationToken)
    {
        var entity = await FindGlLineAsync(ruleId, id, cancellationToken);
        if (request.RowVersion != entity.RowVersion)
            throw ChangedByAnotherUser("GL line");

        await ApplyGlLineAsync(entity, request, cancellationToken);
        await SaveAsync("GL line", cancellationToken);
        return (await ListGlLinesAsync(ruleId, cancellationToken)).First(x => x.RuleGlLineId == id);
    }

    public async Task DeleteGlLineAsync(long ruleId, long id, int rowVersion, CancellationToken cancellationToken)
    {
        var entity = await FindGlLineAsync(ruleId, id, cancellationToken);
        if (rowVersion != entity.RowVersion)
            throw ChangedByAnotherUser("GL line");

        db.Remove(entity);
        await SaveAsync("GL line", cancellationToken);
    }

    private async Task ApplyGlLineAsync(AdmProductEventRuleGlLine entity, SaveRuleGlLineRequest r, CancellationToken cancellationToken)
    {
        var e = new SetupErrors();
        var dc = e.OneOf(nameof(r.DebitCreditCode), "Debit/Credit", r.DebitCreditCode, ["D", "C"]);
        var description = e.Optional(nameof(r.DescriptionTemplate), "Description", r.DescriptionTemplate, 500);
        var lineNo = r.LineNo ?? 10;
        var journalNo = r.JournalNo ?? 1;
        if (journalNo < 1)
            e.Add(nameof(r.JournalNo), "Journal number must be 1 or more.");
        if (lineNo < 1)
            e.Add(nameof(r.LineNo), "Line number must be 1 or more.");
        if (r.RuleChargeId is not null
            && !await db.Set<AdmProductEventRuleCharge>().AnyAsync(x => x.RuleChargeId == r.RuleChargeId && x.ProductEventRuleId == entity.ProductEventRuleId, cancellationToken))
            e.Add(nameof(r.RuleChargeId), "The selected charge does not belong to this product event rule.");
        var amount = e.OneOf(nameof(r.AmountCode), "Amount", r.AmountCode,
            r.RuleChargeId is null ? AdmProductEventRuleGlLine.RuleAmounts : AdmProductEventRuleGlLine.ChargeAmounts);
        var source = e.OneOf(nameof(r.AccountSourceCode), "Account source", r.AccountSourceCode ?? AdmProductEventRuleGlLine.AccountSourceAccount,
            [AdmProductEventRuleGlLine.AccountSourceAccount, AdmProductEventRuleGlLine.AccountSourceBankAccount]);
        var bank = source == AdmProductEventRuleGlLine.AccountSourceBankAccount;
        if (bank)
        {
            if (r.AccountId is not null)
                e.Add(nameof(r.AccountId), "Leave the account empty: it comes from the bank account chosen at the event.");
        }
        else if (r.AccountId is null)
            e.Add(nameof(r.AccountId), "Account is required.");
        else if (r.AccountId != entity.AccountId
                 && !await db.Set<GlAccount>().AnyAsync(x => x.AccountId == r.AccountId && x.IsActive && x.IsPostingAllowed && !x.IsSummary, cancellationToken))
            e.Add(nameof(r.AccountId), "Select an active posting account.");
        if (await db.Set<AdmProductEventRuleGlLine>().AnyAsync(x => x.ProductEventRuleId == entity.ProductEventRuleId && x.RuleChargeId == r.RuleChargeId
                                                                    && x.JournalNo == journalNo && x.LineNo == lineNo && x.RuleGlLineId != entity.RuleGlLineId, cancellationToken))
            e.Add(nameof(r.LineNo), "Line number already exists in this journal of this section.");
        e.ThrowIfAny();

        entity.RuleChargeId = r.RuleChargeId;
        entity.LineNo = lineNo;
        entity.JournalNo = journalNo;
        entity.DebitCreditCode = dc;
        entity.AmountCode = amount;
        entity.AccountSourceCode = source;
        entity.AccountId = bank ? null : r.AccountId;
        entity.DescriptionTemplate = description;
        entity.IsActive = r.IsActive;
    }

    private async Task<AdmProductEventRuleGlLine> FindGlLineAsync(long ruleId, long id, CancellationToken cancellationToken) =>
        await db.Set<AdmProductEventRuleGlLine>().FirstOrDefaultAsync(x => x.RuleGlLineId == id && x.ProductEventRuleId == ruleId, cancellationToken)
            ?? throw new NotFoundException($"GL line {id} was not found on product event rule {ruleId}.");

    private async Task ApplyAsync(AdmProductEventRule entity, SaveProductEventRuleRequest r, CancellationToken cancellationToken)
    {
        var e = new SetupErrors();
        var code = e.Required(nameof(r.RuleCode), "Rule code", r.RuleCode, 50, upper: true);
        var nameEn = e.Required(nameof(r.RuleNameEn), "Rule name (EN)", r.RuleNameEn, 200);
        var nameAr = e.Optional(nameof(r.RuleNameAr), "Rule name (AR)", r.RuleNameAr, 200);
        if (r.ProductId is null)
            e.Add(nameof(r.ProductId), "Product is required.");
        else if (!await db.Set<AdmProduct>().AnyAsync(x => x.ProductId == r.ProductId, cancellationToken))
            e.Add(nameof(r.ProductId), "The selected product does not exist.");
        if (r.EventTypeId is null)
            e.Add(nameof(r.EventTypeId), "Accounting event is required.");
        else if (!await db.Set<GlAccountingEventType>().AnyAsync(x => x.EventTypeId == r.EventTypeId && x.IsActive, cancellationToken))
            e.Add(nameof(r.EventTypeId), "The selected accounting event does not exist or is inactive.");
        if (r.EffectiveFromDate is null)
            e.Add(nameof(r.EffectiveFromDate), "Effective From is required.");
        e.Dates(nameof(r.EffectiveToDate), r.EffectiveFromDate, r.EffectiveToDate);

        if (code.Length > 0 && r.ProductId is not null && r.EventTypeId is not null
            && await db.Set<AdmProductEventRule>().AnyAsync(x => x.ProductId == r.ProductId && x.EventTypeId == r.EventTypeId && x.RuleCode == code
                                                                 && x.ProductEventRuleId != entity.ProductEventRuleId, cancellationToken))
            e.Add(nameof(r.RuleCode), "Rule code already exists for the selected product and event.");
        e.ThrowIfAny();

        entity.ProductId = r.ProductId!.Value;
        entity.EventTypeId = r.EventTypeId!.Value;
        entity.RuleCode = code;
        entity.RuleNameEn = nameEn;
        entity.RuleNameAr = nameAr;
        entity.PriorityNo = r.PriorityNo ?? 10;
        entity.EffectiveFromDate = r.EffectiveFromDate!.Value;
        entity.EffectiveToDate = r.EffectiveToDate;
        entity.IsActive = r.IsActive;
    }

    private async Task ApplyChargeAsync(AdmProductEventRuleCharge entity, SaveRuleChargeRequest r, CancellationToken cancellationToken)
    {
        var e = new SetupErrors();
        var method = e.OneOf(nameof(r.CalculationMethodCode), "Calculation method", r.CalculationMethodCode, CalculationMethods);
        var timing = e.OneOf(nameof(r.ChargeTimingCode), "Charge timing", r.ChargeTimingCode ?? "DUE_SEPARATELY", ChargeTimings);
        var income = e.OneOf(nameof(r.IncomeRecognitionCode), "Income recognition", r.IncomeRecognitionCode ?? "PRORATE",
            AdmProductEventRuleCharge.IncomeRecognitions);
        var amountBase = string.IsNullOrWhiteSpace(r.AmountBaseCode)
            ? null
            : e.OneOf(nameof(r.AmountBaseCode), "Amount base", r.AmountBaseCode, AdmProductEventRuleCharge.AmountBases);
        if (method == "PERCENTAGE" && amountBase is null)
            e.Add(nameof(r.AmountBaseCode), "Amount base is required for a percentage charge.");
        if (method == "FIXED" && r.FixedAmount is null)
            e.Add(nameof(r.FixedAmount), "Fixed amount is required for a fixed charge.");
        if (method == "PERCENTAGE" && r.ChargeRate is null)
            e.Add(nameof(r.ChargeRate), "Rate is required for a percentage charge.");
        if (method == "PER_UNIT" && r.PerUnitAmount is null)
            e.Add(nameof(r.PerUnitAmount), "Per unit amount is required for a per unit charge.");
        if (r.ChargeId is null)
            e.Add(nameof(r.ChargeId), "Charge is required.");
        else if (r.ChargeId != entity.ChargeId && !await db.Set<AdmCharge>().AnyAsync(x => x.ChargeId == r.ChargeId && x.IsActive, cancellationToken))
            e.Add(nameof(r.ChargeId), "The selected charge does not exist or is inactive.");
        else if (await db.Set<AdmProductEventRuleCharge>().AnyAsync(x => x.ProductEventRuleId == entity.ProductEventRuleId && x.ChargeId == r.ChargeId
                                                                         && x.RuleChargeId != entity.RuleChargeId, cancellationToken))
            e.Add(nameof(r.ChargeId), "This charge is already linked to the product event rule.");
        e.NotNegative(nameof(r.FixedAmount), "Fixed amount", r.FixedAmount);
        e.NotNegative(nameof(r.PerUnitAmount), "Per unit amount", r.PerUnitAmount);
        e.NotNegative(nameof(r.MinimumAmount), "Minimum amount", r.MinimumAmount);
        e.NotNegative(nameof(r.MaximumAmount), "Maximum amount", r.MaximumAmount);
        e.Percent(nameof(r.ChargeRate), "Rate", r.ChargeRate);
        e.Percent(nameof(r.SpreadRate), "Spread", r.SpreadRate);
        e.Percent(nameof(r.TaxRate), "Tax rate", r.TaxRate);
        if (r.MinimumAmount.HasValue && r.MaximumAmount.HasValue && r.MaximumAmount < r.MinimumAmount)
            e.Add(nameof(r.MaximumAmount), "Maximum amount cannot be less than the minimum amount.");
        if (r.DayCountBasis.HasValue && !DayCountBases.Contains(r.DayCountBasis.Value))
            e.Add(nameof(r.DayCountBasis), "Day count basis must be 360, 365 or 366.");
        if (r.RoundingScale is < 0 or > 6)
            e.Add(nameof(r.RoundingScale), "Rounding scale must be between 0 and 6.");
        if (r.DueDays < 0)
            e.Add(nameof(r.DueDays), "Due days cannot be negative.");
        if (r.EffectiveFromDate is null)
            e.Add(nameof(r.EffectiveFromDate), "Effective From is required.");
        e.Dates(nameof(r.EffectiveToDate), r.EffectiveFromDate, r.EffectiveToDate);
        e.ThrowIfAny();

        entity.ChargeId = r.ChargeId!.Value;
        entity.SequenceNo = r.SequenceNo ?? 10;
        entity.CalculationMethodCode = method;
        entity.FixedAmount = r.FixedAmount;
        entity.ChargeRate = r.ChargeRate;
        entity.PerUnitAmount = r.PerUnitAmount;
        entity.AmountBaseCode = method == "PERCENTAGE" ? amountBase : null;
        entity.IncomeRecognitionCode = income;
        entity.SpreadRate = r.SpreadRate ?? 0m;
        entity.DayCountBasis = r.DayCountBasis;
        entity.MinimumAmount = r.MinimumAmount;
        entity.MaximumAmount = r.MaximumAmount;
        entity.RoundingScale = r.RoundingScale ?? 2;
        entity.ChargeTimingCode = timing;
        entity.DueDays = r.DueDays ?? 0;
        entity.TaxRate = r.TaxRate ?? 0m;
        entity.EffectiveFromDate = r.EffectiveFromDate!.Value;
        entity.EffectiveToDate = r.EffectiveToDate;
        entity.IsActive = r.IsActive;
    }

    private async Task<RuleChargeDto> GetChargeAsync(long ruleId, long id, CancellationToken cancellationToken) =>
        (await ListChargesAsync(ruleId, cancellationToken)).First(x => x.RuleChargeId == id);

    private async Task<AdmProductEventRule> FindAsync(long id, CancellationToken cancellationToken) =>
        await db.Set<AdmProductEventRule>().FirstOrDefaultAsync(x => x.ProductEventRuleId == id, cancellationToken)
            ?? throw new NotFoundException($"Product event rule {id} was not found.");

    private async Task<AdmProductEventRuleCharge> FindChargeAsync(long ruleId, long id, CancellationToken cancellationToken) =>
        await db.Set<AdmProductEventRuleCharge>().FirstOrDefaultAsync(x => x.RuleChargeId == id && x.ProductEventRuleId == ruleId, cancellationToken)
            ?? throw new NotFoundException($"Rule charge {id} was not found on product event rule {ruleId}.");

    private async Task SaveAsync(string what, CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw ChangedByAnotherUser(what);
        }
    }

    private static ConflictException ChangedByAnotherUser(string what) => new($"This {what} was changed by another user. Reload and try again.");
}
