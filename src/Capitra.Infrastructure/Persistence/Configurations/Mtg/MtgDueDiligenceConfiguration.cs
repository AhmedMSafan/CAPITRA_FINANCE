using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgDueDiligenceConfiguration : IEntityTypeConfiguration<MtgDueDiligence>
{
    public void Configure(EntityTypeBuilder<MtgDueDiligence> builder)
    {
        builder.ToTable("MTG_DUE_DILIGENCES");
        builder.HasKey(x => x.DdId);
        builder.HasIndex(x => x.PortfolioId);

        builder.Property(x => x.DdId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CheckTypeCode).HasMaxLength(50);
        builder.Property(x => x.SubjectType).HasMaxLength(30);
        builder.Property(x => x.AssignedTo).HasMaxLength(255);
        builder.Property(x => x.ResultCode).HasMaxLength(30);
        builder.Property(x => x.RiskLevelCode).HasMaxLength(20);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpRiskLevel>().WithMany().HasForeignKey(x => x.RiskLevelCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgLkpDdCheckType>().WithMany().HasForeignKey(x => x.CheckTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
