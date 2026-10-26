using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlOracleGlExportBatchConfiguration : IEntityTypeConfiguration<GlOracleGlExportBatch>
{
    public void Configure(EntityTypeBuilder<GlOracleGlExportBatch> builder)
    {
        builder.ToTable("GL_ORACLE_GL_EXPORT_BATCHES");
        builder.HasKey(x => x.ExportBatchId);
        builder.HasIndex(x => new { x.ExternalSystemId, x.ExportBatchNo }).IsUnique();
        builder.HasIndex(x => new { x.ExternalSystemId, x.ExportStatus });
        builder.HasIndex(x => x.GroupId);

        builder.Property(x => x.ExportBatchId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ExportBatchNo).HasMaxLength(50);
        builder.Property(x => x.ExportType).HasMaxLength(30);
        builder.Property(x => x.ExportStatus).HasMaxLength(30);
        builder.Property(x => x.OracleImportRequestId).HasMaxLength(100);
        builder.Property(x => x.ExportFileName).HasMaxLength(500);
        builder.Property(x => x.ExportFileMimeType).HasMaxLength(200);
        builder.Property(x => x.TotalHeaders).HasPrecision(28, 2);
        builder.Property(x => x.TotalLines).HasPrecision(28, 2);
        builder.Property(x => x.TotalEnteredDr).HasPrecision(18, 4);
        builder.Property(x => x.TotalEnteredCr).HasPrecision(18, 4);
        builder.Property(x => x.TotalAccountedDr).HasPrecision(18, 4);
        builder.Property(x => x.TotalAccountedCr).HasPrecision(18, 4);
        builder.Property(x => x.ValidationStatus).HasMaxLength(30);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);
        builder.Property(x => x.ValidatedBy).HasMaxLength(100);
        builder.Property(x => x.ExportedBy).HasMaxLength(100);

        builder.HasOne<GlExternalGlSystem>().WithMany().HasForeignKey(x => x.ExternalSystemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlOracleGlExportProfile>().WithMany().HasForeignKey(x => x.ExportProfileId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
