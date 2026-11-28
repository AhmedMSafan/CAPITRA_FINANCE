using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCurrencyExposureLineConfiguration : IEntityTypeConfiguration<GlCurrencyExposureLine>
{
    public void Configure(EntityTypeBuilder<GlCurrencyExposureLine> builder)
    {
        builder.ToTable("GL_CURRENCY_EXPOSURE_LINES");
        builder.HasKey(x => x.ExposureLineId);
        builder.HasIndex(x => new { x.ExposureSnapshotId, x.LineNo }).IsUnique();
        builder.HasIndex(x => new { x.ExposureSnapshotId, x.CurrencyCode });
        builder.HasIndex(x => new { x.BusinessLineCode, x.CurrencyCode });

        builder.Property(x => x.ExposureLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.BusinessLineCode).HasMaxLength(30);
        builder.Property(x => x.ExposureType).HasMaxLength(30);
        builder.Property(x => x.EnteredAmount).HasPrecision(18, 2);
        builder.Property(x => x.ExchangeRate).HasPrecision(9, 6);
        builder.Property(x => x.AccountedAmount).HasPrecision(18, 2);
        builder.Property(x => x.FunctionalCurrencyCode).HasMaxLength(3);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.CurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.FunctionalCurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCurrencyExposureSnapshot>().WithMany().HasForeignKey(x => x.ExposureSnapshotId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
