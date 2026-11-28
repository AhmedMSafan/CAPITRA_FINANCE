using Capitra.Application.Abstractions;
using Capitra.Application.Common;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Gl;
using Capitra.Domain.Modules.Lnd;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Lnd;

/// <summary>One charge of a rule: how it is worked out, its default timing and income, and what it is on this finance.</summary>
public sealed record LndCfRuleChargeDto(string ChargeCode, string ChargeName, string Calculation, string DefaultTiming, string Income, string? OnThisFinance);

/// <summary>One GL line of a rule: journal, debit or credit, account, amount code and the amount it gives on this finance (null before there is one).</summary>
public sealed record LndCfRuleLineDto(int JournalNo, string DebitCredit, string Account, string AmountCode, string? ChargeCode, decimal? Amount);

/// <summary>A rule a button of the page posts: how it is linked, its journal category, charges and GL lines; Problem when it cannot be read.</summary>
public sealed record LndCfRuleUseDto(string Button, string RuleCode, string? RuleName, string LinkedBy, string? CategoryCode,
    IReadOnlyList<LndCfRuleChargeDto> Charges, IReadOnlyList<LndCfRuleLineDto> Lines, string? Problem, string? NotApplied = null);

/// <summary>A page of the consumer finance (New Finance or a workflow step): its role and the rules its buttons post.</summary>
public sealed record LndCfRulePageDto(string StepCode, string StepName, string? Role, string Note, IReadOnlyList<LndCfRuleUseDto> Rules);

public sealed record LndCfRulesDto(string? ProductCode, string? ProductName, IReadOnlyList<LndCfRulePageDto> Pages);

/// <summary>
/// "Rules and GL" (Ahmed, 27 Nov 2026), read only: for New Finance and each CF_FINANCE step, the role that does it, the product rules its
/// buttons post (the step's rule named in the workflow, or a product action's linked rule), the rule's charges and GL lines, and the
/// amounts they give on this finance. Read the same way the posting reads them, so it shows what will post.
/// </summary>
public sealed class LndCfRulesService(ICapitraDbContext db, AdmProductEventRuleService rules, ProductActionPosting actions, IClock clock,
    LndCfChargesPlanner planner)
{
    public const string NewFinance = "CF_NEW";

    private sealed record Use(string Button, string? StepRule, string? Action);

    private static readonly (string Step, string Note, Use[] Uses)[] Pages =
    [
        (NewFinance, "Posts nothing: the main data only. The finance's rules post at the steps below.", []),
        (LndCfFinanceRules.Credit, "Credit decides each charge of the rule below (upfront or added to the schedule) and its amount. Approving books the claim once: the upfront charges post here; those added to the schedule post as part of the finance at Delivery.",
            [new("Approve (creates the claim)", LndCfFinanceRules.Credit, null)]),
        (LndCfFinanceRules.Charges, "Executes Credit's decision: only the upfront charges post here. The claim is paid in Collection.",
            [new("Create claim", LndCfFinanceRules.Charges, null), new("Pay Claim (Collection › Customer Payments)", null, ProductActions.CfClaimPayment)]),
        (LndCfFinanceRules.Schedule, "Posts nothing: builds the schedule. Accept waits until the claim is paid.", []),
        (LndCfFinanceRules.Delivery, "Books the finance.", [new("Accept and book", LndCfFinanceRules.Delivery, null)]),
        (LndCfFinanceRules.SupplierPayment, "Pays the supplier; the workflow ends.", [new("Pay the supplier", LndCfFinanceRules.SupplierPayment, null)]),
    ];

    /// <summary>The page of a step (or of New Finance), or all of them with step null; for a finance (its rules and amounts) or for New Finance.</summary>
    /// <remarks>
    /// Credit's choices still on the screen (not saved until Approve) can be given: each charge's timing and amount, and the down
    /// payment; the page then shows what Approve would post (a charge added to the schedule posts nothing here, at Delivery instead).
    /// </remarks>
    public async Task<LndCfRulesDto> GetAsync(long? loanId, string? step, CancellationToken ct, IReadOnlyDictionary<string, string>? timings = null,
        IReadOnlyDictionary<string, decimal>? amounts = null, decimal? down = null)
    {
        var today = LndLoanService.CairoToday(clock);
        LndLoan? loan = null;
        if (loanId is { } id)
            loan = await db.Set<LndLoan>().AsNoTracking().FirstOrDefaultAsync(x => x.LoanId == id, ct) ?? throw new NotFoundException($"Finance {id} was not found.");
        var product = loan is not null
            ? await db.Set<AdmProduct>().AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == loan.ProductId, ct)
            : await db.Set<AdmProduct>().AsNoTracking().Where(x => x.IsActive && x.BusinessLineCode == LndProductTerms.BusinessLines.ConsumerFinance)
                .OrderBy(x => x.ProductNameEn).FirstOrDefaultAsync(ct);
        // the workflow the finance runs on, else the one new finances start on
        var xml = loan?.WorkflowInstanceId is { } run ? await WfeVersions.XmlOfInstanceAsync(db, run, ct) : null;
        xml ??= await db.Set<WfeWorkflowDefinition>().AsNoTracking().Where(x => x.WorkflowCode == LndCfFinanceRules.WorkflowCode).Select(x => x.BpmnXml).FirstOrDefaultAsync(ct);

        List<LndLoanCharge> saved = loan is null ? [] : await db.Set<LndLoanCharge>().AsNoTracking().Where(x => x.LoanId == loan.LoanId).ToListAsync(ct);
        // before Credit approves, nothing is saved: the charges as Credit has them on the screen, else as the rule works them out now
        var onScreen = timings is not null || amounts is not null || down is not null;
        if (loan is not null && (saved.Count == 0 || onScreen))
        {
            var work = await planner.WorkAsync(loan, timings, ct, down, amounts);
            if (work.Problem is null)
                saved = [.. work.Charges.Select(c => new LndLoanCharge
                {
                    LoanId = loan.LoanId, ChargeCode = c.ChargeCode, ChargeName = c.ChargeName, ChargeAmount = c.Amount, TaxAmount = c.TaxAmount, TimingCode = c.TimingCode,
                })];
        }
        var lineAmounts = loan is null ? null : Amounts(loan, saved);
        var pages = new List<LndCfRulePageDto>();
        // finances that started before 27 Nov 2026 create the claim at Charges and claim; new ones when Credit approves
        var hasCharges = xml is not null && BpmnModel.StepByCode(xml, LndCfFinanceRules.Charges) is not null;
        foreach (var (code, baseNote, baseUses) in Pages.Where(p => step is null || string.Equals(p.Step, step, StringComparison.OrdinalIgnoreCase)))
        {
            if (code == LndCfFinanceRules.Charges && !hasCharges) continue;
            var (note, uses) = code == LndCfFinanceRules.Credit && hasCharges
                ? ("Posts nothing: Credit decides each charge of the rule below (upfront or added to the schedule) and its amount; it posts at Charges and claim, or as part of the finance at Delivery.",
                   new[] { new Use("— (decided here)", LndCfFinanceRules.Charges, null) })
                : (baseNote, baseUses);
            var element = xml is null || code == NewFinance ? null : BpmnModel.StepByCode(xml, code);
            var role = code == NewFinance ? "SALES_OFFICER" : element?.Assignee;
            var list = new List<LndCfRuleUseDto>();
            foreach (var u in uses)
                list.Add(await UseAsync(u, product, xml, today, saved, lineAmounts, ct));
            pages.Add(new LndCfRulePageDto(code, code == NewFinance ? "New Finance" : LndCfFinanceRules.StepName(code), role, note, list));
        }
        return new LndCfRulesDto(product?.ProductCode, product?.ProductNameEn, pages);
    }

    private async Task<LndCfRuleUseDto> UseAsync(Use u, AdmProduct? product, string? xml, DateOnly today, List<LndLoanCharge> saved,
        Func<string, string?, decimal?>? amounts, CancellationToken ct)
    {
        ProductEventPlan? plan = null;
        string linkedBy, ruleCode;
        string? problem = null;
        if (u.Action is { } action)
        {
            linkedBy = $"Product action {action}";
            ruleCode = action;
            try { plan = (await actions.PlanAsync(action, today, ct)).Plan; }
            catch (Exception ex) when (ex is not OperationCanceledException) { problem = ex.Message; }
        }
        else
        {
            var named = xml is null ? null : BpmnModel.StepByCode(xml, u.StepRule!)?.RuleCode?.Trim();
            ruleCode = string.IsNullOrWhiteSpace(named) ? (u.StepRule is LndCfFinanceRules.Charges or LndCfFinanceRules.Credit ? LndCfCharges.RuleCode : u.StepRule!) : named;
            linkedBy = string.IsNullOrWhiteSpace(named) ? "Workflow step (default rule of the step's code)" : "Workflow step (finova:ruleCode)";
            if (product is null) problem = "No consumer finance product is active.";
            else
            {
                plan = await rules.PlanByCodeAsync(product.ProductCode, ruleCode, today, ct);
                if (plan is null) problem = $"Product {product.ProductCode} has no active rule {ruleCode} today.";
            }
        }
        if (plan is null) return new LndCfRuleUseDto(u.Button, ruleCode, null, linkedBy, null, [], [], problem);

        var ruleName = await db.Set<AdmProductEventRule>().AsNoTracking().Where(x => x.ProductEventRuleId == plan.ProductEventRuleId).Select(x => x.RuleNameEn).FirstOrDefaultAsync(ct);
        var codes = plan.Lines.Where(l => l.AccountCode != null).Select(l => l.AccountCode!).Distinct().ToList();
        var names = await db.Set<GlAccount>().AsNoTracking().Where(a => codes.Contains(a.AccountCode)).GroupBy(a => a.AccountCode)
            .Select(g => new { Code = g.Key, Name = g.Min(a => a.AccountNameEn) }).ToDictionaryAsync(x => x.Code, x => x.Name, ct);
        var charges = plan.Charges.Select(c =>
        {
            var s = saved.FirstOrDefault(x => x.ChargeCode == c.ChargeCode);
            return new LndCfRuleChargeDto(c.ChargeCode, c.ChargeNameEn, LndCfCharges.Basis(c), Timing(LndCfCharges.DefaultTiming(c)), Income(c.IncomeRecognitionCode),
                s is null ? null : $"{s.ChargeAmount + s.TaxAmount:#,0.00} · {Timing(s.TimingCode)}{(s.IsOverridden ? " (changed by Credit)" : "")}"
                    + (s.TimingCode == LndCfCharges.Financed ? " · posts at Delivery as part of the finance" : ""));
        }).ToList();
        var lines = plan.Lines.OrderBy(l => l.JournalNo).ThenBy(l => l.LineNo).Select(l => new LndCfRuleLineDto(l.JournalNo, l.DebitCreditCode,
            l.FromBankAccount ? "Bank account chosen" : l.AccountCode is { } a ? $"{a} {names.GetValueOrDefault(a)}".Trim() : "—",
            l.AmountCode, l.ChargeCode, amounts?.Invoke(l.AmountCode, l.ChargeCode))).ToList();
        // on a finance, a rule whose lines all give 0 posts nothing here: say why (Ahmed, 27 Nov 2026)
        string? notApplied = null;
        if (amounts is not null && lines.Count > 0 && lines.All(l => (l.Amount ?? 0) == 0))
            notApplied = plan.Charges.Count > 0 && saved.Count > 0 && saved.All(x => x.TimingCode == LndCfCharges.Financed)
                ? "All charges are added to the schedule: they post at Delivery as part of the finance."
                : plan.Charges.Count > 0 && saved.Count > 0 && saved.All(x => x.ChargeAmount + x.TaxAmount == 0)
                    ? "The charges are waived (0): nothing posts."
                    : "Nothing to post on this finance yet.";
        return new LndCfRuleUseDto(u.Button, plan.RuleCode, ruleName, linkedBy, plan.CategoryCode, charges, lines, null, notApplied);
    }

    /// <summary>The amount each amount code gives on this finance, as the steps post them (a charge line: that charge, when it is upfront).</summary>
    private static Func<string, string?, decimal?> Amounts(LndLoan l, List<LndLoanCharge> saved)
    {
        var (charges, down) = LndCfFinanceRules.ClaimAmounts(saved, l.DownPaymentCollectorCode, l.DownPaymentAmount);
        var booking = LndCfFinanceRules.BookingAmounts(l);
        var due = LndCfFinanceRules.SupplierDue(l);
        var commission = l.SupplierCommissionAmount ?? due.CommissionAmount;
        return (code, chargeCode) =>
        {
            if (chargeCode is not null)
                return saved.Where(x => x.ChargeCode == chargeCode && x.TimingCode == LndCfCharges.Upfront).Sum(x => x.ChargeAmount + x.TaxAmount);
            return code switch
            {
                "PAYMENT_AMOUNT" => charges + down,
                "CHARGES_PAID_AMOUNT" => charges,
                "DOWN_PAYMENT_AMOUNT" => down,
                "PAYABLE_AMOUNT" => due.PayableAmount,
                "COMMISSION_AMOUNT" => commission,
                "NET_PAYMENT_AMOUNT" => due.PayableAmount - commission,
                _ => booking.GetValueOrDefault(code),
            };
        };
    }

    private static string Timing(string code) => code == LndCfCharges.Financed ? "Added to schedule" : "Upfront";

    private static string Income(string code) => code switch
    {
        "IMMEDIATE" => "Immediate",
        "PRORATE" => "Spread over the schedule",
        _ => code,
    };
}
