using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlOracleGlImportRequestConfiguration : IEntityTypeConfiguration<GlOracleGlImportRequest>
{
    public void Configure(EntityTypeBuilder<GlOracleGlImportRequest> builder)
    {
        builder.ToTable("GL_ORACLE_GL_IMPORT_REQUESTS");
        builder.HasKey(x => x.ImportRequestId);

        builder.Property(x => x.ImportRequestId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RequestMethod).HasMaxLength(30);
        builder.Property(x => x.OracleRequestId).HasMaxLength(100);
        builder.Property(x => x.OracleProcessName).HasMaxLength(200);
        builder.Property(x => x.RequestStatus).HasMaxLength(30);
        builder.Property(x => x.SubmittedBy).HasMaxLength(100);
        builder.Property(x => x.OutputReference).HasMaxLength(1000);
        builder.Property(x => x.LogReference).HasMaxLength(1000);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasOne<GlOracleGlExportBatch>().WithMany().HasForeignKey(x => x.ExportBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
