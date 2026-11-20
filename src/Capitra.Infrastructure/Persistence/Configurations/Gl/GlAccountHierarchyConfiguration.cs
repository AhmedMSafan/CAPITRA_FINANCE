using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAccountHierarchyConfiguration : IEntityTypeConfiguration<GlAccountHierarchy>
{
    public void Configure(EntityTypeBuilder<GlAccountHierarchy> builder)
    {
        builder.ToTable("GL_ACCOUNT_HIERARCHIES");
        builder.HasKey(x => x.HierarchyId);
        builder.HasIndex(x => new { x.CoaId, x.HierarchyCode }).IsUnique();
        builder.HasIndex(x => new { x.CoaId, x.HierarchyType, x.IsActive });

        builder.Property(x => x.HierarchyId).UseIdentityByDefaultColumn();
        builder.Property(x => x.HierarchyCode).HasMaxLength(30);
        builder.Property(x => x.HierarchyNameEn).HasMaxLength(240);
        builder.Property(x => x.HierarchyNameAr).HasMaxLength(240);
        builder.Property(x => x.HierarchyType).HasMaxLength(30);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<GlCoaStructure>().WithMany().HasForeignKey(x => x.CoaId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
