using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlOracleGlExportLineConfiguration : IEntityTypeConfiguration<GlOracleGlExportLine>
{
    public void Configure(EntityTypeBuilder<GlOracleGlExportLine> builder)
    {
        builder.ToTable("GL_ORACLE_GL_EXPORT_LINES");
        builder.HasKey(x => x.ExportLineId);
        builder.HasIndex(x => new { x.ExportHeaderId, x.JournalLineId }).IsUnique();
        builder.HasIndex(x => x.JournalLineId);

        builder.Property(x => x.ExportLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.OracleConcatenatedAccount).HasMaxLength(500);
        builder.Property(x => x.EnteredDr).HasPrecision(18, 4);
        builder.Property(x => x.EnteredCr).HasPrecision(18, 4);
        builder.Property(x => x.AccountedDr).HasPrecision(18, 4);
        builder.Property(x => x.AccountedCr).HasPrecision(18, 4);
        builder.Property(x => x.LineDescription).HasMaxLength(1000);
        builder.Property(x => x.Reference1).HasMaxLength(240);
        builder.Property(x => x.Reference2).HasMaxLength(240);
        builder.Property(x => x.Reference3).HasMaxLength(240);
        builder.Property(x => x.Reference4).HasMaxLength(240);
        builder.Property(x => x.Reference5).HasMaxLength(240);
        builder.Property(x => x.Reference6).HasMaxLength(240);
        builder.Property(x => x.Reference7).HasMaxLength(240);
        builder.Property(x => x.Reference8).HasMaxLength(240);
        builder.Property(x => x.Reference9).HasMaxLength(240);
        builder.Property(x => x.Reference10).HasMaxLength(240);
        builder.Property(x => x.LineStatus).HasMaxLength(30);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasOne<GlOracleGlExportHeader>().WithMany().HasForeignKey(x => x.ExportHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.JournalLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
