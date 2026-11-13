using Capitra.Domain.Modules.Chq;
using Capitra.Domain.Modules.Col;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Col;

public sealed class ColRepaymentSourceConfiguration : IEntityTypeConfiguration<ColRepaymentSource>
{
    public void Configure(EntityTypeBuilder<ColRepaymentSource> builder)
    {
        builder.ToTable("COL_REPAYMENT_SOURCES");
        builder.HasKey(x => x.RepaymentSourceId);
        builder.HasIndex(x => new { x.RepaymentTxnId, x.StatusCode });
        builder.HasIndex(x => new { x.ChequeId, x.StatusCode });
        builder.HasIndex(x => new { x.SourceRepaymentTxnId, x.StatusCode });

        builder.Property(x => x.RepaymentSourceId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SourceType).HasMaxLength(30);
        builder.Property(x => x.SourceReference).HasMaxLength(100);
        builder.Property(x => x.SourceAmount).HasPrecision(18, 2);
        builder.Property(x => x.AllocatedAmount).HasPrecision(18, 2);
        builder.Property(x => x.StatusCode).HasMaxLength(20);

        builder.HasOne<ColRepaymentTransaction>().WithMany().HasForeignKey(x => x.RepaymentTxnId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ChqCheque>().WithMany().HasForeignKey(x => x.ChequeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ColRepaymentTransaction>().WithMany().HasForeignKey(x => x.SourceRepaymentTxnId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
