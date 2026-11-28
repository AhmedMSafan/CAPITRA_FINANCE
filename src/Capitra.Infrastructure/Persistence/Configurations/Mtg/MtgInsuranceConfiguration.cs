using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgInsuranceConfiguration : IEntityTypeConfiguration<MtgInsurance>
{
    public void Configure(EntityTypeBuilder<MtgInsurance> builder)
    {
        builder.ToTable("MTG_INSURANCES");
        builder.HasKey(x => x.InsuranceId);
        builder.HasIndex(x => x.PortfolioId);

        builder.Property(x => x.InsuranceId).UseIdentityByDefaultColumn();
        builder.Property(x => x.InsuranceTypeCode).HasMaxLength(30);
        builder.Property(x => x.InsuranceCompany).HasMaxLength(300);
        builder.Property(x => x.PolicyNo).HasMaxLength(100);
        builder.Property(x => x.SumInsured).HasPrecision(18, 2);

        builder.HasOne<MtgPurchasedPortfolioDocument>().WithMany().HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpInsuranceType>().WithMany().HasForeignKey(x => x.InsuranceTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgProjectUnit>().WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
