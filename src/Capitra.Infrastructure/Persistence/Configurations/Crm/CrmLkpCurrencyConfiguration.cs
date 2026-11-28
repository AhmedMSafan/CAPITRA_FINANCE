using Capitra.Domain.Modules.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Crm;

public sealed class CrmLkpCurrencyConfiguration : IEntityTypeConfiguration<CrmLkpCurrency>
{
    public void Configure(EntityTypeBuilder<CrmLkpCurrency> builder)
    {
        builder.ToTable("CRM_LKP_CURRENCIES");
        builder.HasKey(x => x.CurrencyId);
        builder.HasIndex(x => x.CurrencyCode).IsUnique();

        builder.Property(x => x.CurrencyId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.CurrencyName).HasMaxLength(200);
        builder.Property(x => x.CurrencyNameAr).HasMaxLength(200);
        builder.Property(x => x.CurrencySymbol).HasMaxLength(10);
        builder.Property(x => x.NumericCode).HasMaxLength(3);

        builder.HasOne<CrmLkpCountry>().WithMany().HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
