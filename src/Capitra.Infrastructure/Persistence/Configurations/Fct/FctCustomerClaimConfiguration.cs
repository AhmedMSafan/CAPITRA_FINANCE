using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fct;

public sealed class FctCustomerClaimConfiguration : IEntityTypeConfiguration<FctCustomerClaim>
{
    public void Configure(EntityTypeBuilder<FctCustomerClaim> builder)
    {
        builder.ToTable("FCT_CUSTOMER_CLAIMS");
        builder.HasKey(x => x.ClaimId);
        builder.HasIndex(x => x.ClaimNumber).IsUnique();
        builder.HasIndex(x => new { x.ClientId, x.StatusCode });
        builder.HasIndex(x => x.DisbursementId);

        builder.Property(x => x.ClaimId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ClaimNumber).HasMaxLength(40);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.CancelledBy).HasMaxLength(100);

        builder.HasOne<CrmClient>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FctDisbursement>().WithMany().HasForeignKey(x => x.DisbursementId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class FctCustomerClaimLineConfiguration : IEntityTypeConfiguration<FctCustomerClaimLine>
{
    public void Configure(EntityTypeBuilder<FctCustomerClaimLine> builder)
    {
        builder.ToTable("FCT_CUSTOMER_CLAIM_LINES");
        builder.HasKey(x => x.ClaimLineId);
        builder.HasIndex(x => new { x.ClaimId, x.ChargeCode }).IsUnique();

        builder.Property(x => x.ClaimLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ChargeCode).HasMaxLength(80);
        builder.Property(x => x.ChargeName).HasMaxLength(200);
        builder.Property(x => x.ChargeAmount).HasPrecision(18, 2);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2);

        builder.HasOne<FctCustomerClaim>().WithMany().HasForeignKey(x => x.ClaimId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class FctCustomerClaimPaymentConfiguration : IEntityTypeConfiguration<FctCustomerClaimPayment>
{
    public void Configure(EntityTypeBuilder<FctCustomerClaimPayment> builder)
    {
        builder.ToTable("FCT_CUSTOMER_CLAIM_PAYMENTS");
        builder.HasKey(x => x.ClaimPaymentId);
        builder.HasIndex(x => x.ClaimId);

        builder.Property(x => x.ClaimPaymentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.PaymentAmount).HasPrecision(18, 2);
        builder.Property(x => x.GlTranSetId).HasMaxLength(40);

        builder.HasOne<FctCustomerClaim>().WithMany().HasForeignKey(x => x.ClaimId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FctRepaymentTransaction>().WithMany().HasForeignKey(x => x.RepaymentTxnId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FctRepaymentSource>().WithMany().HasForeignKey(x => x.RepaymentSourceId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
