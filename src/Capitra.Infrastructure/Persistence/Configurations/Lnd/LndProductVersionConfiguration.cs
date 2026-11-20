using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Lnd;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Lnd;

public sealed class LndProductVersionConfiguration : IEntityTypeConfiguration<LndProductVersion>
{
    public void Configure(EntityTypeBuilder<LndProductVersion> builder)
    {
        builder.ToTable("LND_PRODUCT_VERSIONS");
        builder.HasKey(x => x.ProductVersionId);
        builder.HasIndex(x => new { x.ProductId, x.VersionNo }).IsUnique();
        builder.HasIndex(x => new { x.ProductId, x.StatusCode, x.EffectiveFromDate });

        builder.Property(x => x.ProductVersionId).UseIdentityByDefaultColumn();
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.MinAmount).HasPrecision(18, 2);
        builder.Property(x => x.MaxAmount).HasPrecision(18, 2);
        builder.Property(x => x.RepaymentFrequencyCode).HasMaxLength(20);
        builder.Property(x => x.InterestMethodCode).HasMaxLength(20);
        builder.Property(x => x.AnnualRatePct).HasPrecision(9, 6);
        builder.Property(x => x.DayCountCode).HasMaxLength(20);
        builder.Property(x => x.LateFeeTypeCode).HasMaxLength(20);
        builder.Property(x => x.LateFeeRatePct).HasPrecision(9, 6);
        builder.Property(x => x.LateFeeFlatAmount).HasPrecision(18, 2);
        builder.Property(x => x.LateFeeBaseCode).HasMaxLength(30);
        builder.Property(x => x.PaymentOrder).HasMaxLength(200);
        builder.Property(x => x.PrepaymentRecalcCode).HasMaxLength(20);
        builder.Property(x => x.ChangeReason).HasMaxLength(1000);
        builder.Property(x => x.RequestedBy).HasMaxLength(256);
        builder.Property(x => x.DecidedBy).HasMaxLength(256);
        builder.Property(x => x.DecisionNotes).HasMaxLength(1000);
        builder.HasOne<AdmProduct>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
