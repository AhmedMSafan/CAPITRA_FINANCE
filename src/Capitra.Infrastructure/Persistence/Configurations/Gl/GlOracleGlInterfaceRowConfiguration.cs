using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlOracleGlInterfaceRowConfiguration : IEntityTypeConfiguration<GlOracleGlInterfaceRow>
{
    public void Configure(EntityTypeBuilder<GlOracleGlInterfaceRow> builder)
    {
        builder.ToTable("GL_ORACLE_GL_INTERFACE_ROWS");
        builder.HasKey(x => x.InterfaceRowId);
        builder.HasIndex(x => x.ExportLineId).IsUnique();
        builder.HasIndex(x => new { x.ExportBatchId, x.InterfaceStatus });
        builder.HasIndex(x => new { x.GroupId, x.OracleStatus });

        builder.Property(x => x.InterfaceRowId).UseIdentityByDefaultColumn();
        builder.Property(x => x.InterfaceStatus).HasMaxLength(30);
        builder.Property(x => x.OracleStatus).HasMaxLength(30);
        builder.Property(x => x.LedgerName).HasMaxLength(200);
        builder.Property(x => x.CurrencyCode).HasMaxLength(15);
        builder.Property(x => x.UserJeSourceName).HasMaxLength(100);
        builder.Property(x => x.UserJeCategoryName).HasMaxLength(100);
        builder.Property(x => x.EnteredDr).HasPrecision(18, 4);
        builder.Property(x => x.EnteredCr).HasPrecision(18, 4);
        builder.Property(x => x.AccountedDr).HasPrecision(18, 4);
        builder.Property(x => x.AccountedCr).HasPrecision(18, 4);
        builder.Property(x => x.Segment1).HasMaxLength(100);
        builder.Property(x => x.Segment2).HasMaxLength(100);
        builder.Property(x => x.Segment3).HasMaxLength(100);
        builder.Property(x => x.Segment4).HasMaxLength(100);
        builder.Property(x => x.Segment5).HasMaxLength(100);
        builder.Property(x => x.Segment6).HasMaxLength(100);
        builder.Property(x => x.Segment7).HasMaxLength(100);
        builder.Property(x => x.Segment8).HasMaxLength(100);
        builder.Property(x => x.Segment9).HasMaxLength(100);
        builder.Property(x => x.Segment10).HasMaxLength(100);
        builder.Property(x => x.Segment11).HasMaxLength(100);
        builder.Property(x => x.Segment12).HasMaxLength(100);
        builder.Property(x => x.Segment13).HasMaxLength(100);
        builder.Property(x => x.Segment14).HasMaxLength(100);
        builder.Property(x => x.Segment15).HasMaxLength(100);
        builder.Property(x => x.Segment16).HasMaxLength(100);
        builder.Property(x => x.Segment17).HasMaxLength(100);
        builder.Property(x => x.Segment18).HasMaxLength(100);
        builder.Property(x => x.Segment19).HasMaxLength(100);
        builder.Property(x => x.Segment20).HasMaxLength(100);
        builder.Property(x => x.Segment21).HasMaxLength(100);
        builder.Property(x => x.Segment22).HasMaxLength(100);
        builder.Property(x => x.Segment23).HasMaxLength(100);
        builder.Property(x => x.Segment24).HasMaxLength(100);
        builder.Property(x => x.Segment25).HasMaxLength(100);
        builder.Property(x => x.Segment26).HasMaxLength(100);
        builder.Property(x => x.Segment27).HasMaxLength(100);
        builder.Property(x => x.Segment28).HasMaxLength(100);
        builder.Property(x => x.Segment29).HasMaxLength(100);
        builder.Property(x => x.Segment30).HasMaxLength(100);
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
        builder.Property(x => x.ErrorCode).HasMaxLength(100);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasOne<GlOracleGlExportBatch>().WithMany().HasForeignKey(x => x.ExportBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlOracleGlExportLine>().WithMany().HasForeignKey(x => x.ExportLineId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
