using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgGuaranteeConfiguration : IEntityTypeConfiguration<MtgGuarantee>
{
    public void Configure(EntityTypeBuilder<MtgGuarantee> builder)
    {
        builder.ToTable("MTG_GUARANTEES");
        builder.HasKey(x => x.GuaranteeId);
        builder.HasIndex(x => x.PortfolioId);

        builder.Property(x => x.GuaranteeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.GuaranteeTypeCode).HasMaxLength(50);
        builder.Property(x => x.GuarantorType).HasMaxLength(30);
        builder.Property(x => x.GuaranteeAmount).HasPrecision(18, 2);
        builder.Property(x => x.GuaranteePct).HasPrecision(9, 6);
        builder.Property(x => x.VerifiedBy).HasMaxLength(255);

        builder.HasOne<MtgPurchasedPortfolioDocument>().WithMany().HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpGuaranteeType>().WithMany().HasForeignKey(x => x.GuaranteeTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
