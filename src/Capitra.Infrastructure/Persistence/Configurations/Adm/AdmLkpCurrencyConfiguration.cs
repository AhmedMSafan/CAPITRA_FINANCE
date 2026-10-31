using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpCurrencyConfiguration : IEntityTypeConfiguration<AdmLkpCurrency>
{
    public void Configure(EntityTypeBuilder<AdmLkpCurrency> builder)
    {
        builder.ToTable("ADM_LKP_CURRENCIES");
        builder.HasKey(x => x.CurrencyCode);

        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.CurrencyNameEn).HasMaxLength(100);
        builder.Property(x => x.CurrencyNameAr).HasMaxLength(100);

        builder.ConfigureAudit();
    }
}
