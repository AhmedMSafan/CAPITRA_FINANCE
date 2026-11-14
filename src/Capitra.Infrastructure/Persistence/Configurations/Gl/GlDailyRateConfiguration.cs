using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlDailyRateConfiguration : IEntityTypeConfiguration<GlDailyRate>
{
    public void Configure(EntityTypeBuilder<GlDailyRate> builder)
    {
        builder.ToTable("GL_DAILY_RATES");
        builder.HasKey(x => x.DailyRateId);
        builder.HasIndex(x => new { x.RateTypeId, x.FromCurrencyCode, x.ToCurrencyCode, x.ConversionDate }).IsUnique();
        builder.HasIndex(x => new { x.ConversionDate, x.RateTypeId });
        builder.HasIndex(x => new { x.FromCurrencyCode, x.ToCurrencyCode, x.ConversionDate });
        builder.HasIndex(x => new { x.RateStatus, x.ConversionDate });

        builder.Property(x => x.DailyRateId).UseIdentityByDefaultColumn();
        builder.Property(x => x.FromCurrencyCode).HasMaxLength(3);
        builder.Property(x => x.ToCurrencyCode).HasMaxLength(3);
        builder.Property(x => x.ExchangeRate).HasPrecision(24, 12);
        builder.Property(x => x.InverseRate).HasPrecision(24, 12);
        builder.Property(x => x.RateStatus).HasMaxLength(30);
        builder.Property(x => x.SourceCode).HasMaxLength(30);
        builder.Property(x => x.SourceReference).HasMaxLength(240);
        builder.Property(x => x.TriangulationCurrency).HasMaxLength(3);
        builder.Property(x => x.TolerancePercent).HasPrecision(9, 4);
        builder.Property(x => x.ApprovedBy).HasMaxLength(100);
        builder.Property(x => x.RejectedBy).HasMaxLength(100);
        builder.Property(x => x.RejectionReason).HasMaxLength(1000);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.FromCurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.ToCurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.TriangulationCurrency).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlRateType>().WithMany().HasForeignKey(x => x.RateTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
