using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlRateImportLineConfiguration : IEntityTypeConfiguration<GlRateImportLine>
{
    public void Configure(EntityTypeBuilder<GlRateImportLine> builder)
    {
        builder.ToTable("GL_RATE_IMPORT_LINES");
        builder.HasKey(x => x.ImportLineId);
        builder.HasIndex(x => new { x.ImportBatchId, x.LineNo }).IsUnique();
        builder.HasIndex(x => new { x.ImportBatchId, x.ValidationStatus });

        builder.Property(x => x.ImportLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RateTypeCode).HasMaxLength(30);
        builder.Property(x => x.FromCurrencyCode).HasMaxLength(3);
        builder.Property(x => x.ToCurrencyCode).HasMaxLength(3);
        builder.Property(x => x.ExchangeRate).HasPrecision(9, 6);
        builder.Property(x => x.InverseRate).HasPrecision(24, 12);
        builder.Property(x => x.SourceReference).HasMaxLength(240);
        builder.Property(x => x.ValidationStatus).HasMaxLength(30);
        builder.Property(x => x.ValidationMessage).HasMaxLength(4000);

        builder.HasOne<GlRateImportBatch>().WithMany().HasForeignKey(x => x.ImportBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlDailyRate>().WithMany().HasForeignKey(x => x.DailyRateId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
