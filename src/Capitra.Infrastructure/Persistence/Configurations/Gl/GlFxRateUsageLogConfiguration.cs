using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlFxRateUsageLogConfiguration : IEntityTypeConfiguration<GlFxRateUsageLog>
{
    public void Configure(EntityTypeBuilder<GlFxRateUsageLog> builder)
    {
        builder.ToTable("GL_FX_RATE_USAGE_LOGS");
        builder.HasKey(x => x.RateUsageId);
        builder.HasIndex(x => new { x.SourceModule, x.SourceTransactionType, x.SourceTransactionId });

        builder.Property(x => x.RateUsageId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SourceModule).HasMaxLength(30);
        builder.Property(x => x.SourceTransactionType).HasMaxLength(50);
        builder.Property(x => x.SourceTransactionId).HasMaxLength(100);
        builder.Property(x => x.EnteredCurrencyCode).HasMaxLength(3);
        builder.Property(x => x.AccountedCurrencyCode).HasMaxLength(3);
        builder.Property(x => x.EnteredAmount).HasPrecision(18, 2);
        builder.Property(x => x.AccountedAmount).HasPrecision(18, 2);
        builder.Property(x => x.ExchangeRate).HasPrecision(9, 6);

        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.EnteredCurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlDailyRate>().WithMany().HasForeignKey(x => x.DailyRateId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.AccountedCurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
