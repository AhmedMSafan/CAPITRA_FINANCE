using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseAccountGroupConfiguration : IEntityTypeConfiguration<GlCloseAccountGroup>
{
    public void Configure(EntityTypeBuilder<GlCloseAccountGroup> builder)
    {
        builder.ToTable("GL_CLOSE_ACCOUNT_GROUPS");
        builder.HasKey(x => x.AccountGroupId);
        builder.HasIndex(x => x.AccountGroupCode).IsUnique();

        builder.Property(x => x.AccountGroupId).UseIdentityByDefaultColumn();
        builder.Property(x => x.AccountGroupCode).HasMaxLength(50);
        builder.Property(x => x.AccountGroupNameEn).HasMaxLength(200);
        builder.Property(x => x.AccountGroupNameAr).HasMaxLength(200);
        builder.Property(x => x.AccountGroupType).HasMaxLength(30);
        builder.Property(x => x.NormalBalance).HasMaxLength(2);
        builder.Property(x => x.CloseBehavior).HasMaxLength(30);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.ConfigureAudit();
    }
}
