using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fct;

public sealed class FctAgreementDebtorConfiguration : IEntityTypeConfiguration<FctAgreementDebtor>
{
    public void Configure(EntityTypeBuilder<FctAgreementDebtor> builder)
    {
        builder.ToTable("FCT_AGREEMENT_DEBTORS");
        builder.HasKey(x => x.AgrmtDebtorId);
        builder.HasIndex(x => x.AgreementId);
        builder.HasIndex(x => x.ClientDebtorId);
        builder.HasIndex(x => x.CurrencyId);
        builder.HasIndex(x => x.DebtorId);
        builder.HasIndex(x => x.IsNotified);
        builder.HasIndex(x => x.AgrmtProductId);
        builder.HasIndex(x => x.StatusId);
        builder.HasIndex(x => new { x.AgreementId, x.DebtorId }).IsUnique();

        builder.Property(x => x.AgrmtDebtorId).UseIdentityByDefaultColumn();
        builder.Property(x => x.DebtorLimit).HasPrecision(18, 3);
        builder.Property(x => x.ApprovedLimit).HasPrecision(18, 3);
        builder.Property(x => x.UtilizedAmount).HasPrecision(18, 2);
        builder.Property(x => x.AvailableAmount).HasPrecision(18, 2);
        builder.Property(x => x.AdvanceRatePct).HasPrecision(9, 6);
        builder.Property(x => x.ConcentrationLimitPct).HasPrecision(9, 6);
        builder.Property(x => x.CurrentConcentrationPct).HasPrecision(9, 6);
        builder.Property(x => x.PaymentTerms).HasMaxLength(50);
        builder.Property(x => x.PaymentTermsAr).HasMaxLength(50);
        builder.Property(x => x.NotificationMethod).HasMaxLength(30);
        builder.Property(x => x.RiskGrade).HasMaxLength(20);
        builder.Property(x => x.InsuranceCoveragePct).HasPrecision(9, 6);
        builder.Property(x => x.ApprovedBy).HasMaxLength(200);
        builder.Property(x => x.ApprovalReference).HasMaxLength(100);
        builder.Property(x => x.Remarks).HasMaxLength(2000);
        builder.Property(x => x.RemarksAr).HasMaxLength(2000);
        builder.Property(x => x.DebtorName).HasMaxLength(100);
        builder.Property(x => x.TaxId).HasMaxLength(20);

        builder.HasOne<FctAgreement>().WithMany().HasForeignKey(x => x.AgreementId).OnDelete(DeleteBehavior.Restrict);
        // CLIENT_DEBTOR_ID -> FCT_CLIENT_DEBTORS: added when that table is ported.
        builder.HasOne<CrmLkpCurrency>().WithMany().HasForeignKey(x => x.CurrencyId).OnDelete(DeleteBehavior.Restrict);
        // DEBTOR_ID -> FCT_DEBTORS: added when that table is ported.
        // AGRMT_PRODUCT_ID -> FCT_AGREEMENT_PRODUCTS: added when that table is ported.
        builder.HasOne<CrmLkpStatusCode>().WithMany().HasForeignKey(x => x.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
