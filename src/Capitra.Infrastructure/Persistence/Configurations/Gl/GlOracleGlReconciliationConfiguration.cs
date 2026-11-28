using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlOracleGlReconciliationConfiguration : IEntityTypeConfiguration<GlOracleGlReconciliation>
{
    public void Configure(EntityTypeBuilder<GlOracleGlReconciliation> builder)
    {
        builder.ToTable("GL_ORACLE_GL_RECONCILIATIONS");
        builder.HasKey(x => x.ReconciliationId);
        builder.HasIndex(x => x.ExportBatchId).IsUnique();

        builder.Property(x => x.ReconciliationId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ReconciliationStatus).HasMaxLength(30);
        builder.Property(x => x.FinovaTotalDr).HasPrecision(18, 4);
        builder.Property(x => x.FinovaTotalCr).HasPrecision(18, 4);
        builder.Property(x => x.OracleImportedDr).HasPrecision(18, 4);
        builder.Property(x => x.OracleImportedCr).HasPrecision(18, 4);
        builder.Property(x => x.DifferenceDr).HasPrecision(18, 4);
        builder.Property(x => x.DifferenceCr).HasPrecision(18, 4);
        builder.Property(x => x.ReconciledBy).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasOne<GlOracleGlExportBatch>().WithMany().HasForeignKey(x => x.ExportBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
