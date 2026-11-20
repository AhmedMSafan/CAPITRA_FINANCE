using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlOracleGlCategoryMapConfiguration : IEntityTypeConfiguration<GlOracleGlCategoryMap>
{
    public void Configure(EntityTypeBuilder<GlOracleGlCategoryMap> builder)
    {
        builder.ToTable("GL_ORACLE_GL_CATEGORY_MAPS");
        builder.HasKey(x => x.CategoryMapId);
        builder.HasIndex(x => new { x.ExternalSystemId, x.FinovaCategoryCode }).IsUnique();

        builder.Property(x => x.CategoryMapId).UseIdentityByDefaultColumn();
        builder.Property(x => x.FinovaCategoryCode).HasMaxLength(50);
        builder.Property(x => x.OracleJeCategoryName).HasMaxLength(100);
        builder.Property(x => x.OracleCategoryKey).HasMaxLength(100);

        builder.HasOne<GlExternalGlSystem>().WithMany().HasForeignKey(x => x.ExternalSystemId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
