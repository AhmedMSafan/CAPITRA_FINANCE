using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffApplicationPropertyConfiguration : IEntityTypeConfiguration<MffApplicationProperty>
{
    public void Configure(EntityTypeBuilder<MffApplicationProperty> builder)
    {
        builder.ToTable("MFF_APPLICATION_PROPERTIES");
        builder.HasKey(x => x.AppPropertyId);
        builder.HasIndex(x => new { x.ApplicationId, x.PropertyId }).IsUnique();
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.PropertyId);

        builder.Property(x => x.AppPropertyId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RequestedPrice).HasPrecision(18, 2);

        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffProperty>().WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
