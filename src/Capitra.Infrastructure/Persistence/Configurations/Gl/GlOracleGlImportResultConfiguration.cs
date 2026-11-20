using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlOracleGlImportResultConfiguration : IEntityTypeConfiguration<GlOracleGlImportResult>
{
    public void Configure(EntityTypeBuilder<GlOracleGlImportResult> builder)
    {
        builder.ToTable("GL_ORACLE_GL_IMPORT_RESULTS");
        builder.HasKey(x => x.ImportResultId);
        builder.HasIndex(x => new { x.ExportBatchId, x.ResultStatus });

        builder.Property(x => x.ImportResultId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ResultLevel).HasMaxLength(30);
        builder.Property(x => x.ResultStatus).HasMaxLength(30);
        builder.Property(x => x.OraclePeriodName).HasMaxLength(30);
        builder.Property(x => x.ErrorCode).HasMaxLength(100);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasOne<GlOracleGlImportRequest>().WithMany().HasForeignKey(x => x.ImportRequestId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlOracleGlExportBatch>().WithMany().HasForeignKey(x => x.ExportBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlOracleGlExportHeader>().WithMany().HasForeignKey(x => x.ExportHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlOracleGlExportLine>().WithMany().HasForeignKey(x => x.ExportLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlOracleGlInterfaceRow>().WithMany().HasForeignKey(x => x.InterfaceRowId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
