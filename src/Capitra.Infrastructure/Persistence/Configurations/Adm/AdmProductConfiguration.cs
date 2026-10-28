using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmProductConfiguration : IEntityTypeConfiguration<AdmProduct>
{
    public void Configure(EntityTypeBuilder<AdmProduct> builder)
    {
        builder.ToTable("ADM_PRODUCTS");
        builder.HasKey(x => x.ProductId);
        builder.HasIndex(x => x.ProductCode).IsUnique();
        builder.HasIndex(x => new { x.IsActive, x.BusinessLineCode });
        builder.HasIndex(x => x.BusinessLineCode);
        builder.HasIndex(x => x.DefaultCurrencyCode);

        builder.Property(x => x.ProductId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ProductCode).HasMaxLength(50);
        builder.Property(x => x.ProductNameEn).HasMaxLength(200);
        builder.Property(x => x.ProductNameAr).HasMaxLength(200);
        builder.Property(x => x.BusinessLineCode).HasMaxLength(30);
        builder.Property(x => x.ProductTypeCode).HasMaxLength(30);
        builder.Property(x => x.FactoringModeCode).HasMaxLength(20);
        builder.Property(x => x.DefaultCurrencyCode).HasMaxLength(3);

        builder.HasOne<AdmLkpBusinessLine>().WithMany().HasForeignKey(x => x.BusinessLineCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpCurrency>().WithMany().HasForeignKey(x => x.DefaultCurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
