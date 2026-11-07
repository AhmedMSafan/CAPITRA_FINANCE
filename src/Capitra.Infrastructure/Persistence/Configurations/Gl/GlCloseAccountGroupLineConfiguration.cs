using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseAccountGroupLineConfiguration : IEntityTypeConfiguration<GlCloseAccountGroupLine>
{
    public void Configure(EntityTypeBuilder<GlCloseAccountGroupLine> builder)
    {
        builder.ToTable("GL_CLOSE_ACCOUNT_GROUP_LINES");
        builder.HasKey(x => x.AccountGroupLineId);

        builder.Property(x => x.AccountGroupLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.AccountCodeFrom).HasMaxLength(50);
        builder.Property(x => x.AccountCodeTo).HasMaxLength(50);
        builder.Property(x => x.IncludeExcludeFlag).HasMaxLength(10);

        builder.HasOne<GlCloseAccountGroup>().WithMany().HasForeignKey(x => x.AccountGroupId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccount>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
