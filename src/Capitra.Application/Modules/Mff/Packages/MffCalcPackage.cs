using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mff.Packages;

/// <summary>
/// NEW (not in Oracle): MFF_CALC_PKG. Page 80 calls run_application and build_schedule, but the package is not in the
/// FINOVA database. Designed in C# from the Fund calculator: it reproduces the three reference cases in
/// MFF_CALCULATION_RUNS (SOURCE_SAMPLE) to the pound.
/// </summary>
/// <remarks>
/// The borrower pays a share of the monthly income (max payment to income %), growing once a year by the growth % for
/// the growth years. The basic mortgage is the present value of those payments at the mortgage rate; a monthly payment
/// subsidy (buydown) adds its present value. The loan is that maximum, limited by the property value less the
/// downpayment assistance and the minimum borrower downpayment.
/// </remarks>
public sealed class MffCalcPackage(ICapitraDbContext db, MffParamPackage param, IClock clock)
{
    public sealed record Inputs(decimal Income, decimal PropertyValue, decimal RatePct, decimal TermYears, decimal GrowthPct, decimal GrowthYears,
        decimal MaxPaymentIncomePct, decimal DownpaymentAssist, decimal MonthlySubsidy, int SubsidyMonths, decimal DiscountRatePct,
        decimal MinDownpaymentPct, decimal MaxDownpaymentPct, decimal MaxSubsidyAmount);

    public sealed record Outputs(decimal BasicMortgage, decimal MaxMortgage, decimal PvBuydownSubsidy, decimal DownpaymentAssist, decimal TotalSubsidy,
        decimal MortgageLoan, decimal InitialPayment, decimal LtvPct, decimal BorrowerDownpayment, decimal MaxPropertyValue, decimal PropertyValue,
        decimal MinPropertyValue, bool IsNegativeAmortization);

    /// <summary>The borrower's payment of month m (from 1): the initial payment grown once a year, for at most the growth years.</summary>
    public static decimal GrowingPayment(decimal initial, decimal growthPct, decimal growthYears, int month) =>
        initial * (decimal)Math.Pow((double)(1 + growthPct / 100), (double)Math.Min((month - 1) / 12, growthYears));

    /// <summary>The present value at the annual rate of the payments of months 1..n given by pay(m).</summary>
    public static decimal PresentValue(decimal annualRatePct, int months, Func<int, decimal> pay)
    {
        var r = (double)annualRatePct / 1200;
        double s = 0;
        for (var m = 1; m <= months; m++)
            s += (double)pay(m) / Math.Pow(1 + r, m);
        return (decimal)s;
    }

    /// <summary>
    /// What is left of the loan at maturity after the payments (the bullet payment). The Fund flags negative amortization when
    /// the payments do not repay the loan; a balance that only grows in the first years is not flagged (reference cases 2001, 2002).
    /// </summary>
    public static decimal Rest(decimal loan, decimal annualRatePct, int months, Func<int, decimal> pay)
    {
        var r = annualRatePct / 1200;
        var b = loan;
        for (var m = 1; m <= months; m++)
            b += R(b * r) - pay(m);
        return Math.Max(0, b);
    }

    /// <summary>The calculation itself, without the database.</summary>
    public static Outputs Calculate(Inputs i)
    {
        var months = (int)Math.Round(i.TermYears * 12);
        var payment = i.Income * i.MaxPaymentIncomePct / 100;
        var basic = PresentValue(i.RatePct, months, m => GrowingPayment(payment, i.GrowthPct, i.GrowthYears, m));
        var subMonths = Math.Min(i.SubsidyMonths, months);
        var subsidyAtRate = PresentValue(i.RatePct, subMonths, _ => i.MonthlySubsidy);
        var pvBuydown = PresentValue(i.DiscountRatePct > 0 ? i.DiscountRatePct : i.RatePct, subMonths, _ => i.MonthlySubsidy);
        var maxMortgage = basic + subsidyAtRate;
        var assist = Math.Max(0, Math.Min(i.DownpaymentAssist, i.MaxSubsidyAmount - pvBuydown));
        var loan = Math.Max(0, Math.Min(maxMortgage, i.PropertyValue - assist - i.PropertyValue * i.MinDownpaymentPct / 100));
        if (loan < maxMortgage && basic > 0)
            payment *= Math.Max(0, loan - subsidyAtRate) / basic;  // the payment that repays the smaller loan
        var negative = Rest(loan, i.RatePct, months, m => R(GrowingPayment(payment, i.GrowthPct, i.GrowthYears, m)) + (m <= subMonths ? i.MonthlySubsidy : 0)) > 1;
        var financing = maxMortgage + assist;
        return new Outputs(
            R(basic), R(maxMortgage), R(pvBuydown), R(assist), R(assist + pvBuydown), R(loan), R(payment),
            i.PropertyValue > 0 ? R(loan / i.PropertyValue * 100) : 0, R(i.PropertyValue - loan - assist),
            i.MaxDownpaymentPct < 100 ? R(financing / (1 - i.MaxDownpaymentPct / 100)) : 0, R(i.PropertyValue),
            i.MinDownpaymentPct < 100 ? R(financing / (1 - i.MinDownpaymentPct / 100)) : 0, negative);
    }

    /// <summary>NEW run_application: calculates for the application with the page's inputs (blank: the program parameters) and saves the run.</summary>
    public async Task<long> RunApplicationAsync(long applicationId, decimal? annualRatePct, decimal? termYears, decimal? annualGrowthPct, decimal? growthYears,
        decimal? maxPaymentIncomePct, decimal? downpaymentAssistance, decimal? monthlyPaymentSubsidy, int? subsidyMonths, decimal? discountRatePct,
        string? sourceReference, CancellationToken ct)
    {
        var a = await db.Set<MffApplication>().AsNoTracking().FirstOrDefaultAsync(x => x.ApplicationId == applicationId, ct)
            ?? throw new ApexProcessException("ORA-20002: طلب التمويل غير موجود.");
        if ((a.MonthlyIncomeTotal ?? 0) <= 0)
            throw new ApexProcessException("ORA-20012: الدخل الشهري للطلب غير مسجل.");
        if ((a.PropertyPrice ?? 0) <= 0)
            throw new ApexProcessException("ORA-20013: قيمة الوحدة للطلب غير مسجلة.");
        var today = DateOnly.FromDateTime(clock.UtcNow);
        Task<decimal?> P(string code, decimal? def) => param.GetNumberAsync(a.ProgramVersionId, code, a.FinancierId, today, def, ct);

        var rate = annualRatePct ?? await P("HISTORICAL_RATE_PCT", null)
            ?? throw new ApexProcessException("ORA-20014: Annual rate is required.");
        var term = termYears ?? await P("MAX_TERM_YEARS", null)
            ?? throw new ApexProcessException("ORA-20015: Term years is required.");
        var growth = annualGrowthPct ?? 0;
        var input = new Inputs(
            a.MonthlyIncomeTotal!.Value, a.PropertyPrice!.Value, rate, term, growth, growthYears ?? (growth > 0 ? term : 0),
            maxPaymentIncomePct ?? await P("MAX_PAYMENT_INCOME_PCT", 25) ?? 25, downpaymentAssistance ?? 0, monthlyPaymentSubsidy ?? 0, subsidyMonths ?? 0,
            discountRatePct ?? 0, await P("MIN_DOWNPAYMENT_PCT", 10) ?? 10, await P("MAX_DOWNPAYMENT_PCT", 50) ?? 50, await P("MAX_SUBSIDY_AMOUNT", 60000) ?? 60000);
        var o = Calculate(input);

        var run = new MffCalculationRun
        {
            ApplicationId = applicationId,
            ProgramVersionId = a.ProgramVersionId,
            FinancierId = a.FinancierId,
            RunType = "APPLICATION",
            InputIncome = input.Income,
            InputPropertyValue = input.PropertyValue,
            InputDownpayment = a.DownpaymentAmount,
            MortgageRatePct = input.RatePct,
            TermYears = input.TermYears,
            PaymentGrowthYears = input.GrowthYears,
            WageIncreasePct = input.GrowthPct,
            MaxPaymentIncomePct = input.MaxPaymentIncomePct,
            MinDownpaymentPct = input.MinDownpaymentPct,
            MaxDownpaymentPct = input.MaxDownpaymentPct,
            MaxSubsidyAmount = input.MaxSubsidyAmount,
            DiscountFactorPct = input.DiscountRatePct,
            InputMonthlySubsidy = input.MonthlySubsidy,
            InputSubsidyMonths = input.SubsidyMonths,
            OutBasicMortgage = o.BasicMortgage,
            OutMaxMortgage = o.MaxMortgage,
            OutPvBuydownSubsidy = o.PvBuydownSubsidy,
            OutDownpaymentAssist = o.DownpaymentAssist,
            OutTotalSubsidy = o.TotalSubsidy,
            OutMortgageLoan = o.MortgageLoan,
            OutInitialPayment = o.InitialPayment,
            OutLtvPct = o.LtvPct,
            OutBorrowerDownpayment = o.BorrowerDownpayment,
            OutMaxPropertyValue = o.MaxPropertyValue,
            OutPropertyValue = o.PropertyValue,
            OutMinPropertyValue = o.MinPropertyValue,
            IsNegativeAmortization = o.IsNegativeAmortization,
            WarningMessageAr = o.IsNegativeAmortization ? "القسط الأول أقل من عائد الشهر الأول (استهلاك سلبي)." : null,
            CalcStatus = "COMPLETE",
            SourceReference = sourceReference,
        };
        db.Add(run);
        await db.SaveChangesAsync(ct);
        return run.CalcRunId;
    }

    /// <summary>NEW build_schedule: the run's monthly schedule (replaces it): borrower and subsidy payments, interest, principal, and a bullet for any rest.</summary>
    public async Task BuildScheduleAsync(long calcRunId, CancellationToken ct)
    {
        var run = await db.Set<MffCalculationRun>().AsNoTracking().FirstOrDefaultAsync(x => x.CalcRunId == calcRunId, ct)
            ?? throw new ApexProcessException("ORA-20016: Calculation run not found.");
        foreach (var x in await db.Set<MffCalculationSchedule>().Where(x => x.CalcRunId == calcRunId).ToListAsync(ct))
            db.Remove(x);

        var months = (int)Math.Round((run.TermYears ?? 0) * 12);
        var r = (run.MortgageRatePct ?? 0) / 1200;
        var open = run.OutMortgageLoan ?? 0;
        var first = DateOnly.FromDateTime(clock.UtcNow);
        first = new DateOnly(first.Year, first.Month, 1).AddMonths(1);
        for (var m = 1; m <= months; m++)
        {
            var borrower = R(GrowingPayment(run.OutInitialPayment ?? 0, run.WageIncreasePct ?? 0, run.PaymentGrowthYears ?? 0, m));
            var subsidy = m <= (run.InputSubsidyMonths ?? 0) ? run.InputMonthlySubsidy ?? 0 : 0;
            var interest = R(open * r);
            var principal = borrower + subsidy - interest;
            var bullet = 0m;
            if (m == months && open - principal > 0)
                bullet = R(open - principal);
            principal = R(Math.Min(open, principal + bullet));
            var close = R(open - principal);
            db.Add(new MffCalculationSchedule
            {
                CalcRunId = calcRunId,
                InstallmentNo = m,
                DueMonthDate = first.AddMonths(m - 1),
                OpeningPrincipal = open,
                BorrowerPayment = borrower,
                SubsidyPayment = subsidy,
                BulletPayment = bullet,
                InterestAmount = interest,
                PrincipalAmount = principal,
                ClosingPrincipal = close,
            });
            open = close;
        }

        await db.SaveChangesAsync(ct);
    }

    private static decimal R(decimal d) => Math.Round(d, 2, MidpointRounding.AwayFromZero);
}
