using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgAmlResultConfiguration : IEntityTypeConfiguration<MtgAmlResult>
{
    public void Configure(EntityTypeBuilder<MtgAmlResult> builder)
    {
        builder.ToTable("MTG_AML_RESULTS");
        builder.HasKey(x => x.AmlId);
        builder.HasIndex(x => x.PortfolioId);
        builder.HasIndex(x => new { x.PortfolioId, x.SubjectType, x.SubjectId });

        builder.Property(x => x.AmlId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SubjectType).HasMaxLength(30);
        builder.Property(x => x.AmlTypeCode).HasMaxLength(50);
        builder.Property(x => x.ReferenceNo).HasMaxLength(100);
        builder.Property(x => x.ResultCode).HasMaxLength(50);
        builder.Property(x => x.RiskLevelCode).HasMaxLength(30);
        builder.Property(x => x.MatchScorePct).HasPrecision(9, 6);
        builder.Property(x => x.ResultDetails).HasMaxLength(2000);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
