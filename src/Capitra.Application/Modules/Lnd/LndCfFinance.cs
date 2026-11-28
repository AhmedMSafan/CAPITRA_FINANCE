using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Crm;
using Capitra.Application.Modules.Gl;
using Capitra.Application.Modules.Gl.Journals;
using Capitra.Application.Modules.Scr;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Lnd;
using Capitra.Domain.Modules.Trs;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Lnd;

/// <summary>
/// CF_FINANCE (Ahmed, 26-27 Nov 2026): a consumer finance bought from a supplier. 1 Application: Sales enters the main data on New
/// Finance (creating it submits it). 2 Credit approval (CREDIT_OFFICER): approves every value and decides each charge, upfront (the
/// rule's default) or added to the schedule; approving creates the upfront claim once (the rule the step names, CF_CHARGES: claim and
/// GL), paid like a factoring claim. 3 Schedule and contract (OPERATIONS_OFFICER): once the claim is paid, the schedule on the financed
/// amount plus the charges added to it. 4 Delivery and customer receipt (OPERATIONS_OFFICER). 5 Supplier payment (TREASURY_OFFICER).
/// Finances that started before 27 Nov 2026 keep the step Charges and claim (OPERATIONS_OFFICER) between 2 and 3, where their claim is
/// created. Anyone holding a step's role does it (Ahmed, 28 Nov 2026).
/// </summary>
public static class LndCfFinanceRules
{
    public const string WorkflowCode = "CF_FINANCE";

    public const string Credit = "CF_CREDIT";
    public const string Charges = "CF_CHARGES";
    public const string Schedule = "CF_SCHEDULE";
    public const string Delivery = "CF_DELIVERY";
    public const string SupplierPayment = "CF_SUPPLIER_PAYMENT";

    /// <summary>The step of the first version, replaced by Charges and claim and Schedule and contract.</summary>
    public const string Agreement = "CF_AGREEMENT";

    public static readonly string[] Steps = [Credit, Charges, Schedule, Delivery, SupplierPayment, Agreement];

    /// <summary>The steps built so far; the others are refused with a clear message until they come.</summary>
    public static readonly string[] Ready = [Credit, Charges, Schedule, Delivery, SupplierPayment];

    public static class Claim
    {
        /// <summary>Unpaid (the factoring claim's code).</summary>
        public const string Open = "OPEN";
        public const string PartiallyPaid = "PARTIALLY_PAID";
        public const string Paid = "PAID";
        /// <summary>The finance was rejected before anything was paid; the claim's GL is reversed.</summary>
        public const string Cancelled = "CANCELLED";
        /// <summary>Nothing is paid upfront: no claim.</summary>
        public const string None = "NONE";
    }

    public static readonly string[] PaymentMethods = ["CASH", "BANK_TRANSFER", "PAYMENT_NETWORK", "WALLET"];

    public static string StepName(string? stepCode) => (stepCode ?? "").ToUpperInvariant() switch
    {
        Credit => "Credit approval",
        Charges => "Charges and claim",
        Schedule => "Schedule and contract",
        Delivery => "Delivery and customer receipt",
        SupplierPayment => "Supplier payment",
        Agreement => "Agreement and upfront payment",
        _ => stepCode ?? "the step",
    };

    /// <summary>
    /// Segregation of duties (the first rule, 26 Nov 2026). Not applied since 28 Nov 2026 (Ahmed): anyone holding a step's role does it,
    /// as the inbox offers it, the person who entered the finance too. Kept for a later product setting.
    /// </summary>
    public static void CheckPerson(string? enteredByPerson, string actorPerson)
    {
        if (enteredByPerson is not null && string.Equals(enteredByPerson, actorPerson, StringComparison.Ordinal))
            throw new BusinessRuleException("Segregation of duties: the person who entered the finance cannot do its workflow steps.");
    }

    /// <summary>
    /// Credit approval: the customer's compliance is clear (CLEAR, or DUE for review soon) and the supplier is still active and valid today.
    /// </summary>
    public static List<string> CheckCredit(string complianceCode, string? complianceLabel, bool supplierActive)
    {
        var p = new List<string>();
        if (complianceCode is not ("CLEAR" or "DUE"))
            p.Add($"The customer's compliance must be clear (now {(complianceLabel ?? complianceCode).ToLowerInvariant()}, Customer 360 > Compliance).");
        if (!supplierActive) p.Add("The supplier is no longer active, or its agreement is not valid today.");
        return p;
    }

    /// <summary>
    /// What the customer pays upfront: the charges Credit left upfront (with their tax) and the down payment when we take it (the
    /// supplier's agreement says COMPANY); when the supplier takes the down payment it is not in our claim.
    /// </summary>
    public static (decimal Charges, decimal DownPayment) ClaimAmounts(IEnumerable<LndLoanCharge> charges, string? downPaymentCollectorCode, decimal? downPayment) =>
        (charges.Where(x => x.TimingCode == LndCfCharges.Upfront).Sum(x => x.ChargeAmount + x.TaxAmount),
         downPaymentCollectorCode == "COMPANY" ? Money.Round(downPayment ?? 0, 2) : 0);

    /// <summary>The finance's own values, as Credit approval shows them.</summary>
    public static LndCfTermsDto Terms(LndLoan l) => new(
        l.GoodsPriceAmount ?? 0, l.DownPaymentAmount ?? 0, Money.Round((l.GoodsPriceAmount ?? 0) * (l.SupplierMinDownPaymentPct ?? 0) / 100m, 2),
        l.TermMonths, l.RepaymentFrequencyCode, l.InterestMethodCode, l.AnnualRatePct, l.DayCountCode, l.LateFeeTypeCode, l.LateFeeRatePct,
        l.LateFeeFlatAmount, l.LateFeeBaseCode, l.LateFeeGraceDays, LndProductTerms.Parts(l.PaymentOrder), l.IsInsuranceRequired, l.IsPrepaymentAllowed,
        l.PrepaymentRecalcCode, l.PrepaymentFeePct, l.EarlySettlementFeePct,
        new LndProductOptionsDto(LndProductTerms.Frequencies, LndProductTerms.InterestMethods, LndProductTerms.DayCounts, LndProductTerms.LateFeeTypes,
            LndProductTerms.LateFeeBases, LndProductTerms.PaymentParts, LndProductTerms.PrepaymentRecalcs));

    /// <summary>
    /// Applies Credit's values to the finance (a value left out stays): checked like the product's terms, the down payment against the
    /// price and the supplier's minimum, the term from 1 to 120 months, the rate from 0 to 100% (kept as a margin on the product rate).
    /// Returns what changed, old → new, for the history. The amount follows the down payment (and the charges, set after).
    /// </summary>
    public static List<string> ApplyTerms(LndLoan l, LndCfTermsRequest t, decimal productRatePct, decimal? marginPct = null)
    {
        static string C(string? s) => (s ?? "").Trim().ToUpperInvariant();
        var price = l.GoodsPriceAmount ?? 0;
        var v = new LndProductVersion
        {
            RepaymentFrequencyCode = t.RepaymentFrequencyCode is null ? l.RepaymentFrequencyCode : C(t.RepaymentFrequencyCode),
            InterestMethodCode = t.InterestMethodCode is null ? l.InterestMethodCode : C(t.InterestMethodCode),
            AnnualRatePct = t.AnnualRatePct ?? (marginPct is { } m ? productRatePct + m : l.AnnualRatePct),
            DayCountCode = t.DayCountCode is null ? l.DayCountCode : C(t.DayCountCode),
            LateFeeTypeCode = t.LateFeeTypeCode is null ? l.LateFeeTypeCode : C(t.LateFeeTypeCode),
            LateFeeBaseCode = t.LateFeeBaseCode is null ? l.LateFeeBaseCode : C(t.LateFeeBaseCode),
            PaymentOrder = t.PaymentOrder is null ? l.PaymentOrder : string.Join(",", t.PaymentOrder.Select(C)),
            IsInsuranceRequired = t.IsInsuranceRequired ?? l.IsInsuranceRequired,
            IsPrepaymentAllowed = t.IsPrepaymentAllowed ?? l.IsPrepaymentAllowed,
            PrepaymentRecalcCode = t.PrepaymentRecalcCode is null ? l.PrepaymentRecalcCode : C(t.PrepaymentRecalcCode),
            PrepaymentFeePct = t.PrepaymentFeePct ?? l.PrepaymentFeePct,
            EarlySettlementFeePct = t.EarlySettlementFeePct ?? l.EarlySettlementFeePct,
        };
        var typeChanged = t.LateFeeTypeCode is not null;
        v.LateFeeRatePct = v.LateFeeTypeCode is "PCT_ONCE" or "PCT_DAILY" ? (typeChanged ? t.LateFeeRatePct : t.LateFeeRatePct ?? l.LateFeeRatePct) : null;
        v.LateFeeFlatAmount = v.LateFeeTypeCode == "FLAT" ? (typeChanged ? t.LateFeeFlatAmount : t.LateFeeFlatAmount ?? l.LateFeeFlatAmount) : null;
        v.LateFeeGraceDays = v.LateFeeTypeCode == "NONE" ? 0 : t.LateFeeGraceDays ?? l.LateFeeGraceDays;
        var down = Money.Round(t.DownPaymentAmount ?? l.DownPaymentAmount ?? 0, 2);
        var term = t.TermMonths ?? l.TermMonths;

        var e = LndProductTerms.Check(v);
        var minDown = Money.Round(price * (l.SupplierMinDownPaymentPct ?? 0) / 100m, 2);
        if (down < minDown) e["downPaymentAmount"] = [$"The down payment must be at least {l.SupplierMinDownPaymentPct:0.##}% of the price ({minDown:#,0.00})."];
        else if (down >= price) e["downPaymentAmount"] = ["The down payment must be less than the price."];
        if (term is < 1 or > 120) e["termMonths"] = ["The term must be from 1 to 120 months."];
        if (e.Count > 0) throw new BusinessValidationException(e);

        // what changed, for the history
        var before = new LndProductVersion
        {
            RepaymentFrequencyCode = l.RepaymentFrequencyCode, InterestMethodCode = l.InterestMethodCode, AnnualRatePct = l.AnnualRatePct,
            DayCountCode = l.DayCountCode, LateFeeTypeCode = l.LateFeeTypeCode, LateFeeRatePct = l.LateFeeRatePct, LateFeeFlatAmount = l.LateFeeFlatAmount,
            LateFeeBaseCode = l.LateFeeBaseCode, LateFeeGraceDays = l.LateFeeGraceDays, PaymentOrder = l.PaymentOrder, IsInsuranceRequired = l.IsInsuranceRequired,
            IsPrepaymentAllowed = l.IsPrepaymentAllowed, PrepaymentRecalcCode = l.PrepaymentRecalcCode, PrepaymentFeePct = l.PrepaymentFeePct,
            EarlySettlementFeePct = l.EarlySettlementFeePct,
        };
        var changes = new List<string>();
        if (down != Money.Round(l.DownPaymentAmount ?? 0, 2)) changes.Add($"down payment {l.DownPaymentAmount:#,0.00} → {down:#,0.00}");
        if (term != l.TermMonths) changes.Add($"term {l.TermMonths} → {term} months");
        changes.AddRange(LndProductTerms.Diff(before, v).Select(c => $"{c.Field.ToLowerInvariant()} {c.OldValue} → {c.NewValue}"));

        l.DownPaymentAmount = down;
        l.TermMonths = term;
        l.RepaymentFrequencyCode = v.RepaymentFrequencyCode;
        l.InterestMethodCode = v.InterestMethodCode;
        l.AnnualRatePct = v.AnnualRatePct;
        l.RateMarginPct = Money.Round(v.AnnualRatePct - productRatePct, 6);
        l.DayCountCode = v.DayCountCode;
        l.LateFeeTypeCode = v.LateFeeTypeCode;
        l.LateFeeRatePct = v.LateFeeRatePct;
        l.LateFeeFlatAmount = v.LateFeeFlatAmount;
        l.LateFeeBaseCode = v.LateFeeBaseCode;
        l.LateFeeGraceDays = v.LateFeeGraceDays;
        l.PaymentOrder = v.PaymentOrder;
        l.IsInsuranceRequired = v.IsInsuranceRequired;
        l.IsPrepaymentAllowed = v.IsPrepaymentAllowed;
        l.PrepaymentRecalcCode = v.PrepaymentRecalcCode;
        l.PrepaymentFeePct = v.PrepaymentFeePct;
        l.EarlySettlementFeePct = v.EarlySettlementFeePct;
        return changes;
    }

    /// <summary>
    /// The amounts of the booking at delivery (rule CF_DELIVERY): the finance amount (LOAN_AMOUNT), the price less the down payment, owed
    /// to the supplier (FINANCED_AMOUNT), the charges added to the schedule (CAPITALISED_CHARGES_AMOUNT) and the down payment we took and
    /// held, now owed to the supplier (DOWN_PAYMENT_HELD_AMOUNT).
    /// </summary>
    public static Dictionary<string, decimal?> BookingAmounts(LndLoan l)
    {
        var financed = Money.Round((l.GoodsPriceAmount ?? 0) - (l.DownPaymentAmount ?? 0), 2);
        return new()
        {
            ["LOAN_AMOUNT"] = l.PrincipalAmount,
            ["FINANCED_AMOUNT"] = financed,
            ["CAPITALISED_CHARGES_AMOUNT"] = Money.Round(l.PrincipalAmount - financed, 2),
            ["DOWN_PAYMENT_HELD_AMOUNT"] = l.ClaimStatusCode == Claim.Paid ? l.ClaimDownPaymentAmount ?? 0 : 0,
        };
    }

    /// <summary>The delivery: a date from the Schedule step's day to today, the delivery note number, and the customer's confirmation.</summary>
    public static Dictionary<string, string[]> CheckDelivery(LndCfDeliveryRequest d, DateOnly scheduledOn, DateOnly today)
    {
        var e = new Dictionary<string, string[]>();
        if (d.DeliveryDate is not { } date) e["deliveryDate"] = ["Enter the delivery date."];
        else if (date > today) e["deliveryDate"] = ["The delivery date cannot be in the future."];
        else if (date < scheduledOn) e["deliveryDate"] = [$"The delivery date cannot be before the schedule was built ({scheduledOn:dd MMM yyyy})."];
        if (string.IsNullOrWhiteSpace(d.DeliveryNoteNo)) e["deliveryNoteNo"] = ["Enter the delivery note number."];
        else if (d.DeliveryNoteNo.Trim().Length > 50) e["deliveryNoteNo"] = ["The delivery note number is longer than 50 characters."];
        if (d.SerialNo is { Length: > 100 }) e["serialNo"] = ["The serial number is longer than 100 characters."];
        if (d.CustomerConfirmed != true) e["customerConfirmed"] = ["Confirm the customer received the goods and they match the contract."];
        return e;
    }

    /// <summary>
    /// What the supplier is owed (Supplier payment): the price less the down payment it took, or the whole price when we took the down
    /// payment (held, then owed at delivery); its agreed commission is that % of the amount owed, taken from the payment.
    /// </summary>
    public static LndCfSupplierDueDto SupplierDue(LndLoan l)
    {
        var a = BookingAmounts(l);
        var payable = (a["FINANCED_AMOUNT"] ?? 0) + (a["DOWN_PAYMENT_HELD_AMOUNT"] ?? 0);
        var pct = l.SupplierCommissionPct ?? 0;
        var commission = Money.Round(payable * pct / 100m, 2);
        return new LndCfSupplierDueDto(payable, pct, commission, payable - commission, l.SupplierPaymentDueDate);
    }

    /// <summary>The payment: a bank account, a reference, a commission from 0 to the amount owed, a reason when it is not the agreed one.</summary>
    public static Dictionary<string, string[]> CheckSupplierPayment(LndCfSupplierPaymentRequest r, LndCfSupplierDueDto due)
    {
        var e = new Dictionary<string, string[]>();
        if (r.BankAccountId is null) e["bankAccountId"] = ["Choose the bank account the supplier is paid from."];
        if (string.IsNullOrWhiteSpace(r.Reference)) e["reference"] = ["Enter the transfer reference."];
        else if (r.Reference.Trim().Length > 100) e["reference"] = ["The reference is longer than 100 characters."];
        var commission = Money.Round(r.CommissionAmount ?? due.CommissionAmount, 2);
        if (commission < 0 || commission > due.PayableAmount) e["commissionAmount"] = ["The commission must be from 0 to the amount owed."];
        else if (commission != due.CommissionAmount && string.IsNullOrWhiteSpace(r.CommissionReason))
            e["commissionReason"] = [$"Write why the commission is not the agreed {due.CommissionPct:0.##}% ({due.CommissionAmount:#,0.00})."];
        if (r.CommissionReason is { Length: > 500 }) e["commissionReason"] = ["The reason is longer than 500 characters."];
        return e;
    }

    /// <summary>The finance rate: the product rate plus Credit's margin, from 0 to 100%.</summary>
    public static decimal FinanceRate(decimal productRatePct, decimal marginPct)
    {
        var rate = productRatePct + marginPct;
        if (rate < 0 || rate > 100)
            throw BusinessValidationException.For("rateMarginPct", $"The product rate {productRatePct:0.####}% plus the margin must be from 0 to 100%.");
        return rate;
    }

    /// <summary>The schedule (and, on older finances, Charges and claim) waits until the claim is paid, or there is nothing upfront.</summary>
    public static string? CheckClaim(string? claimStatus, decimal claimTotal, decimal paid = 0) => claimStatus switch
    {
        Claim.Paid or Claim.None => null,
        Claim.Open or Claim.PartiallyPaid => $"Waiting for the claim to be paid (outstanding {claimTotal - paid:#,0.00}). It is paid in Collection › Customer Payments.",
        _ => claimTotal == 0 ? null : "Create the claim first.",
    };
}

/// <summary>The open step of a finance, whether the signed-in user holds its task, and what the step shows.</summary>
public sealed record LndCfStepDto(string StepCode, string StepName, long? InboxId, bool IsMine, LndCfChargesDto? Charges = null,
    IReadOnlyList<LndPayingAccountDto>? Accounts = null, decimal? ProductRatePct = null, decimal RateMarginPct = 0, LndCfTermsDto? Terms = null,
    LndCfSupplierDueDto? SupplierDue = null, bool ClaimAtCredit = false, bool ViewOnly = false, IReadOnlyList<string>? GlTranSetIds = null);

/// <summary>
/// The financial values of a finance, its own copy (Ahmed, 27 Nov 2026): Credit approval can change every one of them for this finance.
/// The price is the supplier's and is not changed.
/// </summary>
public sealed record LndCfTermsDto(
    decimal PriceAmount, decimal DownPaymentAmount, decimal MinDownPaymentAmount, int TermMonths, string RepaymentFrequencyCode,
    string InterestMethodCode, decimal AnnualRatePct, string DayCountCode, string LateFeeTypeCode, decimal? LateFeeRatePct, decimal? LateFeeFlatAmount,
    string LateFeeBaseCode, int LateFeeGraceDays, IReadOnlyList<string> PaymentOrder, bool IsInsuranceRequired, bool IsPrepaymentAllowed,
    string PrepaymentRecalcCode, decimal PrepaymentFeePct, decimal EarlySettlementFeePct, LndProductOptionsDto Options);

/// <summary>Credit's values for the finance; a value left out stays as it is.</summary>
public sealed record LndCfTermsRequest(
    decimal? DownPaymentAmount = null, int? TermMonths = null, string? RepaymentFrequencyCode = null, string? InterestMethodCode = null,
    decimal? AnnualRatePct = null, string? DayCountCode = null, string? LateFeeTypeCode = null, decimal? LateFeeRatePct = null,
    decimal? LateFeeFlatAmount = null, string? LateFeeBaseCode = null, int? LateFeeGraceDays = null, IReadOnlyList<string>? PaymentOrder = null,
    bool? IsInsuranceRequired = null, bool? IsPrepaymentAllowed = null, string? PrepaymentRecalcCode = null, decimal? PrepaymentFeePct = null,
    decimal? EarlySettlementFeePct = null);

/// <summary>
/// ACCEPT or REJECT. At Credit approval: ChargeTimings and ChargeAmounts are Credit's decision per charge code (PAID_UPFRONT or
/// FINANCE_WITH_PRINCIPAL; the amount, 0 to waive), and RateMarginPct the margin on the product rate (+ or -).
/// </summary>
public sealed record LndCfCompleteRequest(string? Action, string? Comment, IReadOnlyDictionary<string, string>? ChargeTimings = null,
    IReadOnlyDictionary<string, decimal>? ChargeAmounts = null, decimal? RateMarginPct = null, LndCfTermsRequest? Terms = null,
    LndCfDeliveryRequest? Delivery = null, LndCfSupplierPaymentRequest? SupplierPayment = null);

/// <summary>
/// Supplier payment: our bank account it is paid from, the transfer reference, and the commission (the supplier's agreed %; another
/// amount needs a reason).
/// </summary>
public sealed record LndCfSupplierPaymentRequest(long? BankAccountId, string? Reference, decimal? CommissionAmount, string? CommissionReason);

/// <summary>What the supplier is owed for a finance, its agreed commission, and the net paid.</summary>
public sealed record LndCfSupplierDueDto(decimal PayableAmount, decimal CommissionPct, decimal CommissionAmount, decimal NetAmount, DateOnly? DueDate);

/// <summary>Delivery and customer receipt: the day, the delivery note, the serial or IMEI, and the customer's confirmation.</summary>
public sealed record LndCfDeliveryRequest(DateOnly? DeliveryDate, string? DeliveryNoteNo, string? SerialNo, bool? CustomerConfirmed);

/// <summary>
/// The charges of a consumer finance from the rule its Charges and claim step names (finova:ruleCode, CF_CHARGES by default): worked out
/// for Credit approval, and saved with Credit's decision.
/// </summary>
public sealed class LndCfChargesPlanner(ICapitraDbContext db, AdmProductEventRuleService rules, IClock clock)
{
    /// <summary>
    /// The step that creates the finance's claim and names its charges' rule: Charges and claim on runs that have it (started before
    /// 27 Nov 2026), else Credit approval.
    /// </summary>
    public async Task<string> ClaimStepAsync(LndLoan loan, CancellationToken ct)
    {
        var xml = loan.WorkflowInstanceId is { } run ? await WfeVersions.XmlOfInstanceAsync(db, run, ct) : null;
        return xml is not null && BpmnModel.StepByCode(xml, LndCfFinanceRules.Charges) is not null ? LndCfFinanceRules.Charges : LndCfFinanceRules.Credit;
    }

    /// <summary>The rule a step names in the BPMN the finance's run is on (the claim's step when none is given).</summary>
    public async Task<string> RuleCodeAsync(LndLoan loan, CancellationToken ct, string? step = null)
    {
        step ??= await ClaimStepAsync(loan, ct);
        var xml = loan.WorkflowInstanceId is { } run ? await WfeVersions.XmlOfInstanceAsync(db, run, ct) : null;
        var code = xml is null ? null : BpmnModel.StepByCode(xml, step)?.RuleCode?.Trim();
        // the step names its rule (finova:ruleCode); else the rule of the step's own code (CF_DELIVERY, ...), CF_CHARGES for the charges
        return string.IsNullOrWhiteSpace(code) ? (step is LndCfFinanceRules.Charges or LndCfFinanceRules.Credit ? LndCfCharges.RuleCode : step) : code;
    }

    public async Task<(string ProductCode, string BusinessLine, string Module, ProductEventPlan? Plan, string RuleCode)> PlanAsync(LndLoan loan, CancellationToken ct,
        string? step = null)
    {
        var product = await db.Set<AdmProduct>().AsNoTracking().SingleAsync(x => x.ProductId == loan.ProductId, ct);
        var ruleCode = await RuleCodeAsync(loan, ct, step);
        var plan = await rules.PlanByCodeAsync(product.ProductCode, ruleCode, LndLoanService.CairoToday(clock), ct);
        var module = string.IsNullOrWhiteSpace(product.GlTranSetCode) ? "LND" : product.GlTranSetCode.Trim().ToUpperInvariant();
        return (product.ProductCode, product.BusinessLineCode, module, plan, ruleCode);
    }

    /// <summary>The charges Credit saved; else the rule's charges with the timings given (or the rule's defaults).</summary>
    public async Task<LndCfChargesDto> WorkAsync(LndLoan loan, IReadOnlyDictionary<string, string>? timings, CancellationToken ct, decimal? down = null,
        IReadOnlyDictionary<string, decimal>? amounts = null)
    {
        var downPayment = Money.Round(down ?? loan.DownPaymentAmount ?? 0, 2);
        var financed = Money.Round((loan.GoodsPriceAmount ?? 0) - downPayment, 2);
        var saved = await db.Set<LndLoanCharge>().AsNoTracking().Where(x => x.LoanId == loan.LoanId).OrderBy(x => x.LoanChargeId).ToListAsync(ct);
        var (productCode, _, _, plan, ruleCode) = await PlanAsync(loan, ct);
        if (saved.Count > 0 && timings is null && down is null && amounts is null)
        {
            // Credit's decision, each with how its rule works it out
            string BasisOf(string code) => plan?.Charges.FirstOrDefault(c => c.ChargeCode == code) is { } c ? LndCfCharges.Basis(c) : "";
            return LndCfCharges.Totals([.. saved.Select(x => new LndCfChargeDto(x.ChargeCode, x.ChargeName, BasisOf(x.ChargeCode), x.ChargeAmount, x.TaxAmount,
                x.TimingCode, x.TimingCode, x.IsOverridden, x.ChargeAmount))], financed, true);
        }
        if (plan is null)
            return new LndCfChargesDto([], financed, 0, 0, financed,
                $"Product {productCode} has no active rule {ruleCode} today. Add it in Admin Setup > Product Catalog > {productCode} > Rules.");
        return LndCfCharges.Work(plan.Charges, Money.Round(loan.GoodsPriceAmount ?? 0, 2), downPayment, timings, amounts);
    }

    /// <summary>
    /// Saves Credit's decision on a tracked finance: its charges (each marked when it changed the rule's timing or amount) and its amount,
    /// the financed amount plus the charges added to the schedule. Not saved to the database here. Returns what Credit changed, for the
    /// history.
    /// </summary>
    public async Task<List<string>> DecideAsync(LndLoan loan, IReadOnlyDictionary<string, string>? timings, CancellationToken ct,
        IReadOnlyDictionary<string, decimal>? amounts = null)
    {
        var old = await db.Set<LndLoanCharge>().Where(x => x.LoanId == loan.LoanId).ToListAsync(ct);
        db.Set<LndLoanCharge>().RemoveRange(old);
        var (productCode, _, _, plan, ruleCode) = await PlanAsync(loan, ct);
        if (plan is null)
            throw new BusinessRuleException($"Product {productCode} has no active rule {ruleCode} today. Add it in Admin Setup > Product Catalog > {productCode} > Rules.");
        var work = LndCfCharges.Work(plan.Charges, Money.Round(loan.GoodsPriceAmount ?? 0, 2), Money.Round(loan.DownPaymentAmount ?? 0, 2), timings, amounts);
        if (work.Problem is not null) throw new BusinessRuleException(work.Problem);
        db.Set<LndLoanCharge>().AddRange(work.Charges.Select(c => new LndLoanCharge
        {
            LoanId = loan.LoanId, ChargeCode = c.ChargeCode, ChargeName = c.ChargeName, ChargeAmount = c.Amount, TaxAmount = c.TaxAmount,
            TimingCode = c.TimingCode, IsOverridden = c.IsOverridden,
        }));
        loan.PrincipalAmount = work.LoanAmount;
        return [.. work.Charges.Where(c => c.IsOverridden).Select(c =>
            $"{c.ChargeName}{(c.Amount != c.RuleAmount ? $" {c.RuleAmount:#,0.00} → {c.Amount:#,0.00}" : "")}"
            + $"{(c.TimingCode != c.DefaultTimingCode ? (c.TimingCode == LndCfCharges.Upfront ? ", upfront" : ", added to schedule") : "")}")];
    }
}

/// <summary>
/// The business effect of CF_FINANCE's steps, run inside the task's completion (a failed check leaves the task open):
/// REJECT rejects the finance; ACCEPT checks segregation of duties and the step's rules, then moves the finance on.
/// </summary>
public sealed class LndCfFinanceWorkflowHandler(
    ICapitraDbContext db, IClock clock, CrmCustomer360Service customers, LndCfChargesPlanner planner,
    ProductActionPosting posting, LndCfClaimService claims)
    : IWfeTaskCompletionHandler
{
    public bool CanHandle(WfeTaskContext context) =>
        string.Equals(context.WorkflowCode, LndCfFinanceRules.WorkflowCode, StringComparison.OrdinalIgnoreCase)
        && LndCfFinanceRules.Steps.Contains(context.StepCode ?? "", StringComparer.OrdinalIgnoreCase);

    public async Task HandleAsync(WfeTaskContext context, CancellationToken cancellationToken)
    {
        var loan = await db.Set<LndLoan>().FirstOrDefaultAsync(x => x.WorkflowInstanceId == context.WorkflowInstanceId, cancellationToken)
            ?? throw new BusinessRuleException("The workflow's finance was not found.");
        var step = (context.StepCode ?? "").ToUpperInvariant();
        var name = LndCfFinanceRules.StepName(step);

        if (!string.Equals(context.Action, "ACCEPT", StringComparison.OrdinalIgnoreCase))
        {
            // an unpaid claim is cancelled (its GL reversed); a paid one is to be refunded
            var claimNote = await claims.CancelAsync(loan, cancellationToken);
            Move(loan, LndLoanService.Statuses.Rejected, $"Rejected at {name} by {context.User}.{(claimNote is null ? "" : " " + claimNote)}");
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        // anyone holding the step's role does it, the person who entered the finance too (Ahmed, 28 Nov 2026)
        if (step == LndCfFinanceRules.Agreement)
            throw new BusinessRuleException("This finance started on the first version of the workflow. Reject it and enter it again.");
        if (!LndCfFinanceRules.Ready.Contains(step))
            throw new BusinessRuleException($"{name} is not available yet; it comes with the next update.");

        switch (step)
        {
            case LndCfFinanceRules.Credit:
            {
                var compliance = await customers.ComplianceBadgeAsync(loan.ClientId, cancellationToken);
                var supplierActive = await LndLoanService.ActiveSupplierAgreements(db, LndLoanService.CairoToday(clock))
                    .AnyAsync(x => x.ApplicationId == loan.SupplierApplicationId, cancellationToken);
                var problems = LndCfFinanceRules.CheckCredit(compliance.Code, compliance.Label, supplierActive);
                if (problems.Count > 0) throw new BusinessRuleException($"{name} cannot be accepted: {string.Join(" ", problems)}");
                // accepted from the inbox, without the finance page: the rule's defaults are Credit's decision
                if (!await db.Set<LndLoanCharge>().AnyAsync(x => x.LoanId == loan.LoanId, cancellationToken))
                {
                    await planner.DecideAsync(loan, null, cancellationToken);
                    await db.SaveChangesAsync(cancellationToken); // the claim reads the charges saved
                }
                // the claim is created once, here (Ahmed, 27 Nov 2026), unless the run still has the Charges and claim step
                string claimNote = "";
                if (await planner.ClaimStepAsync(loan, cancellationToken) == LndCfFinanceRules.Credit)
                {
                    await claims.CreateAsync(loan, LndCfFinanceRules.Credit, cancellationToken);
                    claimNote = loan.ClaimStatusCode == LndCfFinanceRules.Claim.None ? " Nothing upfront: no claim."
                        : $" Claim {(loan.ClaimChargesAmount ?? 0) + (loan.ClaimDownPaymentAmount ?? 0):#,0.00} created.";
                }
                Move(loan, LndLoanService.Statuses.Approved, $"Approved at {name} by {context.User}: amount {loan.PrincipalAmount:#,0.00} over {loan.TermMonths} months.{claimNote}");
                break;
            }
            case LndCfFinanceRules.Charges:
            {
                var charges = await db.Set<LndLoanCharge>().AsNoTracking().Where(x => x.LoanId == loan.LoanId).ToListAsync(cancellationToken);
                var (c, d) = LndCfFinanceRules.ClaimAmounts(charges, loan.DownPaymentCollectorCode, loan.DownPaymentAmount);
                if (LndCfFinanceRules.CheckClaim(loan.ClaimStatusCode, c + d, LndCfClaimService.Paid(loan)) is { } problem) throw new BusinessRuleException(problem);
                if (loan.ClaimStatusCode is null)
                {
                    loan.ClaimStatusCode = LndCfFinanceRules.Claim.None;
                    loan.ClaimChargesAmount = 0;
                    loan.ClaimDownPaymentAmount = 0;
                }
                break;
            }
            case LndCfFinanceRules.Schedule:
            {
                // the claim must be paid first (or nothing is paid upfront)
                if (LndCfFinanceRules.CheckClaim(loan.ClaimStatusCode, (loan.ClaimChargesAmount ?? 0) + (loan.ClaimDownPaymentAmount ?? 0), LndCfClaimService.Paid(loan)) is { } unpaid)
                    throw new BusinessRuleException($"{name} cannot be accepted. {unpaid}");
                // the schedule on the loan amount Credit approved (the financed amount plus the charges added to it), from today; the
                // Delivery step moves its dates to the delivery day
                var lines = await ScheduleAsync(loan, LndLoanService.CairoToday(clock), cancellationToken);
                db.Set<LndLoanStatusHist>().Add(new LndLoanStatusHist
                {
                    LoanId = loan.LoanId, FromStatusCode = loan.StatusCode, ToStatusCode = loan.StatusCode,
                    Reason = $"Schedule built at {name} by {context.User}: {lines.Count} instalments of {lines[0].Total:#,0.00}.",
                });
                break;
            }
            case LndCfFinanceRules.Delivery:
            {
                // Delivery and customer receipt (27 Nov 2026): the finance is booked (the step's rule, CF_DELIVERY), it becomes active,
                // its schedule runs from the delivery day and the supplier is paid by the delivery day plus its payment days
                if (loan.DeliveryDate is not { } delivered)
                    throw new BusinessRuleException("Enter the delivery on the finance page first: the date, the delivery note and the customer's confirmation.");
                var (_, businessLine, module, plan, ruleCode) = await planner.PlanAsync(loan, cancellationToken, LndCfFinanceRules.Delivery);
                if (plan is null) throw new BusinessRuleException($"The finance's product has no active rule {ruleCode} today. Add it in Admin Setup.");
                var amounts = LndCfFinanceRules.BookingAmounts(loan);
                var lines = ProductEventPosting.Build(plan, l => amounts.GetValueOrDefault(l.AmountCode ?? "") ?? 0, null);
                var doc = new ProductActionDocument("LND_CF_DELIVERY", loan.LoanId.ToString(CultureInfo.InvariantCulture), loan.LoanNumber, delivered,
                    loan.CurrencyCode, $"Finance {loan.LoanNumber} booked at delivery, {loan.PrincipalAmount:#,0.00}");
                try
                {
                    loan.DeliveryGlTranSetId = await posting.PostLinesAsync(plan, lines, module, "Consumer finance booked at delivery", "3100", businessLine, doc, cancellationToken);
                }
                catch (ApexProcessException ex)
                {
                    throw new BusinessRuleException($"GL posting failed, the finance was not booked: {GlPostingApi.CleanMessage(ex.Message)}");
                }
                var schedule = await ScheduleAsync(loan, delivered, cancellationToken);
                loan.SupplierPaymentDueDate = delivered.AddDays(loan.SupplierPaymentDays ?? 0);
                db.Set<LndLoanEvent>().Add(new LndLoanEvent
                {
                    LoanId = loan.LoanId, EventCode = "CF_DELIVERY", EventDate = delivered, EventAmount = loan.PrincipalAmount, GlTranSetId = loan.DeliveryGlTranSetId,
                    SourceDocumentType = "LND_CF_DELIVERY", SourceDocumentId = loan.LoanId,
                    Notes = $"Delivered on {delivered:dd MMM yyyy}, delivery note {loan.DeliveryNoteNo}; supplier to be paid by {loan.SupplierPaymentDueDate:dd MMM yyyy}.",
                });
                Move(loan, LndLoanService.Statuses.Active,
                    $"Booked at {name} by {context.User}: delivered {delivered:dd MMM yyyy}, {schedule.Count} instalments of {schedule[0].Total:#,0.00} from {schedule[0].DueDate:dd MMM yyyy}.");
                break;
            }
            case LndCfFinanceRules.SupplierPayment:
            {
                // Supplier payment (27 Nov 2026): the step's rule CF_SUPPLIER_PAYMENT: the amount owed settled, the commission earned,
                // the net paid from our bank account
                if (loan.SupplierPaymentBankAccountId is not { } accountId || loan.SupplierCommissionAmount is not { } commission)
                    throw new BusinessRuleException("Enter the payment on the finance page first: the bank account, the reference and the commission.");
                var bankGl = await db.Set<TrsBankAccount>().AsNoTracking().Where(x => x.AccountId == accountId).Select(x => x.ErpAccountCode).FirstOrDefaultAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(bankGl)) throw new BusinessRuleException("The bank account has no GL account code. Set it in Treasury.");
                var (_, businessLine, module, plan, ruleCode) = await planner.PlanAsync(loan, cancellationToken, LndCfFinanceRules.SupplierPayment);
                if (plan is null) throw new BusinessRuleException($"The finance's product has no active rule {ruleCode} today. Add it in Admin Setup.");
                var due = LndCfFinanceRules.SupplierDue(loan);
                var net = due.PayableAmount - commission;
                var amounts = new Dictionary<string, decimal> { ["PAYABLE_AMOUNT"] = due.PayableAmount, ["COMMISSION_AMOUNT"] = commission, ["NET_PAYMENT_AMOUNT"] = net };
                var lines = ProductEventPosting.Build(plan, l => amounts.GetValueOrDefault(l.AmountCode ?? ""), bankGl.Trim());
                var today = LndLoanService.CairoToday(clock);
                var doc = new ProductActionDocument("LND_CF_SUPPLIER_PAYMENT", loan.LoanId.ToString(CultureInfo.InvariantCulture), loan.LoanNumber, today,
                    loan.CurrencyCode, $"Finance {loan.LoanNumber}: supplier paid {net:#,0.00} (commission {commission:#,0.00}), {loan.SupplierPaymentReference}");
                try
                {
                    loan.SupplierPaymentGlTranSetId = await posting.PostLinesAsync(plan, lines, module, "Consumer finance supplier payment", "3100", businessLine, doc, cancellationToken);
                }
                catch (ApexProcessException ex)
                {
                    throw new BusinessRuleException($"GL posting failed, the supplier was not paid: {GlPostingApi.CleanMessage(ex.Message)}");
                }
                loan.SupplierPaidAt = clock.UtcNow;
                loan.SupplierPaidBy = context.User;
                db.Set<LndLoanEvent>().Add(new LndLoanEvent
                {
                    LoanId = loan.LoanId, EventCode = "CF_SUPPLIER_PAYMENT", EventDate = today, EventAmount = net, GlTranSetId = loan.SupplierPaymentGlTranSetId,
                    SourceDocumentType = "LND_CF_SUPPLIER_PAYMENT", SourceDocumentId = loan.LoanId,
                    Notes = $"Supplier paid {net:#,0.00}: owed {due.PayableAmount:#,0.00} less commission {commission:#,0.00}"
                        + (loan.SupplierCommissionReason is { } why ? $" (changed from the agreed {due.CommissionAmount:#,0.00}: {why})" : "") + $"; reference {loan.SupplierPaymentReference}.",
                });
                db.Set<LndLoanStatusHist>().Add(new LndLoanStatusHist
                {
                    LoanId = loan.LoanId, FromStatusCode = loan.StatusCode, ToStatusCode = loan.StatusCode,
                    Reason = $"Supplier paid at {name} by {context.User}: {net:#,0.00}, commission {commission:#,0.00}.",
                });
                break;
            }
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>The schedule on the finance amount from a start day (the Schedule step's day, then the delivery day); the header follows it.</summary>
    private async Task<IReadOnlyList<LndScheduleLine>> ScheduleAsync(LndLoan loan, DateOnly start, CancellationToken ct)
    {
        var first = LndSchedule.DefaultFirstDueDate(start, loan.RepaymentFrequencyCode);
        var lines = LndSchedule.Build(loan.PrincipalAmount, loan.TermMonths, loan.RepaymentFrequencyCode, loan.InterestMethodCode, loan.AnnualRatePct, first);
        db.Set<LndInstallment>().RemoveRange(await db.Set<LndInstallment>().Where(x => x.LoanId == loan.LoanId).ToListAsync(ct));
        db.Set<LndInstallment>().AddRange(LndLoanService.Installments(loan.LoanId, lines));
        loan.DisbursementDate = start;
        loan.FirstDueDate = first;
        loan.InstallmentCount = lines.Count;
        loan.InstallmentAmount = lines[0].Total;
        loan.TotalInterestAmount = lines.Sum(x => x.Interest);
        loan.MaturityDate = lines[^1].DueDate;
        return lines;
    }

    private void Move(LndLoan loan, string to, string reason)
    {
        db.Set<LndLoanStatusHist>().Add(new LndLoanStatusHist { LoanId = loan.LoanId, FromStatusCode = loan.StatusCode, ToStatusCode = to, Reason = reason });
        loan.StatusCode = to;
    }
}

/// <summary>
/// The finance page's side of CF_FINANCE: the open step and what it shows, completing the signed-in user's task, and the claim of the
/// Charges and claim step (create it, record its payment).
/// </summary>
public sealed class LndCfFinanceService(
    ICapitraDbContext db, WfeInbox inbox, LndCfChargesPlanner planner, IClock clock, ICurrentUser currentUser,
    LndCfClaimService claims)
{
    private string User => currentUser.UserName ?? currentUser.UserId ?? "SYSTEM";

    /// <summary>
    /// The step waiting now, or null when the finance is not in its workflow; IsMine when inboxId is the user's open task on it. With view,
    /// a completed step instead, view only (28 Nov 2026, the factoring standard): what it decided and the GL tran sets it posted.
    /// </summary>
    public async Task<LndCfStepDto?> StepAsync(long loanId, long? inboxId, CancellationToken ct, decimal? downPayment = null, string? view = null)
    {
        var loan = await db.Set<LndLoan>().AsNoTracking().FirstOrDefaultAsync(x => x.LoanId == loanId, ct)
            ?? throw new NotFoundException($"Finance {loanId} was not found.");
        if (loan.WorkflowInstanceId is not { } run) return null;
        if (!string.IsNullOrWhiteSpace(view))
        {
            var code = view.Trim().ToUpperInvariant();
            var done = await db.Set<WfeStepInstance>().AsNoTracking()
                .AnyAsync(s => s.WorkflowInstanceId == run && s.StepCode == code && s.StepStatus == WfeRuntime.Completed, ct);
            if (!done) throw new BusinessRuleException($"{LndCfFinanceRules.StepName(code)} is not completed on finance {loan.LoanNumber}.");
            var rate = await db.Set<LndProductVersion>().AsNoTracking().Where(x => x.ProductVersionId == loan.ProductVersionId)
                .Select(x => (decimal?)x.AnnualRatePct).FirstOrDefaultAsync(ct);
            var claimStep = await planner.ClaimStepAsync(loan, ct);
            return new LndCfStepDto(code, LndCfFinanceRules.StepName(code), null, false,
                code is LndCfFinanceRules.Credit or LndCfFinanceRules.Charges ? await planner.WorkAsync(loan, null, ct) : null, null, rate, loan.RateMarginPct,
                code == LndCfFinanceRules.Credit ? LndCfFinanceRules.Terms(loan) : null,
                code == LndCfFinanceRules.SupplierPayment ? LndCfFinanceRules.SupplierDue(loan) : null,
                code == LndCfFinanceRules.Credit && claimStep == LndCfFinanceRules.Credit, true, LndCfStepsService.GlOf(loan, code, claimStep));
        }
        var open = await db.Set<WfeStepInstance>().AsNoTracking()
            .Where(s => s.WorkflowInstanceId == run && s.StepStatus != WfeRuntime.Completed && s.StepStatus != WfeRuntime.Cancelled
                && s.StepCode != null && s.StepCode != "")
            .OrderByDescending(s => s.StepInstanceId).Select(s => s.StepCode).FirstOrDefaultAsync(ct);
        if (open is null) return null;
        var mine = false;
        if (inboxId is { } id)
        {
            try
            {
                var item = await inbox.GetAsync(id, ct);
                mine = item.InstanceId == run && item.Status != WfeRuntime.Completed;
            }
            catch (Exception ex) when (ex is NotFoundException or BusinessRuleException) { mine = false; }
        }
        var step = open.ToUpperInvariant();
        LndCfChargesDto? charges = step is LndCfFinanceRules.Credit or LndCfFinanceRules.Charges
            ? await planner.WorkAsync(loan, null, ct, step == LndCfFinanceRules.Credit ? downPayment : null) : null;
        IReadOnlyList<LndPayingAccountDto>? accounts = step is LndCfFinanceRules.Charges or LndCfFinanceRules.SupplierPayment && mine
            ? await AccountsAsync(loan.CurrencyCode, ct) : null;
        var productRate = await db.Set<LndProductVersion>().AsNoTracking().Where(x => x.ProductVersionId == loan.ProductVersionId)
            .Select(x => (decimal?)x.AnnualRatePct).FirstOrDefaultAsync(ct);
        return new LndCfStepDto(open, LndCfFinanceRules.StepName(open), mine ? inboxId : null, mine, charges, accounts, productRate, loan.RateMarginPct,
            step == LndCfFinanceRules.Credit && mine ? LndCfFinanceRules.Terms(loan) : null,
            step == LndCfFinanceRules.SupplierPayment ? LndCfFinanceRules.SupplierDue(loan) : null,
            step == LndCfFinanceRules.Credit && await planner.ClaimStepAsync(loan, ct) == LndCfFinanceRules.Credit);
    }

    /// <summary>
    /// Completes the signed-in user's task: ACCEPT, or REJECT with the reason. Accepting Credit approval saves Credit's decision on the
    /// charges first. The handler's checks run inside it.
    /// </summary>
    public async Task CompleteAsync(long loanId, long inboxId, LndCfCompleteRequest r, CancellationToken ct)
    {
        var action = (r.Action ?? "").Trim().ToUpperInvariant();
        if (action is not ("ACCEPT" or "REJECT")) throw BusinessValidationException.For("action", "Accept or reject the finance.");
        var comment = string.IsNullOrWhiteSpace(r.Comment) ? null : r.Comment.Trim();
        if (action == "REJECT" && comment is null) throw BusinessValidationException.For("comment", "Write why the finance is rejected.");
        if (comment is { Length: > 2000 }) throw BusinessValidationException.For("comment", "The comment is longer than 2000 characters.");

        await using var tx = await db.BeginTransactionAsync(ct);
        var (loan, item) = await TaskAsync(loanId, inboxId, null, ct);
        var changes = new List<string>();
        if (action == "ACCEPT" && string.Equals(item.StepCode, LndCfFinanceRules.Credit, StringComparison.OrdinalIgnoreCase))
        {
            // Credit's values for this finance (its own copy): every financial value can change; the down payment first, as the charges follow it
            var productRate = await db.Set<LndProductVersion>().Where(x => x.ProductVersionId == loan.ProductVersionId).Select(x => x.AnnualRatePct).SingleAsync(ct);
            changes = LndCfFinanceRules.ApplyTerms(loan, r.Terms ?? new LndCfTermsRequest(), productRate, r.RateMarginPct);
            changes.AddRange(await planner.DecideAsync(loan, r.ChargeTimings, ct, r.ChargeAmounts));
            await db.SaveChangesAsync(ct);
        }
        if (action == "ACCEPT" && string.Equals(item.StepCode, LndCfFinanceRules.Delivery, StringComparison.OrdinalIgnoreCase))
        {
            var d = r.Delivery ?? new LndCfDeliveryRequest(null, null, null, null);
            var e = LndCfFinanceRules.CheckDelivery(d, loan.DisbursementDate, LndLoanService.CairoToday(clock));
            if (e.Count > 0) throw new BusinessValidationException(e);
            loan.DeliveryDate = d.DeliveryDate;
            loan.DeliveryNoteNo = d.DeliveryNoteNo!.Trim();
            loan.DeliverySerialNo = string.IsNullOrWhiteSpace(d.SerialNo) ? null : d.SerialNo.Trim();
            await db.SaveChangesAsync(ct);
        }
        if (action == "ACCEPT" && string.Equals(item.StepCode, LndCfFinanceRules.SupplierPayment, StringComparison.OrdinalIgnoreCase))
        {
            var p = r.SupplierPayment ?? new LndCfSupplierPaymentRequest(null, null, null, null);
            var due = LndCfFinanceRules.SupplierDue(loan);
            var e = LndCfFinanceRules.CheckSupplierPayment(p, due);
            var account = p.BankAccountId is { } accountId ? await db.Set<TrsBankAccount>().AsNoTracking().FirstOrDefaultAsync(x => x.AccountId == accountId, ct) : null;
            if (p.BankAccountId is not null && (account is null || !account.IsActive || account.CurrencyCode != loan.CurrencyCode))
                e["bankAccountId"] = ["Choose one of our active accounts in the finance's currency."];
            else if (account is not null && string.IsNullOrWhiteSpace(account.ErpAccountCode))
                e["bankAccountId"] = [$"Bank account {account.AccountRef} has no GL account code. Set it in Treasury."];
            if (e.Count > 0) throw new BusinessValidationException(e);
            var commission = Money.Round(p.CommissionAmount ?? due.CommissionAmount, 2);
            loan.SupplierCommissionAmount = commission;
            loan.SupplierCommissionReason = commission == due.CommissionAmount ? null : p.CommissionReason!.Trim();
            loan.SupplierPaymentBankAccountId = p.BankAccountId;
            loan.SupplierPaymentReference = p.Reference!.Trim();
            await db.SaveChangesAsync(ct);
        }
        await inbox.CompleteAsync(inboxId, new CompleteInboxItemRequest(action, comment), ct);
        var note = string.Join(" ", new[] { changes.Count > 0 ? $"Changed: {string.Join("; ", changes)}." : null, comment }.OfType<string>());
        if (note.Length > 0)
        {
            // what Credit changed and the step's comment go with the status change it made
            var last = await db.Set<LndLoanStatusHist>().Where(x => x.LoanId == loanId).OrderByDescending(x => x.LoanStatusHistId).FirstOrDefaultAsync(ct);
            if (last is not null && last.Reason?.Contains(LndCfFinanceRules.StepName(item.StepCode), StringComparison.Ordinal) == true)
            {
                last.Reason = $"{last.Reason} {note}";
                if (last.Reason.Length > 500) last.Reason = last.Reason[..500];
                await db.SaveChangesAsync(ct);
            }
        }
        await tx.CommitAsync(ct);
    }

    /// <summary>
    /// Charges and claim, on finances that started with that step: creates the claim from Credit's decision (see LndCfClaimService).
    /// </summary>
    public async Task CreateClaimAsync(long loanId, long inboxId, CancellationToken ct)
    {
        await using var tx = await db.BeginTransactionAsync(ct);
        var (loan, _) = await TaskAsync(loanId, inboxId, LndCfFinanceRules.Charges, ct);
        await claims.CreateAsync(loan, LndCfFinanceRules.Charges, ct);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    /// <summary>The tracked finance and the user's open task on it (at the step given), checked for segregation of duties.</summary>
    private async Task<(LndLoan Loan, WfeInboxItemDto Item)> TaskAsync(long loanId, long inboxId, string? step, CancellationToken ct)
    {
        var loan = await db.Set<LndLoan>().FirstOrDefaultAsync(x => x.LoanId == loanId, ct)
            ?? throw new NotFoundException($"Finance {loanId} was not found.");
        var item = await inbox.GetAsync(inboxId, ct); // throws when the task is not the user's
        if (loan.WorkflowInstanceId is null || item.InstanceId != loan.WorkflowInstanceId)
            throw new BusinessRuleException($"This task is not on finance {loan.LoanNumber}.");
        if (item.Status == WfeRuntime.Completed) throw new BusinessRuleException("This step is already completed.");
        if (step is not null)
        {
            if (!string.Equals(item.StepCode, step, StringComparison.OrdinalIgnoreCase))
                throw new BusinessRuleException($"This is done at {LndCfFinanceRules.StepName(step)}; the finance is at {LndCfFinanceRules.StepName(item.StepCode)}.");
        }
        return (loan, item);
    }

    private Task<List<LndPayingAccountDto>> AccountsAsync(string currencyCode, CancellationToken ct) => AccountsAsync(db, currencyCode, ct);

    /// <summary>Our active accounts in a currency (not escrow) money can come into or go out of.</summary>
    public static async Task<List<LndPayingAccountDto>> AccountsAsync(ICapitraDbContext db, string currencyCode, CancellationToken ct) =>
        await (from a in db.Set<TrsBankAccount>().AsNoTracking()
               join b in db.Set<TrsBank>().AsNoTracking() on a.BankId equals b.BankId
               where a.IsActive && a.CurrencyCode == currencyCode && a.AccountTypeCode != "ESCROW"
               orderby b.BankNameEn, a.AccountRef
               select new LndPayingAccountDto(a.AccountId, b.BankNameEn, a.AccountRef, a.AccountNo, a.AccountTypeCode, a.ErpAccountCode))
            .ToListAsync(ct);

    /// <summary>The variables a CF_FINANCE run starts with.</summary>
    public static Dictionary<string, string?> Variables(LndLoan loan) => new()
    {
        ["entity_id"] = loan.LoanId.ToString(CultureInfo.InvariantCulture),
        ["entity_ref"] = loan.LoanNumber,
        ["entity_type"] = "LND_LOAN",
        ["module_code"] = "LND",
    };
}
