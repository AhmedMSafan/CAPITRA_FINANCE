using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fct;

public sealed class FctDisbursementConfiguration : IEntityTypeConfiguration<FctDisbursement>
{
    public void Configure(EntityTypeBuilder<FctDisbursement> builder)
    {
        builder.ToTable("FCT_DISBURSEMENTS");
        builder.HasKey(x => x.DisbursementId);
        builder.HasIndex(x => new { x.ClientId, x.CurrencyId, x.DisbursementDate });
        builder.HasIndex(x => x.AgreementId);
        builder.HasIndex(x => x.BankAccountId);
        builder.HasIndex(x => x.BankReference);
        builder.HasIndex(x => x.BaseCurrencyId);
        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.IsConfirmed);
        builder.HasIndex(x => x.CurrencyId);
        builder.HasIndex(x => x.DisbursementDate);
        builder.HasIndex(x => x.ExecutionDate);
        builder.HasIndex(x => x.FundingRequestId);
        builder.HasIndex(x => x.IsGlPosted);
        builder.HasIndex(x => x.OriginalDisbursementId);
        builder.HasIndex(x => x.PaymentReference);
        builder.HasIndex(x => x.PaymentChannel);
        builder.HasIndex(x => x.PaymentMethod);
        builder.HasIndex(x => x.PaymentStatus);
        builder.HasIndex(x => x.IsReconciled);
        builder.HasIndex(x => x.IsReversed);
        builder.HasIndex(x => x.StatusId);
        builder.HasIndex(x => x.TransactionId);
        builder.HasIndex(x => x.DisbursementType);
        builder.HasIndex(x => x.ValueDate);
        builder.HasIndex(x => new { x.FacilityId, x.SublimitId, x.FacilityReservationStatus });
        builder.HasIndex(x => x.DisbursementNumber).IsUnique();
        builder.HasIndex(x => x.FacilityReservationKey).IsUnique();

        builder.Property(x => x.DisbursementId).UseIdentityByDefaultColumn();
        builder.Property(x => x.DisbursementNumber).HasMaxLength(30);
        builder.Property(x => x.DisbursementRef).HasMaxLength(50);
        builder.Property(x => x.DisbursementType).HasMaxLength(30);
        builder.Property(x => x.DisbursementTypeAr).HasMaxLength(30);
        builder.Property(x => x.GrossAmount).HasPrecision(18, 2);
        builder.Property(x => x.FeeDeducted).HasPrecision(18, 3);
        builder.Property(x => x.InterestDeducted).HasPrecision(18, 3);
        builder.Property(x => x.TaxDeducted).HasPrecision(18, 3);
        builder.Property(x => x.OtherDeductions).HasPrecision(18, 3);
        builder.Property(x => x.NetAmount).HasPrecision(18, 3);
        builder.Property(x => x.ExchangeRate).HasPrecision(9, 6);
        builder.Property(x => x.BaseNetAmount).HasPrecision(18, 2);
        builder.Property(x => x.PaymentMethod).HasMaxLength(30);
        builder.Property(x => x.PaymentMethodAr).HasMaxLength(30);
        builder.Property(x => x.PaymentChannel).HasMaxLength(30);
        builder.Property(x => x.PaymentChannelAr).HasMaxLength(30);
        builder.Property(x => x.BeneficiaryName).HasMaxLength(500);
        builder.Property(x => x.BeneficiaryBank).HasMaxLength(500);
        builder.Property(x => x.BeneficiaryIban).HasMaxLength(50);
        builder.Property(x => x.BeneficiarySwift).HasMaxLength(20);
        builder.Property(x => x.BeneficiaryAccount).HasMaxLength(50);
        builder.Property(x => x.RemittingBank).HasMaxLength(500);
        builder.Property(x => x.RemittingAccount).HasMaxLength(50);
        builder.Property(x => x.RemittingSwift).HasMaxLength(20);
        builder.Property(x => x.PaymentReference).HasMaxLength(100);
        builder.Property(x => x.BankReference).HasMaxLength(100);
        builder.Property(x => x.SwiftReference).HasMaxLength(50);
        builder.Property(x => x.TransactionId).HasMaxLength(100);
        builder.Property(x => x.ConfirmationRef).HasMaxLength(100);
        builder.Property(x => x.ConfirmedBy).HasMaxLength(200);
        builder.Property(x => x.ReconciliationRef).HasMaxLength(100);
        builder.Property(x => x.ReconciledBy).HasMaxLength(200);
        builder.Property(x => x.ReversalReason).HasMaxLength(2000);
        builder.Property(x => x.ReversalReasonAr).HasMaxLength(2000);
        builder.Property(x => x.ReversedBy).HasMaxLength(200);
        builder.Property(x => x.ReversalRef).HasMaxLength(100);
        builder.Property(x => x.GlJournalRef).HasMaxLength(50);
        builder.Property(x => x.ProcessedBy).HasMaxLength(200);
        builder.Property(x => x.AuthorizedBy).HasMaxLength(200);
        builder.Property(x => x.SecondAuthorizedBy).HasMaxLength(200);
        builder.Property(x => x.PaymentStatus).HasMaxLength(30);
        builder.Property(x => x.PaymentStatusAr).HasMaxLength(30);
        builder.Property(x => x.FailureReason).HasMaxLength(2000);
        builder.Property(x => x.FailureReasonAr).HasMaxLength(2000);
        builder.Property(x => x.IndexRate).HasPrecision(9, 6);
        builder.Property(x => x.SpreadRate).HasPrecision(9, 6);
        builder.Property(x => x.BaseDayCountBasis).HasMaxLength(20);
        builder.Property(x => x.PrincipalRepaymentScheduleType).HasMaxLength(20);
        builder.Property(x => x.PrincipalRepaymentMethod).HasMaxLength(20);
        builder.Property(x => x.PrincipalDistributionMethod).HasMaxLength(20);
        builder.Property(x => x.InterestRepaymentMethod).HasMaxLength(20);
        builder.Property(x => x.StatusCode).HasMaxLength(30);
        builder.Property(x => x.AdvanceRatePct).HasPrecision(9, 6);
        builder.Property(x => x.ApprovedInvoiceAmount).HasPrecision(18, 2);
        builder.Property(x => x.ApprovedAdvanceAmount).HasPrecision(18, 3);
        builder.Property(x => x.SourceFundType).HasMaxLength(20);
        builder.Property(x => x.FacilityReservationStatus).HasMaxLength(20);
        builder.Property(x => x.FacilityReservedAmount).HasPrecision(18, 2);
        builder.Property(x => x.FacilityReservationKey).HasMaxLength(200);
        builder.Property(x => x.FacilityInterestRatePct).HasPrecision(9, 6);
        builder.Property(x => x.BankScheduleStatus).HasMaxLength(20);
        builder.Property(x => x.BankScheduleTotalAmount).HasPrecision(18, 2);

        // LEGAL_ENTITY_ID -> FND_LEGAL_ENTITY: added when the FND tables are ported.
        // PRODUCT_ID -> FNV_PRODUCTS: added when the CAP tables are ported.
        // AGREEMENT_ID -> FCT_AGREEMENTS: added when that table is ported.
        // BANK_ACCOUNT_ID -> FCT_CLIENT_BANK_ACCOUNTS: added when that table is ported.
        // BASE_CURRENCY_ID -> LKP_CURRENCIES: added when the LKP tables are ported.
        builder.HasOne<CrmClient>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        // CURRENCY_ID -> LKP_CURRENCIES: added when the LKP tables are ported.
        // FUNDING_REQUEST_ID -> FCT_FUNDING_REQUESTS: added when that table is ported.
        builder.HasOne<FctDisbursement>().WithMany().HasForeignKey(x => x.OriginalDisbursementId).OnDelete(DeleteBehavior.Restrict);
        // STATUS_ID -> LKP_STATUS_CODES: added when the LKP tables are ported.
        // FACILITY_RATE_ID -> TRS_FACILITY_INTEREST_RATES: added when that table is ported.
        // POLICY_ACTIVITY_MEMBER_ID -> FND_POLICY_MEMBERS: added when the FND tables are ported.
        // POLICY_ASSET_MEMBER_ID -> FND_POLICY_MEMBERS: added when the FND tables are ported.
        // POLICY_BUSINESS_LINE_MEMBER_ID -> FND_POLICY_MEMBERS: added when the FND tables are ported.
        // FACILITY_POLICY_CHECK_ID -> TRS_FACILITY_POLICY_CHECKS: added when that table is ported.
        // POLICY_PRODUCT_MEMBER_ID -> FND_POLICY_MEMBERS: added when the FND tables are ported.
        // POLICY_CLIENT_SEGMENT_MEMBER_ID -> FND_POLICY_MEMBERS: added when the FND tables are ported.
        // FK_FCT_DISB_SUB_HIER: FACILITY_ID,TRANCHE_ID,SUBLIMIT_ID -> TRS_FACILITY_SUBLIMITS (table not exported)
        // FK_FCT_DISB_TRANCHE: FACILITY_ID,TRANCHE_ID -> TRS_FACILITY_TRANCHES (table not exported)
        // TREASURY_DRAWDOWN_ID -> TRS_FACILITY_DRAWDOWNS: added when that table is ported.
        builder.ConfigureAudit();
    }
}
