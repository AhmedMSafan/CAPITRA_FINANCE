using Capitra.Domain.Modules.Fin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fin;

public sealed class FinCurrencyConfiguration : IEntityTypeConfiguration<FinCurrency>
{
    public void Configure(EntityTypeBuilder<FinCurrency> builder)
    {
        builder.ToTable("FIN_CURRENCIES");
        builder.HasKey(x => x.CurrencyCode);

        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.CurrencyNameEn).HasMaxLength(100);
        builder.Property(x => x.CurrencyNameAr).HasMaxLength(100);
        builder.Property(x => x.Symbol).HasMaxLength(10);

        builder.ConfigureAudit();
    }
}
