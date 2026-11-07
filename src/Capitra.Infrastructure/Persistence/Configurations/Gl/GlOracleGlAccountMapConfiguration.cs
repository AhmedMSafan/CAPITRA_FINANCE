using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlOracleGlAccountMapConfiguration : IEntityTypeConfiguration<GlOracleGlAccountMap>
{
    public void Configure(EntityTypeBuilder<GlOracleGlAccountMap> builder)
    {
        builder.ToTable("GL_ORACLE_GL_ACCOUNT_MAPS");
        builder.HasKey(x => x.AccountMapId);
        builder.HasIndex(x => new { x.ExternalSystemId, x.AccountCombinationId }).IsUnique();

        builder.Property(x => x.AccountMapId).UseIdentityByDefaultColumn();
        builder.Property(x => x.OracleConcatenatedAccount).HasMaxLength(500);
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
        builder.Property(x => x.MappingStatus).HasMaxLength(30);
        builder.Property(x => x.ValidationMessage).HasMaxLength(2000);

        builder.HasOne<GlExternalGlSystem>().WithMany().HasForeignKey(x => x.ExternalSystemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
