using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Chq.Packages;
using Capitra.Application.Modules.Fct;
using Capitra.Application.Modules.Gl;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Chq;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Crm;

/// <summary>
/// Complete of the Invoice Upload step (agreed with Ahmed, 1 Nov 2026; the charges by claim, 2 Nov 2026): until then the page only saves a
/// draft; Complete does all the work in the completion's one transaction, so a failure anywhere leaves only the draft:
/// 1. the request is processed (invoices checked, charges and repayment schedule calculated; claimed charges are kept);
/// 2. when there are charges, their claim must be PAID (Create Claim on this page, paid in Collection, which posts the charges' GL);
/// 3. the draft cheques are loaded into the cheques wallet (CHQ_CHEQUES, a RECEIVED event each);
/// 4. the cheques' memo entry (the rule's CHEQUE_AMOUNT lines) is posted under one GL transaction set of the product, kept on the
///    request and its cheques.
/// </summary>
public sealed class CrmDisbursementCompletionService(
    ICapitraDbContext db, ICurrentUser currentUser, IClock clock, CrmInvoiceEntryService entry, CrmDisbursementProcessService process,
    ProductActionPosting posting, AdmProductEventRuleService rules, ChqEventPackage events)
{
    private const long LegalEntityId = 1;

    public async Task CompleteAsync(long disbursementId, CancellationToken cancellationToken)
    {
        var d = await db.Set<FctDisbursement>().FirstOrDefaultAsync(x => x.DisbursementId == disbursementId, cancellationToken)
            ?? throw new NotFoundException($"Disbursement request {disbursementId} was not found.");
        if (d.IsGlPosted) return;
        var batch = await db.Set<FctInvoiceBatch>().Where(x => x.DisbursementId == disbursementId).OrderByDescending(x => x.BatchId)
            .FirstOrDefaultAsync(cancellationToken);

        // 1. process (charges of a claim are kept; charges paid before claims existed are kept as they are)
        var claim = await db.Set<FctCustomerClaim>().AsNoTracking()
            .Where(x => x.DisbursementId == disbursementId && x.StatusCode != FctCustomerClaim.Cancelled)
            .OrderByDescending(x => x.ClaimId).FirstOrDefaultAsync(cancellationToken);
        var actioned = await db.Set<FctDisbursementCharge>().AnyAsync(x => x.DisbursementId == disbursementId && x.StatusCode != "CALCULATED"
            && x.StatusCode != "CANCELLED" && x.StatusCode != "REVERSED", cancellationToken);
        if (!(claim is null && actioned && d.StatusCode == "PROCESSED")) await process.ProcessCoreAsync(d, batch, cancellationToken);

        // 2. the charges must be claimed and the claim paid
        var owed = (await db.Set<FctDisbursementCharge>().Where(x => x.DisbursementId == disbursementId).ToListAsync(cancellationToken))
            .Where(FctCustomerClaimService.Owed).ToList();
        if (owed.Sum(FctCustomerClaimService.Outstanding) > 0)
            throw new BusinessRuleException(claim is null
                ? $"The charges ({owed.Sum(x => x.TotalAmount):#,##0.00}) are not claimed yet. Create the claim; Complete is possible once it is paid."
                : $"Claim {claim.ClaimNumber} is not fully paid: {claim.TotalAmount - claim.PaidAmount:#,##0.00} is still to be paid in Collection.");

        // 3. the draft cheques into the wallet
        var drafts = batch is null ? [] : await entry.Cheques(batch.BatchId, disbursementId)
            .Where(x => x.WalletChequeId == null && x.StatusCode == "DRAFT").OrderBy(x => x.DisbursementChequeId).ToListAsync(cancellationToken);
        var loaded = await LoadToWalletAsync(d, drafts, cancellationToken);
        var chequeTotal = drafts.Sum(x => x.ChequeAmount);

        // 4. the cheques' memo entry
        if (chequeTotal == 0) return;

        var product = await db.Set<AdmProduct>().AsNoTracking().FirstAsync(x => x.ProductId == d.ProductId, cancellationToken);
        if (string.IsNullOrWhiteSpace(product.GlTranSetCode))
            throw new BusinessRuleException($"Product {product.ProductCode} has no GL Tran Set Code.");
        var date = GlTime.Today(clock);
        var ruleId = owed.FirstOrDefault()?.ProductEventRuleId ?? claim?.ProductEventRuleId;
        var plan = ruleId is { } id
            ? await rules.PlanOfRuleAsync(id, date, cancellationToken)
            : await process.PlanOfRequestAsync(d, cancellationToken);
        if (plan is null) throw new BusinessRuleException($"The product rule of the request is not in effect on {date:yyyy-MM-dd}.");
        if (!plan.Lines.Any(l => l.ChargeCode is null && l.AmountCode == "CHEQUE_AMOUNT"))
            throw new BusinessRuleException($"Rule {plan.RuleCode} has no memo GL lines for the cheques (amount CHEQUE_AMOUNT). Run setup-charges-payment.sql.");

        var lines = ProductEventPosting.Build(plan, l => l.ChargeCode is null && l.AmountCode == "CHEQUE_AMOUNT" ? chequeTotal : 0, null);
        var no = d.DisbursementNumber;
        string ts;
        try
        {
            ts = await posting.PostLinesAsync(plan, lines, product.GlTranSetCode.Trim().ToUpperInvariant(), "Disbursement cheques", "3100",
                "FACTORING", new ProductActionDocument("FCT_DISBURSEMENT", d.DisbursementId.ToString(CultureInfo.InvariantCulture), no, date, null,
                    $"Disbursement request {no}: {drafts.Count} cheque(s) received in the wallet"), cancellationToken);
        }
        catch (ApexProcessException ex)
        {
            // the GL's own refusal (period closed, no ledger, inactive account...) is shown as it is; nothing of Complete is kept
            throw new BusinessRuleException($"GL posting failed, nothing was completed: {ex.Message}");
        }

        // the transaction set ID on the request and its cheques
        var now = clock.UtcNow;
        d.GlTranSetId = ts;
        d.IsGlPosted = true;
        d.GlPostingDate = date;
        d.GlJournalRef = ts;
        foreach (var c in loaded)
        {
            c.IsGlPosted = true;
            c.GlJournalRef = ts;
            c.GlPostAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Each draft cheque becomes a wallet cheque of the client, received in the company vault (as a wallet batch submission does).</summary>
    private async Task<List<ChqCheque>> LoadToWalletAsync(FctDisbursement d, List<FctDisbursementCheque> drafts, CancellationToken cancellationToken)
    {
        var by = currentUser.UserName ?? "SYSTEM";
        var now = clock.UtcNow;
        var loaded = new List<ChqCheque>();
        foreach (var x in drafts)
        {
            if (await db.Set<ChqCheque>().AnyAsync(c => c.ChequeNo == x.ChequeNo && (c.IssuerBankCode ?? "#") == (x.IssuerBankCode ?? "#"), cancellationToken))
                throw new BusinessRuleException($"Cheque {x.ChequeNo} of bank {x.IssuerBankCode ?? x.IssuerBankName} is already in the wallet.");
            var chequeId = await events.NextIdAsync("CHQ_CHEQUES", "CHEQUE_ID", cancellationToken);
            var received = DateTime.SpecifyKind(x.ReceivedDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var cheque = new ChqCheque
            {
                ChequeId = chequeId,
                ChequeRef = "FCT-CHQ-" + now.ToString("yyyy", CultureInfo.InvariantCulture) + "-" + chequeId.ToString("D10", CultureInfo.InvariantCulture),
                ChequeNo = x.ChequeNo,
                ChequeAt = DateTime.SpecifyKind(x.ChequeDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                DueDate = x.DueDate,
                IssuerBankName = x.IssuerBankName,
                IssuerBankCode = x.IssuerBankCode,
                IssuerBankBranch = x.IssuerBankBranch,
                DrawerNameEn = x.DrawerNameEn,
                DrawerNameAr = x.DrawerNameAr,
                DrawerNationalId = x.DrawerNationalId,
                DrawerAccountNo = x.DrawerAccountNo,
                ChequeAmount = x.ChequeAmount,
                CurrencyCode = x.CurrencyCode,
                CustomerId = x.CustomerId ?? d.ClientId.ToString(CultureInfo.InvariantCulture),
                CustomerNameEn = x.CustomerNameEn,
                CustomerNameAr = x.CustomerNameAr,
                DepositBankAccountId = x.DepositBankAccountId,
                SourceModule = "FACTORING",
                SourceRefType = "DISBURSEMENT",
                SourceRefId = d.DisbursementId.ToString(CultureInfo.InvariantCulture),
                ReceivedAt = received,
                StatusCode = "NEW",
                LegalEntityId = d.LegalEntityId ?? LegalEntityId,
                Notes = x.Notes ?? $"Received with disbursement request {d.DisbursementNumber}",
                CurrentLocationCode = "CLIENT",
                CurrentCustodianUser = "CLIENT_REP",
                BatchId = x.BatchId,
            };
            db.Add(cheque);
            await db.SaveChangesAsync(cancellationToken);
            await events.RegisterEventAsync(chequeId, new ChqEventArgs
            {
                EventType = "RECEIVED",
                ToLocationCode = "COMPANY_VAULT",
                EventAt = received,
                EffectiveAt = received,
                EventAmount = x.ChequeAmount,
                CurrencyCode = x.CurrencyCode,
                Notes = $"Received with disbursement request {d.DisbursementNumber}",
                MovementRef = d.DisbursementNumber,
                HandoverBy = by,
                ReceivedBy = by,
                CreatedBy = by,
            }, cancellationToken);
            x.WalletChequeId = chequeId;
            x.StatusCode = "IN_WALLET";
            loaded.Add(cheque);
        }

        await db.SaveChangesAsync(cancellationToken);
        return loaded;
    }
}
