using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlRateImportBatchConfiguration : IEntityTypeConfiguration<GlRateImportBatch>
{
    public void Configure(EntityTypeBuilder<GlRateImportBatch> builder)
    {
        builder.ToTable("GL_RATE_IMPORT_BATCHES");
        builder.HasKey(x => x.ImportBatchId);
        builder.HasIndex(x => x.BatchCode).IsUnique();

        builder.Property(x => x.ImportBatchId).UseIdentityByDefaultColumn();
        builder.Property(x => x.BatchCode).HasMaxLength(50);
        builder.Property(x => x.BatchName).HasMaxLength(240);
        builder.Property(x => x.SourceCode).HasMaxLength(30);
        builder.Property(x => x.FileName).HasMaxLength(400);
        builder.Property(x => x.ImportStatus).HasMaxLength(30);
        builder.Property(x => x.ImportedBy).HasMaxLength(100);
        builder.Property(x => x.PostedBy).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<GlRateType>().WithMany().HasForeignKey(x => x.RateTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
