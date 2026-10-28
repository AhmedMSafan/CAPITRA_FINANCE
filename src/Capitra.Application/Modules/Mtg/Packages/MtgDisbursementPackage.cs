using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Chq.Packages;
using Capitra.Application.Modules.Gl.Journals;
using Capitra.Application.Modules.Mtg.Pages.Custom;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Chq;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Gl;
using Capitra.Domain.Modules.Mtg;
using Capitra.Domain.Modules.Trs;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Packages;

/// <summary>The figures page 217 sends to preview_json and complete (x01 to x13).</summary>
public sealed record MtgDisbursementInput(
    long PortfolioId, decimal? Admin, decimal? Legal, decimal? Valuation, DateOnly? DisbDate, long? BankAccountId,
    string? MarginMode, string? AdminMode, string? LegalMode, string? ValuationMode);

/// <summary>
/// FNV_PP_DISB_PKG: the disbursement of a purchased portfolio (page 217). The preview shows the cheques, the
/// effective interest schedule of each contract, the income release guide and the GL entries; complete books it in
/// one transaction: CRM clients, wallet cheques, schedules, the GL tran set and the workflow step.
/// </summary>
public sealed class MtgDisbursementPackage(
    ICapitraDbContext db, IClock clock, ICurrentUser user, ChqEventPackage events, GlPostingApi gl, MtgWorkflowSteps steps,
    AdmProductEventRuleService rules)
{
    private const string GlSource = "PURCHASED_PORTFOLIO";
    private const string GlCategory = "PP_DISBURSE";
    private const string Company = "01";
    private const string Branch = "HQ";
    private const string CostCenter = "4200";
    private const string BusinessLine = "MORTGAGE";
    private const string Future = "000";
    private const string BankGl = "110104";
    private const string WalletLocation = "COMPANY_VAULT";
    private const string DisbursementStep = "DISBURSEMENT";

    private static readonly string[] DoneStatuses =
        ["COMPLETED", "CANCELLED", "CANCELED", "CLOSED", "REJECTED", "APPROVED", "SKIPPED", "EXPIRED", "FORWARDED", "RETURNED"];

    private sealed class Chq
    {
        public long? SrcId;
        public int AccIx;
        public int LineNo;
        public string? ChequeNo;
        public string? BankName;
        public DateOnly? ChqDate;
        public DateOnly? Due;
        public decimal Amt;
        public decimal Pv;
        public decimal Days;
        public decimal Opening;
        public decimal Princ;
        public decimal Intr;
        public decimal Closing;
        public decimal Marg;
        public decimal Fee;
        public long? WalletId;
    }

    private sealed class Acc
    {
        public long AccId;
        public long InvId;
        public string? InvCode;
        public string? NameEn;
        public string? NameAr;
        public string? NatId;
        public string? Phone;
        public string? Email;
        public int I1;
        public int I2;
        public int N;
        public decimal Fv;
        public decimal Pv;
        public decimal Marg;
        public decimal Fee;
        public decimal Eir;
        public long? ClientId;
        public string? ClientNo;
        public string ClientNew = "N";
        public string ClientBl = "N";
    }

    private sealed record GlLine(int J, string Acct, decimal Dr, decimal Cr, string Descr);

    /// <summary>The package globals of one call.</summary>
    private sealed class State
    {
        public List<Chq> C = [];
        public List<Acc> A = [];
        public List<GlLine> Gls = [];
        public List<string> Blk = [];
        public List<string> Wrn = [];
        public string Mode = "PREVIEW";
        public long Pf;
        public string PfNo = string.Empty;
        public string? Cur;
        public string? Status;
        public string? Pvm;
        public decimal? Rate;
        public DateOnly? Base;
        public DateOnly Disb;
        public decimal FrPv;
        public decimal FrN;
        public decimal Margin;
        public decimal FrAdmin;
        public decimal FrLegal;
        public decimal FrVal;
        public decimal BoardPrice;
        public decimal Admin;
        public decimal Legal;
        public decimal Val;
        public decimal Fees;
        public string Mm = "PRORATE";
        public string Am = "PRORATE";
        public string Lm = "IMMEDIATE";
        public string Vm = "IMMEDIATE";
        public decimal DefMargin;
        public decimal ImmMargin;
        public decimal DefFees;
        public decimal ImmFees;
        public decimal Fv;
        public decimal Pv;
        public decimal Fund;
        public decimal Price;
        public long? BankAcc;
        public long? BankId;
        public string? BankName;
        public string? AccLabel;
        public string BankGlCode = BankGl;
        public string? BankNote;
        public MtgPurchasedPortfolioDisbursement? Done;
        public string? GlRule;
        public HashSet<string>? RuleCharges;
    }

    private string Me => user.UserName ?? string.Empty;

    // ------------------------------------------------------------------------------------------------ helpers
    private static ApexProcessException Fail(int code, string message) =>
        new("ORA-" + (-code).ToString(CultureInfo.InvariantCulture) + ": " + (message.Length > 2000 ? message[..2000] : message));

    /// <summary>to_char(nvl(p, 0), 'FM999G999G999G990D00').</summary>
    private static string Fmt(decimal? p) => (p ?? 0).ToString("#,##0.00", CultureInfo.InvariantCulture);

    private static decimal Round(decimal n, int d) => Math.Round(n, d, MidpointRounding.AwayFromZero);

    /// <summary>nvl(a, b), with '' as null as in Oracle.</summary>
    private static string? Nvl(string? a, string? b) => string.IsNullOrEmpty(a) ? (string.IsNullOrEmpty(b) ? null : b) : a;

    private static string? Substr(string? s, int n) => string.IsNullOrEmpty(s) ? null : s.Length <= n ? s : s[..n];

    private static string D(DateOnly? p) => p?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)!;

    private static string ModeOf(string? p, string def)
    {
        var l = (Nvl(p, def) ?? def).Trim().ToUpperInvariant();
        if (l is not ("PRORATE" or "IMMEDIATE" or "LATER"))
            throw Fail(-20950, "Unknown charge treatment " + p + ".");
        return l;
    }

    private static void SplitIncome(State g)
    {
        g.DefMargin = g.Mm == "PRORATE" ? g.Margin : 0;
        g.ImmMargin = g.Mm == "IMMEDIATE" ? g.Margin : 0;
        g.DefFees = (g.Am == "PRORATE" ? g.Admin : 0) + (g.Lm == "PRORATE" ? g.Legal : 0) + (g.Vm == "PRORATE" ? g.Val : 0);
        g.ImmFees = g.Fees - g.DefFees;
    }

    private IQueryable<GlCoaSegmentValue> Natural() =>
        from v in db.Set<GlCoaSegmentValue>()
        join s in db.Set<GlCoaSegment>() on v.CoaSegmentId equals s.CoaSegmentId
        where s.SegmentCode == "NATURAL"
        select v;

    private readonly Dictionary<string, string?> names = [];

    private async Task<string?> AcctNameAsync(string? code, CancellationToken ct)
    {
        if (code is null)
            return null;
        if (!names.TryGetValue(code, out var name))
        {
            name = await Natural().Where(v => v.ValueCode == code).MaxAsync(v => (string?)v.ValueNameEn, ct);
            names[code] = name;
        }

        return name;
    }

    private async Task<string?> IssuerCodeAsync(string? name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;
        var n = name.Trim().ToUpper();
#pragma warning disable CA1862 // EF Core translates ToUpper to SQL upper(), as the PL/SQL compared.
        return await db.Set<ChqLkpIssuerBank>().Where(b => b.BankNameEn.Trim().ToUpper() == n && b.IsActive).MinAsync(b => (string?)b.BankCode, ct);
#pragma warning restore CA1862
    }

    /// <summary>Distinct investors (an investor may hold several contracts); counts the new and the existing CRM clients.</summary>
    private static (int N, int New, int Old) InvestorCounts(State g)
    {
        var seen = new HashSet<long>();
        int nn = 0, no = 0;
        foreach (var a in g.A)
        {
            if (!seen.Add(a.InvId))
                continue;
            if (a.ClientNew == "Y")
                nn++;
            else
                no++;
        }

        return (seen.Count, nn, no);
    }

    // ------------------------------------------------------------------------------------------------ TRAN_SET_ID
    private async Task<string> NextTranSetIdAsync(string portfolioNo, DateOnly date, CancellationToken ct)
    {
        var year = date.Year;
        var c = await db.Set<MtgPurchasedPortfolioTranSetCounter>().FirstOrDefaultAsync(x => x.PortfolioNo == portfolioNo && x.YearNo == year, ct);
        if (c is null)
        {
            c = new MtgPurchasedPortfolioTranSetCounter { PortfolioNo = portfolioNo, YearNo = year, LastNo = 9999 };
            db.Set<MtgPurchasedPortfolioTranSetCounter>().Add(c);
        }

        c.LastNo++;
        await db.SaveChangesAsync(ct);
        return portfolioNo + "-" + year.ToString(CultureInfo.InvariantCulture) + "-" + c.LastNo.ToString("00000", CultureInfo.InvariantCulture);
    }

    // ------------------------------------------------------------------------------------------------ bank account
    private async Task LoadBankAsync(State g, long? acc, CancellationToken ct)
    {
        g.BankAcc = acc;
        g.BankId = null;
        g.BankName = null;
        g.AccLabel = null;
        g.BankGlCode = BankGl;
        g.BankNote = null;
        if (acc is null)
            return;
        var r = await (from a in db.Set<TrsBankAccount>()
                       join b in db.Set<TrsBank>() on a.BankId equals b.BankId
                       where a.AccountId == acc
                       select new { a.BankId, b.BankNameEn, Label = a.AccountNo + " - " + a.AccountRef, a.ErpAccountCode }).FirstOrDefaultAsync(ct);
        if (r is null)
        {
            g.BankAcc = null;
            return;
        }

        g.BankId = r.BankId;
        g.BankName = r.BankNameEn;
        g.AccLabel = r.Label;
        var erp = string.IsNullOrWhiteSpace(r.ErpAccountCode) ? null : r.ErpAccountCode.Trim();
        if (erp is null)
        {
            g.BankNote = "The bank account has no GL account code, so the payment is credited to " + BankGl + ".";
        }
        else if (await Natural().AnyAsync(v => v.ValueCode == erp && v.IsEnabled && !v.IsSummary && v.IsPostingAllowed, ct))
        {
            g.BankGlCode = erp;
        }
        else
        {
            g.BankNote = "The bank account GL code " + erp + " is not a posting account in the chart of accounts, so the payment is credited to " + BankGl + ".";
        }
    }

    // ------------------------------------------------------------------------------------------------ schedule of one contract
    private static double PvAt(State g, Acc a, double i)
    {
        var s = 0d;
        for (var k = a.I1; k <= a.I2; k++)
            s += (double)g.C[k].Amt / Math.Pow(1 + i, (double)g.C[k].Days / 365);
        return s;
    }

    /// <summary>
    /// The effective annual rate that discounts the contract cheques (from the disbursement date) back to their present
    /// value; each cheque = principal + interest, and the interest adds up to the funding recovery of the contract.
    /// Margin and fees of the contract are released in the same proportion as the interest.
    /// </summary>
    private static void Schedule(State g, Acc a)
    {
        double lo = 0, hi = 1, i = 0;
        decimal prev = 0, sm = 0, sf = 0;
        var totInt = a.Fv - a.Pv;
        var pv = (double)a.Pv;
        if (totInt > 0.004m && a.Pv > 0)
        {
            while (PvAt(g, a, hi) > pv && hi < 1000000)
                hi *= 2;
            for (var it = 1; it <= 120; it++)
            {
                var mid = (lo + hi) / 2;
                if (PvAt(g, a, mid) > pv)
                    lo = mid;
                else
                    hi = mid;
            }

            i = (lo + hi) / 2;
        }

        a.Eir = Round((decimal)i * 100, 6);
        var b = a.Pv;
        for (var k = a.I1; k <= a.I2; k++)
        {
            var c = g.C[k];
            c.Opening = b;
            if (k == a.I2)
            {
                c.Intr = Round(c.Amt - b, 2);
            }
            else
            {
                c.Intr = Round(b * (decimal)(Math.Pow(1 + i, (double)(c.Days - prev) / 365) - 1), 2);
                if (c.Intr > c.Amt)
                    c.Intr = c.Amt;
                if (c.Intr < 0)
                    c.Intr = 0;
            }

            c.Princ = c.Amt - c.Intr;
            b -= c.Princ;
            c.Closing = b;
            prev = c.Days;
            if (k == a.I2)
            {
                c.Marg = a.Marg - sm;
                c.Fee = a.Fee - sf;
            }
            else
            {
                var w = totInt > 0.004m ? c.Intr / totInt : a.Fv > 0 ? c.Amt / a.Fv : 0;
                c.Marg = Round(a.Marg * w, 2);
                c.Fee = Round(a.Fee * w, 2);
                sm += c.Marg;
                sf += c.Fee;
            }
        }
    }

    // ------------------------------------------------------------------------------------------------ calculation
    private async Task<State> ComputeAsync(MtgDisbursementInput p, CancellationToken ct)
    {
        var g = new State { Mode = "PREVIEW", Pf = p.PortfolioId };
        var pf = await db.Set<MtgPurchasedPortfolio>().AsNoTracking().Where(x => x.PortfolioId == p.PortfolioId)
            .Select(x => new { x.PortfolioNo, x.CurrencyCode, x.PortfolioStatusCode }).FirstOrDefaultAsync(ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        g.PfNo = pf.PortfolioNo;
        g.Cur = pf.CurrencyCode;
        g.Status = pf.PortfolioStatusCode;
        var today = DateOnly.FromDateTime(MtgAjax.CairoNow(clock));
        var fr = await db.Set<MtgPurchasedPortfolioFinancialReview>().AsNoTracking().FirstOrDefaultAsync(x => x.PortfolioId == p.PortfolioId, ct)
            ?? throw Fail(-20951, "The financial review of this portfolio was not saved.");
        g.Pvm = (Nvl(fr.PvMethod, "ANNUAL") ?? "ANNUAL").ToUpperInvariant();
        g.Rate = fr.PvRate;
        g.Base = fr.PvBaseDate ?? today;
        g.FrPv = fr.ChequesPvAmount ?? 0;
        g.FrN = fr.ChequeCount ?? 0;
        g.Margin = fr.MarginAmount ?? 0;
        g.FrAdmin = fr.IsAdminIncluded ? fr.AdminFinalAmount ?? 0 : 0;
        g.FrLegal = fr.IsLegalIncluded ? fr.LegalFinalAmount ?? 0 : 0;
        g.FrVal = fr.IsValuationIncluded ? fr.ValuationFinalAmount ?? 0 : 0;
        g.BoardPrice = fr.ProposedPurchaseValue ?? 0;
        g.Admin = Round(p.Admin ?? g.FrAdmin, 2);
        g.Legal = Round(p.Legal ?? g.FrLegal, 2);
        g.Val = Round(p.Valuation ?? g.FrVal, 2);
        // with a product event rule, only its charge components in effect are charged: the others are left out
        var (_, plan) = await EventPlanAsync(g.Pf, p.DisbDate ?? today, ct);
        if (plan is not null)
        {
            g.RuleCharges = [.. plan.ChargeCodes];
            if (!g.RuleCharges.Contains("ADMIN_EXPENSE"))
                g.FrAdmin = g.Admin = 0;
            if (!g.RuleCharges.Contains("LEGAL_FEE"))
                g.FrLegal = g.Legal = 0;
            if (!g.RuleCharges.Contains("VALUATION_FEE"))
                g.FrVal = g.Val = 0;
        }

        if (g.Admin < 0 || g.Legal < 0 || g.Val < 0)
            throw Fail(-20952, "Charges cannot be negative.");
        g.Fees = g.Admin + g.Legal + g.Val;
        g.Mm = ModeOf(p.MarginMode, "PRORATE");
        g.Am = ModeOf(p.AdminMode, "PRORATE");
        g.Lm = ModeOf(p.LegalMode, "IMMEDIATE");
        g.Vm = ModeOf(p.ValuationMode, "IMMEDIATE");
        SplitIncome(g);
        g.Disb = p.DisbDate ?? today;
        var r = (g.Rate ?? 0) / 100;

        var rows = await (from c in db.Set<MtgPurchasedPortfolioCheque>()
                          join a in db.Set<MtgPurchasedPortfolioAccount>() on c.PortfolioAccountId equals a.PortfolioAccountId
                          join i in db.Set<MtgInvestor>() on a.InvestorId equals i.InvestorId
                          where c.PortfolioId == p.PortfolioId && (a.DealDecisionCode ?? "SELECTED") == "SELECTED"
                          select new
                          {
                              c.ChequeId, a.PortfolioAccountId, a.InvestorId, c.ChequeNo, c.BankName, c.ChequeDate, c.DueDate, c.ChequeAmount,
                              i.InvestorCode, i.FullNameEn, i.FullNameAr, i.NationalId, i.PhoneNo, i.EmailAddress,
                          }).AsNoTracking().ToListAsync(ct);
        long prevAcc = -1;
        Acc? cur = null;
        foreach (var c in rows.OrderBy(x => x.PortfolioAccountId).ThenBy(x => x.DueDate is null).ThenBy(x => x.DueDate).ThenBy(x => x.ChequeId))
        {
            if (c.PortfolioAccountId != prevAcc)
            {
                prevAcc = c.PortfolioAccountId;
                cur = new Acc
                {
                    AccId = c.PortfolioAccountId,
                    InvId = c.InvestorId,
                    InvCode = c.InvestorCode,
                    NameEn = Nvl(c.FullNameEn, c.FullNameAr),
                    NameAr = Nvl(c.FullNameAr, c.FullNameEn),
                    NatId = c.NationalId,
                    Phone = c.PhoneNo,
                    Email = c.EmailAddress,
                    I1 = g.C.Count,
                    I2 = g.C.Count,
                };
                g.A.Add(cur);
            }

            var (dd, mm) = FinancialReviewMath.Period(c.DueDate, g.Base.Value);
            var k = new Chq
            {
                SrcId = c.ChequeId,
                AccIx = g.A.Count - 1,
                ChequeNo = c.ChequeNo,
                BankName = c.BankName,
                ChqDate = c.ChequeDate ?? c.DueDate,
                Due = c.DueDate,
                Amt = c.ChequeAmount,
                Pv = FinancialReviewMath.PresentValue(c.ChequeAmount, r, g.Pvm, dd, mm),
                Days = Math.Max((c.DueDate ?? g.Disb).DayNumber - g.Disb.DayNumber, 0),
            };
            g.C.Add(k);
            cur!.N++;
            cur.I2 = g.C.Count - 1;
            k.LineNo = cur.N;
            cur.Fv += k.Amt;
            cur.Pv += k.Pv;
            g.Fv += k.Amt;
            g.Pv += k.Pv;
        }

        g.Fund = g.Fv - g.Pv;
        g.Price = Round(g.Pv - g.Margin - g.Fees, 2);
        // deferred margin and fees shared between the contracts by present value; the last contract takes the rounding
        decimal am = 0, af = 0;
        for (var x = 0; x < g.A.Count; x++)
        {
            var a = g.A[x];
            if (x < g.A.Count - 1)
            {
                a.Marg = g.Pv > 0 ? Round(g.DefMargin * a.Pv / g.Pv, 2) : 0;
                a.Fee = g.Pv > 0 ? Round(g.DefFees * a.Pv / g.Pv, 2) : 0;
                am += a.Marg;
                af += a.Fee;
            }
            else
            {
                a.Marg = g.DefMargin - am;
                a.Fee = g.DefFees - af;
            }

            Schedule(g, a);
        }

        return g;
    }

    // ------------------------------------------------------------------------------------------------ CRM clients
    private static string ClientNo(Acc a) =>
        (a.InvCode ?? string.Empty).ToUpperInvariant().StartsWith("INV", StringComparison.Ordinal)
            ? a.InvCode!
            : "INV-" + (Nvl(a.InvCode, null) ?? a.InvId.ToString(CultureInfo.InvariantCulture));

    private async Task ResolveClientsAsync(State g, bool create, CancellationToken ct)
    {
        long? country = null, currency = null, status = null;
        if (create)
        {
            var cur = g.Cur?.Trim();
            country = await db.Set<CrmLkpCountry>().Where(x => x.CountryCode == "EG").MinAsync(x => (long?)x.CountryId, ct);
            currency = await db.Set<CrmLkpCurrency>().Where(x => x.CurrencyCode == cur).MinAsync(x => (long?)x.CurrencyId, ct);
            status = await db.Set<CrmLkpStatusCode>().Where(x => x.ModuleCode == "CLIENT" && x.StatusCode == "ACTIVE").MinAsync(x => (long?)x.StatusId, ct);
            if (country is null || currency is null || status is null)
                throw Fail(-20953, "CRM lookups are missing (country EG, currency " + g.Cur + " or client status ACTIVE).");
        }

        var first = new Dictionary<long, Acc>();
        foreach (var a in g.A)
        {
            if (first.TryGetValue(a.InvId, out var f))
            {
                a.ClientId = f.ClientId;
                a.ClientNo = f.ClientNo;
                a.ClientNew = f.ClientNew;
                a.ClientBl = f.ClientBl;
                continue;
            }

            first[a.InvId] = a;
            long? id = null;
            if (!string.IsNullOrWhiteSpace(a.NatId))
            {
                // any client with this national ID (active first), so no second client is created for the same person
                var nid = a.NatId.Trim();
                id = await db.Set<CrmClient>().Where(c => c.NationalId!.Trim() == nid)
                    .OrderBy(c => c.IsActive ? 0 : 1).ThenByDescending(c => c.ClientId).Select(c => (long?)c.ClientId).FirstOrDefaultAsync(ct);
            }

            if (id is null)
            {
                var tag = "FNV_INVESTOR:" + a.InvId.ToString(CultureInfo.InvariantCulture);
                id = await db.Set<CrmClient>().Where(c => c.Attribute1 == tag).MaxAsync(c => (long?)c.ClientId, ct);
            }

            if (id is not null)
            {
                var c = await db.Set<CrmClient>().FirstAsync(x => x.ClientId == id, ct);
                // an inactive client is activated again instead of creating a new one (on complete only, not in the preview)
                if (create && !c.IsActive)
                {
                    c.IsActive = true;
                    c.StatusId = status!.Value;
                }

                a.ClientId = id;
                a.ClientNo = c.ClientNumber;
                a.ClientNew = "N";
                a.ClientBl = c.IsBlacklisted ? "Y" : "N";
            }
            else if (create)
            {
                var c = new CrmClient
                {
                    ClientNumber = ClientNo(a),
                    ClientRefCode = a.InvCode,
                    ClientName = a.NameEn ?? string.Empty,
                    ClientNameAr = a.NameAr ?? string.Empty,
                    ShortName = Substr(a.NameEn, 100),
                    ClientType = "INDIVIDUAL",
                    LegalStructure = "INDIVIDUAL",
                    NationalId = a.NatId,
                    CountryId = country!.Value,
                    BaseCurrencyId = currency!.Value,
                    StatusId = status!.Value,
                    ClientCategory = "RETAIL",
                    ClientSegment = "PURCHASED_PORTFOLIO",
                    MobileNumber = a.Phone,
                    Email = a.Email,
                    OnboardingDate = g.Disb,
                    IsBlacklisted = false,
                    IsRelatedParty = false,
                    IsActive = true,
                    Attribute1 = "FNV_INVESTOR:" + a.InvId.ToString(CultureInfo.InvariantCulture),
                    Attribute2 = "PORTFOLIO:" + g.PfNo,
                    Notes = "Investor of purchased portfolio " + g.PfNo + ", created by the disbursement step.",
                };
                db.Set<CrmClient>().Add(c);
                await db.SaveChangesAsync(ct);
                a.ClientId = c.ClientId;
                a.ClientNo = c.ClientNumber;
                a.ClientNew = "Y";
                a.ClientBl = "N";
            }
            else
            {
                a.ClientId = null;
                a.ClientNo = ClientNo(a);
                a.ClientNew = "Y";
                a.ClientBl = "N";
            }
        }
    }

    // ------------------------------------------------------------------------------------------------ checks
    private async Task ChecksAsync(State g, CancellationToken ct)
    {
        g.Blk.Clear();
        g.Wrn.Clear();
        if (g.C.Count == 0)
            g.Blk.Add("There are no cheques for the investors included in the deal.");
        if (g.C.Count != g.FrN || Math.Abs(g.Pv - g.FrPv) > 0.01m)
        {
            g.Blk.Add("The cheques changed after the financial review: present value is now " + Fmt(g.Pv) + " against " + Fmt(g.FrPv)
                + " reviewed (" + g.C.Count.ToString(CultureInfo.InvariantCulture) + " cheques against " + Num(g.FrN) + "). The financial review must be saved again.");
        }

        if (g.Price <= 0)
            g.Blk.Add("The amount payable to the developer must be above zero.");
        int unmatched = 0, inWallet = 0;
        string? list = null;
        foreach (var c in g.C)
        {
            var code = await IssuerCodeAsync(c.BankName, ct);
            if (code is null)
                unmatched++;
            var key = code ?? "~";
            if (await db.Set<ChqCheque>().AnyAsync(x => x.ChequeNo == c.ChequeNo && (x.IssuerBankCode ?? "~") == key, ct))
            {
                inWallet++;
                if (inWallet <= 5)
                    list = list is null ? c.ChequeNo : list + ", " + c.ChequeNo;
            }
        }

        if (inWallet > 0)
            g.Blk.Add(inWallet.ToString(CultureInfo.InvariantCulture) + " cheque(s) are already in the cheque wallet with the same number and bank: " + list + ".");
        await ResolveClientsAsync(g, false, ct);
        foreach (var a in g.A)
        {
            if (a.ClientBl != "Y")
                continue;
            var m = "Investor " + a.NameEn + " is blacklisted in CRM (client " + a.ClientNo + ").";
            if (!g.Blk.Contains(m))
                g.Blk.Add(m);
        }

        if (unmatched > 0)
            g.Wrn.Add(unmatched.ToString(CultureInfo.InvariantCulture) + " cheque(s) have a bank name that is not in the wallet issuer bank list; they are stored with the bank name only.");
        if (g.BankNote is not null)
            g.Wrn.Add(g.BankNote);
        if ("LATER" == g.Mm || "LATER" == g.Am || "LATER" == g.Lm || "LATER" == g.Vm)
            g.Blk.Add("Collect later is not available yet. Choose Pro-rate or Immediate for every charge.");
        if (g.Mm == "IMMEDIATE" && g.Margin > 0)
            g.Wrn.Add("The margin is recognised as income immediately. IFRS 9 and EAS 47 require it to be spread over the cheque dates by the effective interest method.");
        if (g.Admin != g.FrAdmin || g.Legal != g.FrLegal || g.Val != g.FrVal)
            g.Wrn.Add("Charges differ from the financial review: the amount payable to the developer changes from " + Fmt(g.BoardPrice) + " to " + Fmt(g.Price) + ".");
    }

    private static string Num(decimal n) => (n / 1.000000000000000000000000000000000m).ToString(CultureInfo.InvariantCulture);

    // ------------------------------------------------------------------------------------------------ GL
    private static void AddGl(State g, int j, string acct, decimal dr, decimal cr, string d)
    {
        if (dr == 0 && cr == 0)
            return;
        g.Gls.Add(new GlLine(j, acct, dr, cr, d));
    }

    /// <summary>
    /// The GL entries. When the Disbursement step of the portfolio's workflow raises a business event (finova:eventCode),
    /// they come from the product event rule of MTG_PORTFOLIO_PURCHASING and that event; otherwise the fixed entries below.
    /// A missing rule, or a charge that is not on it, is a blocker (strict) or falls back to the fixed entries (saved view).
    /// </summary>
    private async Task BuildGlAsync(State g, CancellationToken ct, bool strict = true)
    {
        var (ev, plan) = await EventPlanAsync(g.Pf, g.Disb, ct);
        if (ev is null)
        {
            BuildGl(g);
            return;
        }

        var problems = new List<string>();
        if (plan is null)
            problems.Add("No active product event rule for product " + AdmProduct.MortgagePortfolioPurchasingCode + " and business event " + ev + " on "
                         + D(g.Disb) + ". Add it on Admin Setup > Product Event Rules.");
        else
        {
            foreach (var (code, amount, _) in Charges(g))
                if (amount != 0 && !plan.ChargeCodes.Contains(code))
                    problems.Add("Charge " + code + " (" + Fmt(amount) + ") is not a charge component of product event rule " + plan.RuleCode + ".");
            foreach (var code in plan.ChargeCodes.Where(c => Charges(g).All(x => x.Code != c)))
                problems.Add("Charge component " + code + " of product event rule " + plan.RuleCode + " has no amount at disbursement.");
        }

        if (problems.Count > 0)
        {
            if (!strict)
            {
                BuildGl(g);
                return;
            }

            g.Gls.Clear();
            g.Blk.AddRange(problems.Where(m => !g.Blk.Contains(m)));
            return;
        }

        g.Gls.Clear();
        foreach (var l in ProductEventPosting.Build(plan!, l => AmountOf(g, l), g.BankGlCode))
            g.Gls.Add(new GlLine(l.JournalNo, l.Account, l.Dr, l.Cr, l.Description));
        g.GlRule = plan!.RuleCode;
    }

    /// <summary>
    /// The business event the Disbursement step of the portfolio's workflow raises (finova:eventCode), null when none,
    /// and the product event rule of MTG_PORTFOLIO_PURCHASING for it on the date, null when there is none.
    /// </summary>
    private async Task<(string? Event, ProductEventPlan? Plan)> EventPlanAsync(long portfolioId, DateOnly date, CancellationToken ct)
    {
        var xml = await (from p in db.Set<MtgPurchasedPortfolio>()
                         join i in db.Set<WfeWorkflowInstance>() on p.WorkflowInstanceId equals i.WorkflowInstanceId
                         join d in db.Set<WfeWorkflowDefinition>() on i.WorkflowDefinitionId equals d.WorkflowDefinitionId
                         where p.PortfolioId == portfolioId
                         select d.BpmnXml).FirstOrDefaultAsync(ct);
        var ev = xml is null ? null : BpmnModel.StepByCode(xml, DisbursementStep)?.EventCode?.Trim();
        if (string.IsNullOrEmpty(ev))
            return (null, null);
        return (ev, await rules.PlanAsync(AdmProduct.MortgagePortfolioPurchasingCode, ev, date, ct));
    }

    /// <summary>The charges of a disbursement with their final amount and income treatment, by ADM_CHARGES code.</summary>
    private static (string Code, decimal Amount, string Mode)[] Charges(State g) =>
        [("ADMIN_EXPENSE", g.Admin, g.Am), ("LEGAL_FEE", g.Legal, g.Lm), ("VALUATION_FEE", g.Val, g.Vm)];

    /// <summary>The disbursement figure a GL line of the rule posts.</summary>
    private static decimal AmountOf(State g, ProductEventGlLine l)
    {
        if (l.ChargeCode is not null)
        {
            var (_, amount, mode) = Charges(g).First(x => x.Code == l.ChargeCode);
            return l.AmountCode switch
            {
                "CHARGE_AMOUNT" => amount,
                "CHARGE_DEFERRED" => mode == "PRORATE" ? amount : 0,
                "CHARGE_IMMEDIATE" => mode == "IMMEDIATE" ? amount : 0,
                _ => 0,
            };
        }

        return l.AmountCode switch
        {
            "FACE_VALUE" => g.Fv,
            "DISCOUNT" => g.Fund,
            "MARGIN_DEFERRED" => g.DefMargin,
            "MARGIN_IMMEDIATE" => g.ImmMargin,
            "PURCHASE_PRICE" => g.Price,
            _ => 0,
        };
    }

    private static void BuildGl(State g)
    {
        g.Gls.Clear();
        AddGl(g, 1, "120401", g.Fv, 0, "Investor cheques - face value");
        AddGl(g, 1, "120402", 0, g.Fund, "Unearned income - funding recovery");
        AddGl(g, 1, "120403", 0, g.DefMargin, "Unearned income - margin (pro-rated)");
        AddGl(g, 1, "120404", 0, g.DefFees, "Unearned income - fees (pro-rated)");
        AddGl(g, 1, "450102", 0, g.ImmMargin, "Margin income - recognised on disbursement");
        AddGl(g, 1, "450103", 0, g.ImmFees, "Fee income - recognised on disbursement");
        AddGl(g, 1, "230103", 0, g.Price, "Payable to developer - purchase price");
        AddGl(g, 2, "230103", g.Price, 0, "Payable to developer - settled");
        AddGl(g, 2, g.BankGlCode, 0, g.Price, "Payment to developer");
    }

    private static string Payload(State g, string ts, string? comment, string? reference)
    {
        var o = new JsonObject
        {
            ["tran_set_id"] = ts,
            ["source"] = GlSource,
            ["category"] = GlCategory,
            ["accounting_date"] = D(g.Disb),
        };
        if (!string.IsNullOrEmpty(g.Cur))
            o["currency"] = g.Cur.Trim();
        o["batch_name"] = "Purchased portfolio " + g.PfNo + " - booking and disbursement";
        o["description"] = Substr(comment, 1000);
        var js = new JsonArray();
        for (var jn = 1; jn <= 2; jn++)
        {
            var ls = new JsonArray();
            foreach (var l in g.Gls.Where(x => x.J == jn))
            {
                ls.Add(new JsonObject
                {
                    ["company"] = Company,
                    ["branch"] = Branch,
                    ["cost_center"] = CostCenter,
                    ["account"] = l.Acct,
                    ["business_line"] = BusinessLine,
                    ["future1"] = Future,
                    ["future2"] = Future,
                    ["dr"] = l.Dr,
                    ["cr"] = l.Cr,
                    ["description"] = l.Descr,
                    ["reference_no"] = ts,
                });
            }

            js.Add(new JsonObject
            {
                ["name"] = jn == 1 ? "Portfolio " + g.PfNo + " - booking" : "Portfolio " + g.PfNo + " - payment to developer",
                ["description"] = jn == 1
                    ? "Purchased portfolio " + g.PfNo + ": investor cheques received and unearned income"
                    : "Purchased portfolio " + g.PfNo + ": purchase price paid, reference " + reference,
                ["reference_no"] = ts,
                ["source_document_type"] = "PURCHASED_PORTFOLIO",
                ["source_document_id"] = g.Pf.ToString(CultureInfo.InvariantCulture),
                ["source_document_no"] = g.PfNo,
                ["source_transaction_date"] = D(g.Disb),
                ["lines"] = ls,
            });
        }

        o["journals"] = js;
        return o.ToJsonString();
    }

    private static string? Str(JsonElement r, string name) =>
        r.ValueKind == JsonValueKind.Object && r.TryGetProperty(name, out var v) && v.ValueKind != JsonValueKind.Null
            ? v.ValueKind == JsonValueKind.String ? v.GetString() : v.GetRawText()
            : null;

    private static string? FirstError(JsonElement r)
    {
        if (r.TryGetProperty("errors", out var a) && a.ValueKind == JsonValueKind.Array && a.GetArrayLength() > 0)
            return Str(a[0], "message");
        return null;
    }

    // ------------------------------------------------------------------------------------------------ saved disbursement
    private async Task<State?> LoadDoneAsync(long pfId, CancellationToken ct)
    {
        var done = await db.Set<MtgPurchasedPortfolioDisbursement>().AsNoTracking().FirstOrDefaultAsync(x => x.PortfolioId == pfId, ct);
        if (done is null)
            return null;
        var g = new State { Mode = "DONE", Pf = pfId, Done = done };
        var pf = await db.Set<MtgPurchasedPortfolio>().AsNoTracking().Where(x => x.PortfolioId == pfId)
            .Select(x => new { x.PortfolioNo, x.CurrencyCode, x.PortfolioStatusCode }).FirstAsync(ct);
        g.PfNo = pf.PortfolioNo;
        g.Cur = pf.CurrencyCode;
        g.Status = pf.PortfolioStatusCode;
        g.Disb = done.DisbursementDate;
        g.Pvm = done.PvMethod;
        g.Rate = done.PvRate;
        g.Base = done.PvBaseDate;
        g.Fv = done.FaceAmount ?? 0;
        g.Pv = done.PvAmount ?? 0;
        g.Fund = done.FundingRecoveryAmount ?? 0;
        g.FrPv = done.PvAmount ?? 0;
        g.FrN = done.ChequeCount ?? 0;
        g.Margin = done.MarginAmount ?? 0;
        g.Admin = done.AdminFinalAmount ?? 0;
        g.Legal = done.LegalFinalAmount ?? 0;
        g.Val = done.ValuationFinalAmount ?? 0;
        g.FrAdmin = done.AdminFrAmount ?? 0;
        g.FrLegal = done.LegalFrAmount ?? 0;
        g.FrVal = done.ValuationFrAmount ?? 0;
        g.Fees = done.FeesAmount ?? 0;
        g.Price = done.PayableAmount ?? 0;
        g.BoardPrice = done.BoardPriceAmount ?? 0;
        g.Mm = Nvl(done.MarginTreatment, "PRORATE")!;
        g.Am = Nvl(done.AdminTreatment, "PRORATE")!;
        g.Lm = Nvl(done.LegalTreatment, "PRORATE")!;
        g.Vm = Nvl(done.ValuationTreatment, "PRORATE")!;
        SplitIncome(g);
        await LoadBankAsync(g, done.BankAccountId, ct);
        g.BankGlCode = done.BankGlAccount!;
        g.BankNote = null;
        var rows = await (from s in db.Set<MtgPurchasedPortfolioRepaymentSchedule>()
                          join i in db.Set<MtgInvestor>() on s.InvestorId equals i.InvestorId
                          join c in db.Set<CrmClient>() on s.ClientId equals c.ClientId into cs
                          from c in cs.DefaultIfEmpty()
                          where s.DisbursementId == done.DisbursementId
                          orderby s.PortfolioAccountId, s.LineNo
                          select new { s, i.InvestorCode, i.FullNameEn, i.FullNameAr, i.NationalId, ClientNumber = c == null ? null : c.ClientNumber })
            .AsNoTracking().ToListAsync(ct);
        long prevAcc = -1;
        Acc? cur = null;
        foreach (var r in rows)
        {
            var s = r.s;
            if (s.PortfolioAccountId != prevAcc)
            {
                prevAcc = s.PortfolioAccountId;
                cur = new Acc
                {
                    AccId = s.PortfolioAccountId,
                    InvId = s.InvestorId ?? 0,
                    InvCode = r.InvestorCode,
                    NameEn = Nvl(r.FullNameEn, r.FullNameAr),
                    NameAr = Nvl(r.FullNameAr, r.FullNameEn),
                    NatId = r.NationalId,
                    I1 = g.C.Count,
                    I2 = g.C.Count,
                    Eir = s.EirRate ?? 0,
                    ClientId = s.ClientId,
                    ClientNo = r.ClientNumber,
                    ClientNew = s.IsClientCreated ? "Y" : "N",
                };
                g.A.Add(cur);
            }

            g.C.Add(new Chq
            {
                SrcId = s.SourceChequeId,
                AccIx = g.A.Count - 1,
                LineNo = s.LineNo ?? 0,
                ChequeNo = s.ChequeNo,
                BankName = s.BankName,
                Due = s.DueDate,
                Amt = s.ChequeAmount ?? 0,
                Pv = s.PvAmount ?? 0,
                Days = s.DaysFromDisbursement ?? 0,
                Opening = s.OpeningBalance ?? 0,
                Princ = s.PrincipalAmount ?? 0,
                Intr = s.InterestAmount ?? 0,
                Closing = s.ClosingBalance ?? 0,
                Marg = s.MarginRelease ?? 0,
                Fee = s.FeeRelease ?? 0,
                WalletId = s.WalletChequeId,
            });
            cur!.N++;
            cur.I2 = g.C.Count - 1;
            cur.Fv += s.ChequeAmount ?? 0;
            cur.Pv += s.PvAmount ?? 0;
            cur.Marg += s.MarginRelease ?? 0;
            cur.Fee += s.FeeRelease ?? 0;
        }

        await BuildGlAsync(g, ct, strict: false);
        return g;
    }

    // ------------------------------------------------------------------------------------------------ JSON for the page

    /// <summary>An apex_json object: a null value is not written; a number is written as Oracle shows it.</summary>
    private sealed class J : Dictionary<string, object?>
    {
        public new void Add(string key, object? value)
        {
            if (value is not null)
                this[key] = value is decimal d ? d / 1.000000000000000000000000000000000m : value;
        }
    }

    private sealed record Inc(DateOnly Due)
    {
        public int N;
        public decimal Fund;
        public decimal Marg;
        public decimal Fee;
        public string Status = "PENDING";
        public string? Ts;
    }

    private async Task<J> ToJsonAsync(State g, CancellationToken ct)
    {
        var inc = new SortedDictionary<string, Inc>(StringComparer.Ordinal);
        foreach (var c in g.C)
        {
            var due = c.Due ?? g.Disb;
            var key = due.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            if (!inc.TryGetValue(key, out var x))
                inc[key] = x = new Inc(due);
            x.N++;
            x.Fund += c.Intr;
            x.Marg += c.Marg;
            x.Fee += c.Fee;
        }

        if (g.Mode == "DONE")
        {
            var rows = await db.Set<MtgPurchasedPortfolioIncomeSchedule>().AsNoTracking().Where(x => x.DisbursementId == g.Done!.DisbursementId)
                .Select(x => new { x.DueDate, x.PostingStatus, x.TranSetId }).ToListAsync(ct);
            foreach (var r in rows)
            {
                if (inc.TryGetValue(r.DueDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture), out var x))
                {
                    x.Status = r.PostingStatus;
                    x.Ts = r.TranSetId;
                }
            }
        }

        var (inv, nn, no) = InvestorCounts(g);
        var o = new J
        {
            { "success", true },
            { "mode", g.Mode },
            { "portfolio", new J { { "id", (decimal)g.Pf }, { "no", g.PfNo }, { "currency", g.Cur?.Trim() }, { "status", g.Status } } },
            {
                "review", new J
                {
                    { "method", g.Pvm }, { "rate", g.Rate }, { "baseDate", g.Base is null ? null : D(g.Base) }, { "pv", g.FrPv }, { "cheques", g.FrN },
                    { "admin", g.FrAdmin }, { "legal", g.FrLegal }, { "valuation", g.FrVal }, { "price", g.BoardPrice },
                }
            },
            {
                "totals", new J
                {
                    { "disbDate", D(g.Disb) }, { "cheques", g.C.Count }, { "contracts", g.A.Count }, { "investors", inv },
                    { "clientsNew", nn }, { "clientsExisting", no }, { "face", g.Fv }, { "pv", g.Pv }, { "funding", g.Fund },
                    { "margin", g.Margin }, { "admin", g.Admin }, { "legal", g.Legal }, { "valuation", g.Val }, { "fees", g.Fees },
                    { "marginDeferred", g.DefMargin }, { "marginImmediate", g.ImmMargin }, { "feesDeferred", g.DefFees },
                    { "feesImmediate", g.ImmFees }, { "payable", g.Price },
                }
            },
            { "treatment", new J { { "M", g.Mm }, { "A", g.Am }, { "L", g.Lm }, { "V", g.Vm } } },
            {
                "bank", new J
                {
                    { "accountId", g.BankAcc }, { "bankId", g.BankId }, { "bankName", g.BankName }, { "account", g.AccLabel },
                    { "gl", g.BankGlCode }, { "glName", await AcctNameAsync(g.BankGlCode, ct) },
                }
            },
        };
        if (g.Mode == "DONE")
        {
            var d = g.Done!;
            o.Add("done", new J
            {
                { "tranSetId", d.TranSetId }, { "disbursementId", d.DisbursementId }, { "paymentRef", d.PaymentRef }, { "glBatchNo", d.GlBatchNo },
                { "glStatus", d.GlStatus }, { "comments", d.Comments }, { "by", d.CreatedBy },
                { "on", MtgAjax.Cairo(d.CreatedAt)?.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture) },
                { "clientsCreated", d.ClientsCreated }, { "clientsMatched", d.ClientsMatched },
            });
        }

        o.Add("blockers", g.Blk.ToList());
        o.Add("warnings", g.Wrn.ToList());
        o.Add("contracts", g.A.Select(a => new J
        {
            { "acc", a.AccId }, { "inv", a.InvId }, { "code", a.InvCode }, { "name", a.NameEn }, { "nameAr", a.NameAr }, { "nid", a.NatId },
            { "n", a.N }, { "face", a.Fv }, { "pv", a.Pv }, { "funding", a.Fv - a.Pv }, { "margin", a.Marg }, { "fees", a.Fee }, { "eir", a.Eir },
            { "clientId", a.ClientId }, { "clientNo", a.ClientNo }, { "clientNew", a.ClientNew }, { "blacklisted", a.ClientBl },
        }).ToList());
        o.Add("lines", g.C.Select(c => new J
        {
            { "acc", g.A[c.AccIx].AccId }, { "line", c.LineNo }, { "no", c.ChequeNo }, { "bank", c.BankName }, { "due", c.Due is null ? null : D(c.Due) },
            { "amt", c.Amt }, { "pv", c.Pv }, { "open", c.Opening }, { "p", c.Princ }, { "i", c.Intr }, { "close", c.Closing },
            { "m", c.Marg }, { "f", c.Fee }, { "wallet", c.WalletId },
        }).ToList());
        o.Add("income", inc.Values.Select(x => new J
        {
            { "due", D(x.Due) }, { "n", x.N }, { "funding", x.Fund }, { "margin", x.Marg }, { "fees", x.Fee },
            { "total", x.Fund + x.Marg + x.Fee }, { "status", x.Status }, { "tranSetId", x.Ts },
        }).ToList());
        var release = new List<J>();
        foreach (var (part, from, to) in new[] { ("FUNDING", "120402", "450101"), ("MARGIN", "120403", "450102"), ("FEES", "120404", "450103") })
        {
            release.Add(new J
            {
                { "part", part }, { "from", from }, { "fromName", await AcctNameAsync(from, ct) }, { "to", to }, { "toName", await AcctNameAsync(to, ct) },
            });
        }

        o.Add("release", release);
        var gls = new List<J>();
        foreach (var l in g.Gls)
        {
            gls.Add(new J
            {
                { "j", l.J }, { "account", l.Acct }, { "name", await AcctNameAsync(l.Acct, ct) },
                { "combination", Company + "-" + Branch + "-" + CostCenter + "-" + l.Acct + "-" + BusinessLine + "-" + Future + "-" + Future },
                { "dr", l.Dr }, { "cr", l.Cr }, { "description", l.Descr },
            });
        }

        o.Add("gl", gls);
        o.Add("glRule", g.GlRule);
        o.Add("ruleCharges", g.RuleCharges?.Order().ToList());
        return o;
    }

    // ------------------------------------------------------------------------------------------------ public: preview

    /// <summary>preview_json: the saved disbursement when there is one, otherwise the calculation with its checks.</summary>
    public async Task<object> PreviewJsonAsync(MtgDisbursementInput p, CancellationToken ct)
    {
        var g = await LoadDoneAsync(p.PortfolioId, ct);
        if (g is null)
        {
            g = await ComputeAsync(p, ct);
            await LoadBankAsync(g, p.BankAccountId, ct);
            await ChecksAsync(g, ct);
            await BuildGlAsync(g, ct);
        }

        return await ToJsonAsync(g, ct);
    }

    // ------------------------------------------------------------------------------------------------ public: complete

    /// <summary>complete: books the disbursement; returns the TRAN_SET_ID and the next workflow task. It does not commit.</summary>
    public async Task<(string TranSetId, long? NewTaskId)> CompleteAsync(
        MtgDisbursementInput p, long? taskId, long? bankId, string? paymentRef, string? comment, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(comment))
            throw Fail(-20960, "A comment is required to complete the disbursement.");
        if (string.IsNullOrWhiteSpace(paymentRef))
            throw Fail(-20961, "Enter the payment reference of the transfer to the developer.");
        if (p.DisbDate is null)
            throw Fail(-20962, "Enter the disbursement date.");

        var portfolio = await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == p.PortfolioId, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        var old = portfolio.PortfolioStatusCode;
        if (await db.Set<MtgPurchasedPortfolioDisbursement>().AnyAsync(x => x.PortfolioId == p.PortfolioId, ct))
            throw Fail(-20963, "This portfolio was already disbursed.");
        if (!await db.Set<MtgLkpPortfolioStatus>().AnyAsync(x => x.StatusCode == "DISBURSEMENT_COMPLETED", ct))
            throw Fail(-20964, "Status DISBURSEMENT_COMPLETED is missing. Run FNV_DISBURSEMENT_SETUP.sql first.");

        // the open Disbursement task of this portfolio
        var task = await steps.OpenTaskAsync(p.PortfolioId, "DISBURS", taskId, ct, like: true, page: 217);
        if (task is null || DoneStatuses.Contains(task.Status.ToUpperInvariant()))
            throw Fail(-20965, "No open Disbursement task was found for this portfolio. Open the page from the Disbursement task in your inbox.");

        // bank and account
        var bankOk = await (from a in db.Set<TrsBankAccount>()
                            join b in db.Set<TrsBank>() on a.BankId equals b.BankId
                            where a.AccountId == p.BankAccountId && a.BankId == bankId && a.IsActive && b.IsActive
                            select a).AnyAsync(ct);
        if (!bankOk)
            throw Fail(-20966, "Choose an active bank and one of its active accounts.");

        var g = await ComputeAsync(p, ct);
        await LoadBankAsync(g, p.BankAccountId, ct);
        var cur = g.Cur?.Trim();
        if (!await db.Set<TrsBankAccount>().AnyAsync(x => x.AccountId == p.BankAccountId && x.CurrencyCode.Trim() == cur, ct))
            throw Fail(-20967, "The bank account currency must be " + g.Cur + ".");
        await ChecksAsync(g, ct);
        await BuildGlAsync(g, ct);
        if (g.Blk.Count > 0)
            throw Fail(-20968, g.Blk[0]);

        // 1. header with the TRAN_SET_ID
        var ts = await NextTranSetIdAsync(g.PfNo, g.Disb, ct);
        var head = new MtgPurchasedPortfolioDisbursement
        {
            PortfolioId = p.PortfolioId,
            TranSetId = ts,
            DisbursementDate = g.Disb,
            BankId = bankId,
            BankAccountId = p.BankAccountId,
            BankGlAccount = g.BankGlCode,
            PaymentRef = paymentRef.Trim(),
            CurrencyCode = cur,
            PvMethod = g.Pvm,
            PvRate = g.Rate,
            PvBaseDate = g.Base,
            ChequeCount = g.C.Count,
            ContractCount = g.A.Count,
            FaceAmount = g.Fv,
            PvAmount = g.Pv,
            FundingRecoveryAmount = g.Fund,
            MarginAmount = g.Margin,
            AdminFrAmount = g.FrAdmin,
            AdminFinalAmount = g.Admin,
            LegalFrAmount = g.FrLegal,
            LegalFinalAmount = g.Legal,
            ValuationFrAmount = g.FrVal,
            ValuationFinalAmount = g.Val,
            FeesAmount = g.Fees,
            BoardPriceAmount = g.BoardPrice,
            PayableAmount = g.Price,
            WorkflowTaskId = task.TaskId,
            Comments = Substr(comment, 4000),
            StatusCode = "COMPLETED",
            MarginTreatment = g.Mm,
            AdminTreatment = g.Am,
            LegalTreatment = g.Lm,
            ValuationTreatment = g.Vm,
            MarginDeferredAmount = g.DefMargin,
            MarginImmediateAmount = g.ImmMargin,
            FeesDeferredAmount = g.DefFees,
            FeesImmediateAmount = g.ImmFees,
        };
        db.Set<MtgPurchasedPortfolioDisbursement>().Add(head);
        await db.SaveChangesAsync(ct);

        // 2. investors in CRM
        await ResolveClientsAsync(g, true, ct);
        var (_, created, matched) = InvestorCounts(g);

        // 3. wallet cheques (received in the company vault) and the repayment schedule
        var received = await db.Set<AdmLkpChequeStatus>().AnyAsync(x => x.ChequeStatusCode == "RECEIVED", ct);
        var le = await db.Set<GlLedger>().Where(x => x.IsPrimaryLedger).MinAsync(x => (long?)x.LegalEntityId, ct) ?? 1;
        var disbAt = DateTime.SpecifyKind(g.Disb.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var year = MtgAjax.CairoNow(clock).Year.ToString(CultureInfo.InvariantCulture);
        var schedule = new List<MtgPurchasedPortfolioRepaymentSchedule>();
        foreach (var c in g.C)
        {
            var a = g.A[c.AccIx];
            var code = await IssuerCodeAsync(c.BankName, ct);
            var cid = await events.NextIdAsync("CHQ_CHEQUES", "CHEQUE_ID", ct);
            var chqDate = c.ChqDate ?? c.Due ?? throw new ApexProcessException("ORA-01400: cannot insert NULL into (\"FND_CHEQUE\".\"CHEQUE_DATE\")");
            db.Set<ChqCheque>().Add(new ChqCheque
            {
                ChequeId = cid,
                ChequeRef = "MTG-CHQ-" + year + "-" + cid.ToString("0000000000", CultureInfo.InvariantCulture),
                ChequeNo = c.ChequeNo ?? string.Empty,
                ChequeAt = DateTime.SpecifyKind(chqDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                DueDate = c.Due ?? chqDate,
                IssuerBankName = Substr(Nvl(c.BankName, "Unknown bank"), 300)!,
                IssuerBankCode = code,
                DrawerNameEn = Substr(a.NameEn, 300) ?? string.Empty,
                DrawerNameAr = Substr(a.NameAr, 300),
                DrawerNationalId = Substr(a.NatId, 50),
                CustomerId = a.ClientId?.ToString(CultureInfo.InvariantCulture),
                CustomerNameEn = Substr(a.NameEn, 300),
                CustomerNameAr = Substr(a.NameAr, 300),
                ChequeAmount = c.Amt,
                CurrencyCode = Substr(cur, 3) ?? string.Empty,
                SourceModule = "MORTGAGE",
                SourceRefType = "PURCHASED_PORTFOLIO",
                SourceRefId = p.PortfolioId.ToString(CultureInfo.InvariantCulture),
                ReceivedAt = disbAt,
                StatusCode = "NEW",
                IsReplaced = false,
                IsGlPosted = false,
                LegalEntityId = le,
                Notes = "Purchased portfolio " + g.PfNo + ", disbursement " + ts,
                CurrentLocationCode = "CLIENT",
                CurrentCustodianUser = "CLIENT_REP",
            });
            await db.SaveChangesAsync(ct);
            await events.RegisterEventAsync(cid, new ChqEventArgs
            {
                EventType = "RECEIVED",
                ToLocationCode = WalletLocation,
                EventAt = disbAt,
                EffectiveAt = disbAt,
                EventAmount = c.Amt,
                CurrencyCode = cur,
                Notes = "Received with purchased portfolio " + g.PfNo + " (disbursement " + ts + ")",
                MovementRef = ts,
                ToCustodianUser = null,
                HandoverBy = Me,
                ReceivedBy = Me,
                CreatedBy = Me,
                SourceAppId = 105,
                SourcePageId = 217,
            }, ct);
            c.WalletId = cid;
            var line = new MtgPurchasedPortfolioRepaymentSchedule
            {
                DisbursementId = head.DisbursementId,
                PortfolioId = p.PortfolioId,
                PortfolioAccountId = a.AccId,
                InvestorId = a.InvId,
                ClientId = a.ClientId,
                IsClientCreated = a.ClientNew == "Y",
                SourceChequeId = c.SrcId,
                WalletChequeId = cid,
                LineNo = c.LineNo,
                ChequeNo = c.ChequeNo,
                BankName = c.BankName,
                DueDate = c.Due,
                ChequeAmount = c.Amt,
                PvAmount = c.Pv,
                DaysFromDisbursement = (int)c.Days,
                OpeningBalance = c.Opening,
                PrincipalAmount = c.Princ,
                InterestAmount = c.Intr,
                ClosingBalance = c.Closing,
                MarginRelease = c.Marg,
                FeeRelease = c.Fee,
                EirRate = a.Eir,
            };
            schedule.Add(line);
            db.Set<MtgPurchasedPortfolioRepaymentSchedule>().Add(line);
            if (received)
            {
                var src = await db.Set<MtgPurchasedPortfolioCheque>().FirstOrDefaultAsync(x => x.ChequeId == c.SrcId, ct);
                if (src is not null)
                    src.ChequeStatusCode = "RECEIVED";
            }
        }

        // 4. unearned income release guide, one line per cheque date
        foreach (var d in schedule.GroupBy(x => x.DueDate))
        {
            db.Set<MtgPurchasedPortfolioIncomeSchedule>().Add(new MtgPurchasedPortfolioIncomeSchedule
            {
                DisbursementId = head.DisbursementId,
                PortfolioId = p.PortfolioId,
                DueDate = d.Key ?? throw new ApexProcessException("ORA-01400: cannot insert NULL into (\"FNV_PP_INCOME_SCHEDULE\".\"DUE_DATE\")"),
                ChequeCount = d.Count(),
                FundingRecoveryAmount = d.Sum(x => x.InterestAmount),
                MarginAmount = d.Sum(x => x.MarginRelease),
                FeesAmount = d.Sum(x => x.FeeRelease),
                TotalAmount = d.Sum(x => x.InterestAmount) + d.Sum(x => x.MarginRelease) + d.Sum(x => x.FeeRelease),
                PostingStatus = "PENDING",
            });
        }

        await db.SaveChangesAsync(ct);

        // 5. GL: booking + payment journals under one TRAN_SET_ID (no commit inside the GL API)
        var res = await gl.PostTranSetAsync(Payload(g, ts, comment, paymentRef.Trim()), ct);
        var r = JsonDocument.Parse(res).RootElement;
        var glStatus = Str(r, "status");
        if ((glStatus ?? "ERROR") != "OK")
            throw Fail(-20969, "GL refused the entries: " + Str(r, "message") + " " + FirstError(r));
        head.GlBatchNo = Str(r, "batch_no");
        head.GlStatus = glStatus;
        head.GlResponse = res;
        head.ClientsCreated = created;
        head.ClientsMatched = matched;
        await db.SaveChangesAsync(ct);

        // 6. workflow: start if needed, then complete (approve when the step only allows approve)
        await steps.StartAsync(task, ct);
        var newTask = await steps.DecideAsync(task.TaskId, "COMPLETE", comment, ct);
        portfolio.PortfolioStatusCode = "DISBURSEMENT_COMPLETED";
        portfolio.WorkflowTaskId = newTask ?? portfolio.WorkflowTaskId;
        db.Set<MtgPurchasedPortfolioStatusHist>().Add(new MtgPurchasedPortfolioStatusHist
        {
            PortfolioId = p.PortfolioId,
            FromStatus = old,
            ToStatus = "DISBURSEMENT_COMPLETED",
            ChangeAt = clock.UtcNow,
            ChangedBy = Me,
            Comments = Substr("Disbursement " + ts + " completed. " + comment, 4000),
        });
        await db.SaveChangesAsync(ct);
        return (ts, newTask);
    }
}
