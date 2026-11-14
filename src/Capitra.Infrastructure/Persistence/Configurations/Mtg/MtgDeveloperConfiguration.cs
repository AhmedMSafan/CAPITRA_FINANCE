using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgDeveloperConfiguration : IEntityTypeConfiguration<MtgDeveloper>
{
    public void Configure(EntityTypeBuilder<MtgDeveloper> builder)
    {
        builder.ToTable("MTG_DEVELOPERS");
        builder.HasKey(x => x.DeveloperId);
        builder.HasIndex(x => x.DeveloperCode).IsUnique();

        builder.Property(x => x.DeveloperId).UseIdentityByDefaultColumn();
        builder.Property(x => x.DeveloperCode).HasMaxLength(30);
        builder.Property(x => x.LegalNameAr).HasMaxLength(300);
        builder.Property(x => x.LegalNameEn).HasMaxLength(300);
        builder.Property(x => x.CommercialRegNo).HasMaxLength(100);
        builder.Property(x => x.TaxRegNo).HasMaxLength(100);
        builder.Property(x => x.AddressText).HasMaxLength(1000);
        builder.Property(x => x.PhoneNo).HasMaxLength(100);
        builder.Property(x => x.EmailAddress).HasMaxLength(320);
        builder.Property(x => x.RiskLevelCode).HasMaxLength(20);

        builder.HasOne<AdmLkpRiskLevel>().WithMany().HasForeignKey(x => x.RiskLevelCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
