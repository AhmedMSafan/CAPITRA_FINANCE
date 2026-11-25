using Capitra.Domain.Modules.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Crm;

public sealed class CrmRecentClientConfiguration : IEntityTypeConfiguration<CrmRecentClient>
{
    public void Configure(EntityTypeBuilder<CrmRecentClient> builder)
    {
        builder.ToTable("CRM_RECENT_CLIENTS");
        builder.HasKey(x => x.RecentClientId);
        builder.HasIndex(x => new { x.UserName, x.ClientId }).IsUnique();
        builder.HasIndex(x => x.ClientId);

        builder.Property(x => x.RecentClientId).UseIdentityByDefaultColumn();
        builder.Property(x => x.UserName).HasMaxLength(256);

        builder.HasOne<CrmClient>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
