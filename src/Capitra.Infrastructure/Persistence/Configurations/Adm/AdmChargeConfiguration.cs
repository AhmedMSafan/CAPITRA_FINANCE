using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmChargeConfiguration : IEntityTypeConfiguration<AdmCharge>
{
    public void Configure(EntityTypeBuilder<AdmCharge> builder)
    {
        builder.ToTable("ADM_CHARGES");
        builder.HasKey(x => x.ChargeId);
        builder.HasIndex(x => x.ChargeCode).IsUnique();
        builder.HasIndex(x => x.ChargeTypeCode);
        builder.HasIndex(x => x.CurrencyCode);

        builder.Property(x => x.ChargeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ChargeCode).HasMaxLength(50);
        builder.Property(x => x.ChargeNameEn).HasMaxLength(150);
        builder.Property(x => x.ChargeNameAr).HasMaxLength(150);
        builder.Property(x => x.ChargeTypeCode).HasMaxLength(30);
        builder.Property(x => x.FinancialNatureCode).HasMaxLength(20);
        builder.Property(x => x.CalculationTypeCode).HasMaxLength(20);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasOne<AdmLkpChargeType>().WithMany().HasForeignKey(x => x.ChargeTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpCurrency>().WithMany().HasForeignKey(x => x.CurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
