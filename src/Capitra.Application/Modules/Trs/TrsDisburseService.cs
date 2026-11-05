using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Capitra.Domain.Modules.Trs;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Trs;

public sealed record TrsDisburseHeaderDto(
    long DisbursementId, string DisbursementNumber, long ClientId, string ClientName, string? AgreementNumber, string BusinessLine, bool IsReverse,
    string CurrencyCode, decimal NetAmount, int InvoiceCount, int? MaxTenorDays, string FundingStatus, string? ExecutedBy, DateTime? ExecutedAt);

/// <summary>Who the disbursement exposes a bank to. Role is DEBTOR or SELLER; Weight is its share of the amount (0 to 1).</summary>
public sealed record TrsDisbursePartyDto(long? ClientId, string Name, string Role, decimal Weight, string? GroupName);

/// <summary>One possible source of funds: own funds (no sublimit, no limit) or a bank sublimit that may fund this business line.</summary>
public sealed record TrsDisburseSourceDto(
    string SourceKey, string SourceTypeCode, long? BankId, string? BankName, long? FacilityId, string? FacilityRef, string? TrancheName,
    long? SublimitId, string? SublimitName, decimal? Available, decimal Amount, bool IsOverrideRequired, IReadOnlyList<long> RuleIds);

/// <summary>
/// A bank rule as it applies to this disbursement: the most that may come from the lines it covers (the bank's sublimits, or one
/// facility's) before it is breached (LimitAbove) or warns (WarnAbove). Null means the rule cannot be breached by this disbursement.
/// </summary>
public sealed record TrsDisburseRuleDto(
    long RuleId, long BankId, string BankName, long? FacilityId, string RuleTypeCode, string Label, string BreachActionCode, decimal? LimitAbove,
    decimal? WarnAbove, string Detail);

public sealed record TrsDisburseDto(
    TrsDisburseHeaderDto Header, IReadOnlyList<TrsDisbursePartyDto> Parties, IReadOnlyList<TrsDisburseSourceDto> Sources,
    IReadOnlyList<TrsDisburseRuleDto> Rules, bool CanEdit, string? OverrideReason, string? OverrideRequestedBy, string? OverrideApprovedBy,
    bool CanApproveOverride, string? ApproveBlockedReason);

public sealed record TrsDisburseLineFormDto(long? SublimitId, decimal Amount);

public sealed record TrsDisburseFormDto(IReadOnlyList<TrsDisburseLineFormDto> Lines, string? OverrideReason);

/// <summary>
/// Treasury Disburse (agreed with Ahmed, 5 Nov 2026): at the last step of a disbursement Treasury chooses which money pays the net
/// amount: own funds, one bank sublimit or a split. Saving reserves the room on each sublimit (RESERVE movements, split by debtor and
/// seller so bank concentration can be measured); Execute turns the reservations into drawdowns. Every active bank rule is checked
/// against the lines it covers: BLOCK refuses, OVERRIDE needs a second user's approval, WARN only warns. Our own concentration
/// (agreement limit and debtor or supplier share) was checked before this step.
/// </summary>
public sealed class TrsDisburseService(ICapitraDbContext db, ICurrentUser currentUser, IClock clock)
{
    private const decimal Tolerance = 0.001m;
    private string User => currentUser.UserName ?? currentUser.UserId ?? "SYSTEM";

    private sealed record Pair(long? DebtorClientId, long? SellerClientId, decimal Weight);

    private sealed record Context(
        FctDisbursement Disbursement, TrsDisburseHeaderDto Header, List<TrsDisbursePartyDto> Parties, List<Pair> Pairs,
        List<TrsDisburseSourceDto> Sources, List<TrsDisburseRuleDto> Rules, List<TrsDisbursementFunding> Fundings);

    // ---------------------------------------------------------------- read

    public async Task<TrsDisburseDto> GetAsync(long disbursementId, CancellationToken cancellationToken)
    {
        var c = await LoadAsync(disbursementId, cancellationToken);
        var pending = c.Fundings.Where(x => x.IsOverrideRequired).ToList();
        var requestedBy = pending.Select(x => x.ChangedBy).FirstOrDefault();
        var approvedBy = pending.Select(x => x.OverrideApprovedBy).FirstOrDefault(x => x is not null);
        var waiting = pending.Count > 0 && approvedBy is null && c.Header.FundingStatus != "EXECUTED";
        string? blocked = !waiting ? null
            : string.Equals(requestedBy, User, StringComparison.OrdinalIgnoreCase) ? "You chose these sources; another user must approve the override." : null;
        return new TrsDisburseDto(c.Header, c.Parties, c.Sources, c.Rules, c.Header.FundingStatus != "EXECUTED",
            pending.Select(x => x.OverrideReason).FirstOrDefault(x => x is not null), requestedBy, approvedBy, waiting && blocked is null, blocked);
    }

    private async Task<Context> LoadAsync(long disbursementId, CancellationToken cancellationToken)
    {
        var d = await db.Set<FctDisbursement>().AsNoTracking().SingleOrDefaultAsync(x => x.DisbursementId == disbursementId, cancellationToken)
            ?? throw new NotFoundException($"Disbursement {disbursementId} was not found.");
        var client = await db.Set<CrmClient>().AsNoTracking().Where(x => x.ClientId == d.ClientId)
            .Select(x => new { x.ClientId, x.ClientName, x.GroupName }).SingleOrDefaultAsync(cancellationToken);
        var agreement = d.AgreementId is long aid
            ? await db.Set<FctAgreement>().AsNoTracking().Where(x => x.AgreementId == aid).Select(x => new { x.AgreementNumber, x.FactoringType })
                .SingleOrDefaultAsync(cancellationToken)
            : null;
        var reverse = agreement?.FactoringType?.Contains("REVERSE", StringComparison.OrdinalIgnoreCase) == true;
        var businessLine = reverse ? "REVERSE_FACTORING" : "FACTORING";
        var currency = await db.Set<CrmLkpCurrency>().AsNoTracking().Where(x => x.CurrencyId == d.CurrencyId).Select(x => x.CurrencyCode)
            .FirstOrDefaultAsync(cancellationToken) ?? "EGP";
        var net = d.NetAmount ?? d.GrossAmount;

        // the invoices of the disbursement: their counterparty (tax number → customer) and the share of the advance each one takes
        var batchIds = db.Set<FctInvoiceBatch>().Where(x => x.DisbursementId == disbursementId).Select(x => x.BatchId);
        var invoices = await db.Set<FctInvoice>().AsNoTracking()
            .Where(x => x.DisbursementId == disbursementId || (x.BatchId != null && batchIds.Contains(x.BatchId.Value)))
            .Select(x => new { x.DebtorId, Amount = x.AdvanceAmount ?? x.FundedAmount ?? x.NetAmount ?? x.GrossAmount, x.ClientInvoiceAt, x.DueAt })
            .ToListAsync(cancellationToken);
        var taxNumbers = invoices.Where(x => x.DebtorId is not null).Select(x => x.DebtorId!.Value.ToString(CultureInfo.InvariantCulture)).Distinct().ToList();
        var counterparties = await db.Set<CrmClient>().AsNoTracking().Where(x => x.TaxNumber != null && taxNumbers.Contains(x.TaxNumber))
            .Select(x => new { x.ClientId, x.ClientName, x.GroupName, x.TaxNumber }).ToListAsync(cancellationToken);
        var byTax = counterparties.GroupBy(x => x.TaxNumber!).ToDictionary(g => g.Key, g => g.First());
        var total = invoices.Sum(x => Math.Max(0, x.Amount));
        var groups = invoices.GroupBy(x => x.DebtorId is long t && byTax.TryGetValue(t.ToString(CultureInfo.InvariantCulture), out var cp) ? cp.ClientId : (long?)null)
            .Select(g =>
            {
                var cp = counterparties.FirstOrDefault(x => x.ClientId == g.Key);
                var tax = g.Select(x => x.DebtorId).FirstOrDefault(x => x is not null);
                var name = cp?.ClientName ?? (tax is long tn ? $"Tax no. {tn}" : "Unknown counterparty");
                var w = total > 0 ? g.Sum(x => Math.Max(0, x.Amount)) / total : 1m / Math.Max(1, invoices.Count);
                return (ClientId: g.Key, Name: name, cp?.GroupName, Weight: w);
            }).ToList();
        if (groups.Count == 0) groups.Add((null, reverse ? "Supplier not known yet" : "Debtor not known yet", null, 1m));

        var clientName = client?.ClientName ?? $"Client {d.ClientId}";
        var parties = new List<TrsDisbursePartyDto>();
        List<Pair> pairs;
        if (reverse)
        {
            parties.Add(new(d.ClientId, clientName, "DEBTOR", 1m, client?.GroupName));
            parties.AddRange(groups.Select(g => new TrsDisbursePartyDto(g.ClientId, g.Name, "SELLER", g.Weight, g.GroupName)));
            pairs = [.. groups.Select(g => new Pair(d.ClientId, g.ClientId, g.Weight))];
        }
        else
        {
            parties.AddRange(groups.Select(g => new TrsDisbursePartyDto(g.ClientId, g.Name, "DEBTOR", g.Weight, g.GroupName)));
            parties.Add(new(d.ClientId, clientName, "SELLER", 1m, client?.GroupName));
            pairs = [.. groups.Select(g => new Pair(g.ClientId, d.ClientId, g.Weight))];
        }
        int? maxTenor = invoices.Where(x => x.ClientInvoiceAt is not null && x.DueAt is not null)
            .Select(x => (int?)(x.DueAt!.Value.Date - x.ClientInvoiceAt!.Value.Date).Days).Max();

        var fundings = await db.Set<TrsDisbursementFunding>().AsNoTracking()
            .Where(x => x.DisbursementId == disbursementId && (x.StatusCode == "RESERVED" || x.StatusCode == "EXECUTED"))
            .OrderBy(x => x.FundingId).ToListAsync(cancellationToken);
        var executed = fundings.Where(x => x.StatusCode == "EXECUTED").ToList();
        var status = executed.Count > 0 ? "EXECUTED" : fundings.Count > 0 ? "RESERVED" : "NONE";
        var header = new TrsDisburseHeaderDto(d.DisbursementId, d.DisbursementNumber, d.ClientId, clientName, agreement?.AgreementNumber, businessLine,
            reverse, currency, net, invoices.Count, maxTenor, status, executed.Select(x => x.ExecutedBy).FirstOrDefault(), executed.Select(x => x.ExecutedAt).FirstOrDefault());

        // what each sublimit can still take, leaving out what this disbursement holds itself
        var banks = await db.Set<TrsBank>().AsNoTracking().ToDictionaryAsync(x => x.BankId, x => x.BankNameEn, cancellationToken);
        var facilities = await db.Set<TrsCreditFacility>().AsNoTracking().Where(x => x.FacilityStatus != "CLOSED").ToListAsync(cancellationToken);
        var tranches = await db.Set<TrsFacilityTranche>().AsNoTracking().OrderBy(x => x.TrancheName).ToListAsync(cancellationToken);
        var sublimits = await db.Set<TrsFacilitySublimit>().AsNoTracking().OrderBy(x => x.SublimitName).ToListAsync(cancellationToken);
        var scopes = (await db.Set<TrsSublimitScope>().AsNoTracking().ToListAsync(cancellationToken)).ToLookup(x => x.SublimitId, x => x.ScopeValue);
        var others = db.Set<TrsSublimitMovement>().AsNoTracking().Where(x => x.DisbursementId == null || x.DisbursementId != disbursementId);
        var usage = await others.GroupBy(x => x.SublimitId)
            .Select(g => new { g.Key, Out = g.Sum(x => x.OutstandingChange), Res = g.Sum(x => x.ReservedChange) })
            .ToDictionaryAsync(x => x.Key, x => (x.Out, x.Res), cancellationToken);

        var chosen = fundings.Where(x => x.SublimitId is not null).GroupBy(x => x.SublimitId!.Value).ToDictionary(g => g.Key, g => g.ToList());
        var own = fundings.Where(x => x.SublimitId is null).ToList();
        var rules = await db.Set<TrsBankRule>().AsNoTracking().Where(x => x.StatusCode == "ACTIVE").OrderBy(x => x.RuleId).ToListAsync(cancellationToken);

        var sources = new List<TrsDisburseSourceDto>
        {
            new("OWN", "OWN_FUNDS", null, null, null, null, null, null, null, null, own.Sum(x => x.FundedAmount), own.Any(x => x.IsOverrideRequired), []),
        };
        foreach (var f in facilities.Where(x => x.FacilityStatus == "ACTIVE" && string.Equals(x.CurrencyCode, currency, StringComparison.OrdinalIgnoreCase))
                     .OrderBy(x => banks.GetValueOrDefault(x.BankId)).ThenBy(x => x.FacilityRef))
        {
            var node = TrsLimitsService.FacilityNode(f, tranches, sublimits, scopes, usage);
            var ruleIds = rules.Where(r => r.BankId == f.BankId && (r.FacilityId is null || r.FacilityId == f.FacilityId)).Select(r => r.RuleId).ToList();
            foreach (var t in node.Tranches.Where(x => x.StatusCode == "ACTIVE"))
            foreach (var s in t.Sublimits.Where(x => x.StatusCode == "ACTIVE" && (x.BusinessLines.Count == 0 || x.BusinessLines.Contains(businessLine))))
            {
                var mine = chosen.GetValueOrDefault(s.SublimitId) ?? [];
                sources.Add(new($"S{s.SublimitId}", "BANK", f.BankId, banks.GetValueOrDefault(f.BankId), f.FacilityId, f.FacilityRef, t.TrancheName,
                    s.SublimitId, s.SublimitName, s.Available, mine.Sum(x => x.FundedAmount), mine.Any(x => x.IsOverrideRequired), ruleIds));
            }
        }

        var bankIds = sources.Where(x => x.BankId is not null).Select(x => x.BankId!.Value).ToHashSet();
        var ruleDtos = await RulesAsync([.. rules.Where(r => bankIds.Contains(r.BankId))], banks, facilities, sublimits, parties, maxTenor, disbursementId,
            cancellationToken);
        return new Context(d, header, parties, pairs, sources, ruleDtos, fundings);
    }

    private async Task<List<TrsDisburseRuleDto>> RulesAsync(
        List<TrsBankRule> rules, Dictionary<long, string> banks, List<TrsCreditFacility> facilities, List<TrsFacilitySublimit> sublimits,
        List<TrsDisbursePartyDto> parties, int? maxTenor, long disbursementId, CancellationToken cancellationToken)
    {
        if (rules.Count == 0) return [];
        var subFacility = sublimits.ToDictionary(x => x.SublimitId, x => x.FacilityId);
        var rows = await db.Set<TrsSublimitMovement>().AsNoTracking().Where(x => x.DisbursementId == null || x.DisbursementId != disbursementId)
            .GroupBy(x => new { x.SublimitId, x.DebtorClientId, x.SellerClientId })
            .Select(g => new { g.Key.SublimitId, g.Key.DebtorClientId, g.Key.SellerClientId, Amount = g.Sum(x => x.OutstandingChange + x.ReservedChange) })
            .ToListAsync(cancellationToken);
        var debtorIds = rows.Where(x => x.DebtorClientId is not null).Select(x => x.DebtorClientId!.Value).ToHashSet();
        var groupOf = await db.Set<CrmClient>().AsNoTracking().Where(x => debtorIds.Contains(x.ClientId) && x.GroupName != null)
            .ToDictionaryAsync(x => x.ClientId, x => x.GroupName!, cancellationToken);
        var subjectIds = rules.Where(x => x.SubjectClientId is not null).Select(x => x.SubjectClientId!.Value).ToHashSet();
        var subjectNames = await db.Set<CrmClient>().AsNoTracking().Where(x => subjectIds.Contains(x.ClientId))
            .ToDictionaryAsync(x => x.ClientId, x => x.ClientName, cancellationToken);

        var result = new List<TrsDisburseRuleDto>();
        foreach (var r in rules)
        {
            var bankName = banks.GetValueOrDefault(r.BankId) ?? $"Bank {r.BankId}";
            var inScope = facilities.Where(f => f.BankId == r.BankId && (r.FacilityId is null || f.FacilityId == r.FacilityId)).Select(f => f.FacilityId).ToHashSet();
            var scoped = rows.Where(x => subFacility.TryGetValue(x.SublimitId, out var fid) && inScope.Contains(fid)).ToList();
            var role = r.SubjectTypeCode == "SELLER" ? "SELLER" : "DEBTOR";
            string Who() => r.SubjectTypeCode switch
            {
                "DEBTOR_GROUP" => r.SubjectValue is null ? "any debtor group" : $"debtor group {r.SubjectValue}",
                _ => r.SubjectClientId is long id ? subjectNames.GetValueOrDefault(id) ?? $"client {id}" : role == "SELLER" ? "any seller" : "any debtor",
            };

            // the parties this rule looks at, with this disbursement's share (weight) and what they already hold of the bank's money
            var subjects = r.SubjectTypeCode == "DEBTOR_GROUP"
                ? parties.Where(p => p.Role == "DEBTOR" && p.GroupName is not null).GroupBy(p => p.GroupName!)
                    .Select(g => (Key: g.Key, Name: g.Key, Weight: g.Sum(p => p.Weight),
                        Held: scoped.Where(x => x.DebtorClientId is long id && groupOf.GetValueOrDefault(id) == g.Key).Sum(x => x.Amount))).ToList()
                : parties.Where(p => p.Role == role && p.ClientId is not null).GroupBy(p => p.ClientId!.Value)
                    .Select(g => (Key: g.Key.ToString(CultureInfo.InvariantCulture), Name: g.First().Name, Weight: g.Sum(p => p.Weight),
                        Held: scoped.Where(x => (role == "SELLER" ? x.SellerClientId : x.DebtorClientId) == g.Key).Sum(x => x.Amount))).ToList();
            if (r.SubjectClientId is long sid) subjects = [.. subjects.Where(x => x.Key == sid.ToString(CultureInfo.InvariantCulture))];
            if (r.SubjectTypeCode == "DEBTOR_GROUP" && r.SubjectValue is string sv) subjects = [.. subjects.Where(x => x.Key == sv)];

            string label;
            decimal? limit = null, warn = null;
            string detail;
            switch (r.RuleTypeCode)
            {
                case "EXCLUDE":
                    label = $"Exclude {Who()}";
                    var hit = r.SubjectClientId is null && r.SubjectValue is null ? [] : subjects;
                    if (hit.Count > 0) { limit = 0; detail = $"{hit[0].Name} may not be financed with {bankName}'s money."; }
                    else detail = "No excluded party in this disbursement.";
                    break;
                case "MAX_TENOR":
                    label = $"Invoice tenor at most {r.MaxTenorDays} days";
                    if (maxTenor is int t && r.MaxTenorDays is int m && t > m) { limit = 0; detail = $"An invoice runs {t} days, above {m}."; }
                    else detail = maxTenor is int t2 ? $"Longest invoice: {t2} days." : "No invoice dates to check.";
                    break;
                default:
                    var pool = scoped.Sum(x => x.Amount);
                    var facilityLimits = facilities.Where(f => inScope.Contains(f.FacilityId)).Sum(f => f.ApprovedLimit);
                    var denominator = r.BasisCode == "FACILITY_LIMIT" ? facilityLimits : pool;
                    label = r.LimitPct is decimal lp
                        ? $"{Cap(Who())} at most {lp:0.##}% of the {(r.BasisCode == "FACILITY_LIMIT" ? "facility limit" : "funded pool")}"
                        : $"{Cap(Who())} at most {r.LimitAmount:N0}";
                    (decimal Cap, string Name, decimal Held)? tight = null;
                    foreach (var s in subjects)
                    {
                        var c = Min(r.LimitPct is decimal p ? PctCap(p / 100, r.BasisCode, s.Weight, s.Held, pool, facilityLimits) : null,
                            r.LimitAmount is decimal a && s.Weight > 0 ? (a - s.Held) / s.Weight : null);
                        if (c is decimal cv && (tight is null || cv < tight.Value.Cap)) tight = (cv, s.Name, s.Held);
                        if (r.WarningPct is decimal wp && PctCap(wp / 100, r.BasisCode, s.Weight, s.Held, pool, facilityLimits) is decimal wc)
                            warn = Min(warn, Whole(wc));
                    }
                    if (tight is { } tt)
                    {
                        limit = Whole(tt.Cap);
                        var pct = denominator > 0 ? $" ({tt.Held / denominator * 100:0.0}%)" : "";
                        detail = $"{tt.Name} holds {tt.Held:N0}{pct} of {bankName}'s money now.";
                    }
                    else detail = subjects.Count == 0 ? "No party of this disbursement is covered." : "This disbursement cannot breach it.";
                    break;
            }
            result.Add(new TrsDisburseRuleDto(r.RuleId, r.BankId, bankName, r.FacilityId, r.RuleTypeCode, label, r.BreachActionCode, limit, warn, detail));
        }
        return result;
    }

    /// <summary>
    /// The most x this disbursement may draw before party p (weight w, holding e) passes L of the basis. On the funded pool P the pool
    /// grows with x: e + w·x ≤ L·(P + x), so x ≤ (L·P − e)/(w − L), and there is no cap when w ≤ L. On the facility limit F: x ≤ (L·F − e)/w.
    /// </summary>
    public static decimal? PctCap(decimal limit, string basis, decimal weight, decimal held, decimal pool, decimal facilityLimits)
    {
        if (weight <= 0) return null;
        if (basis == "FACILITY_LIMIT") return (limit * facilityLimits - held) / weight;
        return weight <= limit ? null : (limit * pool - held) / (weight - limit);
    }

    private static decimal? Min(decimal? a, decimal? b) => a is null ? b : b is null ? a : Math.Min(a.Value, b.Value);

    private static decimal Whole(decimal x) => Math.Max(0, Math.Floor(x));

    private static string Cap(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];

    // ---------------------------------------------------------------- save (reserve)

    public async Task<TrsDisburseDto> SaveAsync(long disbursementId, TrsDisburseFormDto dto, CancellationToken cancellationToken)
    {
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var current = await db.Set<TrsDisbursementFunding>()
            .Where(x => x.DisbursementId == disbursementId && (x.StatusCode == "RESERVED" || x.StatusCode == "EXECUTED")).ToListAsync(cancellationToken);
        if (current.Any(x => x.StatusCode == "EXECUTED")) throw new BusinessRuleException("The source of funds is already executed and cannot be changed.");
        await ReleaseAsync(current, cancellationToken);

        var c = await LoadAsync(disbursementId, cancellationToken);
        var lines = (dto.Lines ?? []).GroupBy(x => x.SublimitId).Select(g => (SublimitId: g.Key, Amount: Math.Round(g.Sum(x => x.Amount), 3))).ToList();
        if (lines.Any(x => x.Amount < 0)) throw new BusinessRuleException("An amount cannot be negative.");
        lines = [.. lines.Where(x => x.Amount > 0)];
        var total = lines.Sum(x => x.Amount);
        if (total > c.Header.NetAmount + Tolerance)
            throw new BusinessRuleException($"The sources add up to {total:N2}, above the net amount of {c.Header.NetAmount:N2}.");

        var picked = new List<(TrsDisburseSourceDto Source, decimal Amount)>();
        foreach (var (sublimitId, amount) in lines)
        {
            var s = c.Sources.FirstOrDefault(x => x.SublimitId == sublimitId)
                ?? throw new BusinessRuleException("A chosen sublimit cannot fund this disbursement (another business line, currency or status).");
            if (s.Available is decimal av && amount > av + Tolerance)
                throw new BusinessRuleException($"{Path(s)}: at most {av:N2} is available.");
            picked.Add((s, amount));
        }

        var needsOverride = new HashSet<string>();
        foreach (var r in c.Rules.Where(x => x.LimitAbove is not null))
        {
            var covered = picked.Where(x => x.Source.RuleIds.Contains(r.RuleId)).ToList();
            var sum = covered.Sum(x => x.Amount);
            if (sum <= r.LimitAbove!.Value + Tolerance) continue;
            if (r.BreachActionCode == "BLOCK")
                throw new BusinessRuleException($"{r.BankName}: {r.Label}. At most {r.LimitAbove:N0} can come from this bank for this disbursement. {r.Detail}");
            if (r.BreachActionCode == "OVERRIDE") foreach (var x in covered) needsOverride.Add(x.Source.SourceKey);
        }
        var reason = string.IsNullOrWhiteSpace(dto.OverrideReason) ? null : dto.OverrideReason.Trim();
        if (needsOverride.Count > 0 && reason is null)
            throw BusinessValidationException.For("overrideReason", "A bank rule needs an override. Enter the reason, then a second user approves it.");

        var now = clock.UtcNow;
        var fundings = picked.Select(p => new TrsDisbursementFunding
        {
            DisbursementId = disbursementId, SourceTypeCode = p.Source.SourceTypeCode, SublimitId = p.Source.SublimitId, FacilityId = p.Source.FacilityId,
            BankId = p.Source.BankId, FundedAmount = p.Amount, StatusCode = "RESERVED", IsOverrideRequired = needsOverride.Contains(p.Source.SourceKey),
            OverrideReason = needsOverride.Contains(p.Source.SourceKey) ? reason : null, ChangedBy = User, ChangedAt = now,
        }).ToList();
        db.Set<TrsDisbursementFunding>().AddRange(fundings);
        await db.SaveChangesAsync(cancellationToken);

        var facilityIds = fundings.Where(x => x.FacilityId is not null).Select(x => x.FacilityId!.Value).ToHashSet();
        var facilities = await db.Set<TrsCreditFacility>().Where(x => facilityIds.Contains(x.FacilityId)).ToDictionaryAsync(x => x.FacilityId, cancellationToken);
        var today = DateOnly.FromDateTime(now);
        foreach (var f in fundings.Where(x => x.SublimitId is not null))
        {
            foreach (var (pair, share) in Split(c.Pairs, f.FundedAmount))
                db.Set<TrsSublimitMovement>().Add(new TrsSublimitMovement
                {
                    SublimitId = f.SublimitId!.Value, MovementTypeCode = "RESERVE", MovementDate = today, ReservedChange = share, DisbursementId = disbursementId,
                    FundingId = f.FundingId, DebtorClientId = pair.DebtorClientId, SellerClientId = pair.SellerClientId,
                    Notes = $"Treasury Disburse {c.Header.DisbursementNumber}",
                });
            var fac = facilities[f.FacilityId!.Value];
            fac.ReservedBalance += f.FundedAmount;
            fac.AvailableBalance = Math.Max(0, fac.ApprovedLimit - fac.OutstandingBalance - fac.ReservedBalance);
        }
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return await GetAsync(disbursementId, cancellationToken);
    }

    /// <summary>Gives back the room the disbursement held: a RELEASE movement for every reservation, and the facility's reserved balance.</summary>
    private async Task ReleaseAsync(List<TrsDisbursementFunding> fundings, CancellationToken cancellationToken)
    {
        var reserved = fundings.Where(x => x.StatusCode == "RESERVED").ToList();
        if (reserved.Count == 0) return;
        var ids = reserved.Select(x => x.FundingId).ToList();
        var held = await db.Set<TrsSublimitMovement>().AsNoTracking().Where(x => x.FundingId != null && ids.Contains(x.FundingId.Value))
            .GroupBy(x => new { x.FundingId, x.SublimitId, x.DebtorClientId, x.SellerClientId, x.DisbursementId })
            .Select(g => new { g.Key.FundingId, g.Key.SublimitId, g.Key.DebtorClientId, g.Key.SellerClientId, g.Key.DisbursementId, Res = g.Sum(x => x.ReservedChange) })
            .ToListAsync(cancellationToken);
        var facilityIds = reserved.Where(x => x.FacilityId is not null).Select(x => x.FacilityId!.Value).ToHashSet();
        var facilities = await db.Set<TrsCreditFacility>().Where(x => facilityIds.Contains(x.FacilityId)).ToDictionaryAsync(x => x.FacilityId, cancellationToken);
        var today = DateOnly.FromDateTime(clock.UtcNow);
        foreach (var h in held.Where(x => x.Res != 0))
            db.Set<TrsSublimitMovement>().Add(new TrsSublimitMovement
            {
                SublimitId = h.SublimitId, MovementTypeCode = "RELEASE", MovementDate = today, ReservedChange = -h.Res, DisbursementId = h.DisbursementId,
                FundingId = h.FundingId, DebtorClientId = h.DebtorClientId, SellerClientId = h.SellerClientId, Notes = "Source of funds changed",
            });
        foreach (var f in reserved)
        {
            f.StatusCode = "RELEASED";
            f.ChangedBy = User;
            f.ChangedAt = clock.UtcNow;
            if (f.FacilityId is long fid && facilities.TryGetValue(fid, out var fac))
            {
                fac.ReservedBalance = Math.Max(0, fac.ReservedBalance - f.FundedAmount);
                fac.AvailableBalance = Math.Max(0, fac.ApprovedLimit - fac.OutstandingBalance - fac.ReservedBalance);
            }
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Splits an amount over the debtor and seller pairs by weight; the last pair takes the rounding difference.</summary>
    private static IEnumerable<(Pair Pair, decimal Share)> Split(List<Pair> pairs, decimal amount)
    {
        var weight = pairs.Sum(x => x.Weight);
        var left = amount;
        for (var i = 0; i < pairs.Count; i++)
        {
            var share = i == pairs.Count - 1 ? left : Math.Round(amount * (weight > 0 ? pairs[i].Weight / weight : 1m / pairs.Count), 3);
            left -= share;
            if (share != 0) yield return (pairs[i], share);
        }
    }

    private static string Path(TrsDisburseSourceDto s) => s.SublimitId is null ? "Own funds" : $"{s.BankName} › {s.TrancheName} › {s.SublimitName}";

    // ---------------------------------------------------------------- override and execute

    public async Task<TrsDisburseDto> ApproveOverrideAsync(long disbursementId, CancellationToken cancellationToken)
    {
        var pending = await db.Set<TrsDisbursementFunding>()
            .Where(x => x.DisbursementId == disbursementId && x.StatusCode == "RESERVED" && x.IsOverrideRequired && x.OverrideApprovedBy == null)
            .ToListAsync(cancellationToken);
        if (pending.Count == 0) throw new BusinessRuleException("No override is waiting for approval.");
        if (pending.Any(x => string.Equals(x.ChangedBy, User, StringComparison.OrdinalIgnoreCase)))
            throw new BusinessRuleException("You chose these sources; another user must approve the override.");
        foreach (var f in pending)
        {
            f.OverrideApprovedBy = User;
            f.OverrideApprovedAt = clock.UtcNow;
        }
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(disbursementId, cancellationToken);
    }

    public async Task<TrsDisburseDto> ExecuteAsync(long disbursementId, CancellationToken cancellationToken)
    {
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var c = await LoadAsync(disbursementId, cancellationToken);
        if (c.Header.FundingStatus == "EXECUTED") throw new BusinessRuleException("The source of funds is already executed.");
        var fundings = await db.Set<TrsDisbursementFunding>().Where(x => x.DisbursementId == disbursementId && x.StatusCode == "RESERVED")
            .ToListAsync(cancellationToken);
        if (fundings.Count == 0) throw new BusinessRuleException("Choose and save the source of funds first.");
        var total = fundings.Sum(x => x.FundedAmount);
        if (Math.Abs(total - c.Header.NetAmount) > Tolerance)
            throw new BusinessRuleException($"The sources add up to {total:N2}; they must equal the net amount of {c.Header.NetAmount:N2}.");
        if (fundings.Any(x => x.IsOverrideRequired && x.OverrideApprovedBy is null))
            throw new BusinessRuleException("A bank rule override is waiting for a second user's approval.");

        var ids = fundings.Select(x => x.FundingId).ToList();
        var held = await db.Set<TrsSublimitMovement>().AsNoTracking().Where(x => x.FundingId != null && ids.Contains(x.FundingId.Value))
            .GroupBy(x => new { x.FundingId, x.SublimitId, x.DebtorClientId, x.SellerClientId })
            .Select(g => new { g.Key.FundingId, g.Key.SublimitId, g.Key.DebtorClientId, g.Key.SellerClientId, Res = g.Sum(x => x.ReservedChange) })
            .ToListAsync(cancellationToken);
        var facilityIds = fundings.Where(x => x.FacilityId is not null).Select(x => x.FacilityId!.Value).ToHashSet();
        var facilities = await db.Set<TrsCreditFacility>().Where(x => facilityIds.Contains(x.FacilityId)).ToDictionaryAsync(x => x.FacilityId, cancellationToken);
        var now = clock.UtcNow;
        var today = DateOnly.FromDateTime(now);
        foreach (var h in held.Where(x => x.Res != 0))
            db.Set<TrsSublimitMovement>().Add(new TrsSublimitMovement
            {
                SublimitId = h.SublimitId, MovementTypeCode = "DRAWDOWN", MovementDate = today, ReservedChange = -h.Res, OutstandingChange = h.Res,
                DisbursementId = disbursementId, FundingId = h.FundingId, DebtorClientId = h.DebtorClientId, SellerClientId = h.SellerClientId,
                Notes = $"Treasury Disburse {c.Header.DisbursementNumber}",
            });
        foreach (var f in fundings)
        {
            f.StatusCode = "EXECUTED";
            f.ExecutedBy = User;
            f.ExecutedAt = now;
            if (f.FacilityId is long fid && facilities.TryGetValue(fid, out var fac))
            {
                fac.ReservedBalance = Math.Max(0, fac.ReservedBalance - f.FundedAmount);
                fac.OutstandingBalance += f.FundedAmount;
                fac.AvailableBalance = Math.Max(0, fac.ApprovedLimit - fac.OutstandingBalance - fac.ReservedBalance);
            }
        }
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return await GetAsync(disbursementId, cancellationToken);
    }

    /// <summary>True when the disbursement's source of funds is executed (the Treasury Disburse step may be completed).</summary>
    public Task<bool> IsExecutedAsync(long disbursementId, CancellationToken cancellationToken) =>
        db.Set<TrsDisbursementFunding>().AnyAsync(x => x.DisbursementId == disbursementId && x.StatusCode == "EXECUTED", cancellationToken);
}
