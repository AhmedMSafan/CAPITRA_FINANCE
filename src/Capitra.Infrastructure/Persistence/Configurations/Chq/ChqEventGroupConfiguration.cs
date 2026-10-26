using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Chq;

public sealed class ChqEventGroupConfiguration : IEntityTypeConfiguration<ChqEventGroup>
{
    public void Configure(EntityTypeBuilder<ChqEventGroup> builder)
    {
        builder.ToTable("CHQ_EVENT_GROUPS");
        builder.HasKey(x => x.MovementGroupId);

        builder.Property(x => x.MovementGroupId).UseIdentityByDefaultColumn();
        builder.Property(x => x.EventType).HasMaxLength(30);
        builder.Property(x => x.ToLocationCode).HasMaxLength(50);
        builder.Property(x => x.MovementRef).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.ConfigureAudit();
    }
}
