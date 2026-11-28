using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlOracleGlValidationResultConfiguration : IEntityTypeConfiguration<GlOracleGlValidationResult>
{
    public void Configure(EntityTypeBuilder<GlOracleGlValidationResult> builder)
    {
        builder.ToTable("GL_ORACLE_GL_VALIDATION_RESULTS");
        builder.HasKey(x => x.ValidationResultId);

        builder.Property(x => x.ValidationResultId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ValidationCode).HasMaxLength(50);
        builder.Property(x => x.ValidationName).HasMaxLength(200);
        builder.Property(x => x.Severity).HasMaxLength(30);
        builder.Property(x => x.ValidationStatus).HasMaxLength(30);
        builder.Property(x => x.ValidationMessage).HasMaxLength(4000);

        builder.HasOne<GlOracleGlExportBatch>().WithMany().HasForeignKey(x => x.ExportBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlOracleGlExportHeader>().WithMany().HasForeignKey(x => x.ExportHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlOracleGlExportLine>().WithMany().HasForeignKey(x => x.ExportLineId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
