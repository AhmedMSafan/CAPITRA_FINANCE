using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlOracleGlExportHeaderConfiguration : IEntityTypeConfiguration<GlOracleGlExportHeader>
{
    public void Configure(EntityTypeBuilder<GlOracleGlExportHeader> builder)
    {
        builder.ToTable("GL_ORACLE_GL_EXPORT_HEADERS");
        builder.HasKey(x => x.ExportHeaderId);
        builder.HasIndex(x => new { x.ExportBatchId, x.JournalHeaderId }).IsUnique();
        builder.HasIndex(x => x.JournalHeaderId);
        builder.HasIndex(x => x.DuplicateKey);

        builder.Property(x => x.ExportHeaderId).UseIdentityByDefaultColumn();
        builder.Property(x => x.JournalNo).HasMaxLength(100);
        builder.Property(x => x.OracleJournalName).HasMaxLength(200);
        builder.Property(x => x.OracleJournalDescription).HasMaxLength(1000);
        builder.Property(x => x.OracleJeSourceName).HasMaxLength(100);
        builder.Property(x => x.OracleJeCategoryName).HasMaxLength(100);
        builder.Property(x => x.PeriodName).HasMaxLength(30);
        builder.Property(x => x.CurrencyCode).HasMaxLength(15);
        builder.Property(x => x.HeaderStatus).HasMaxLength(30);
        builder.Property(x => x.DuplicateKey).HasMaxLength(300);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasOne<GlOracleGlExportBatch>().WithMany().HasForeignKey(x => x.ExportBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.JournalBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
