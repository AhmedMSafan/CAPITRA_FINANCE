using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Chq;

public sealed class ChqLkpEventTypeConfiguration : IEntityTypeConfiguration<ChqLkpEventType>
{
    public void Configure(EntityTypeBuilder<ChqLkpEventType> builder)
    {
        builder.ToTable("CHQ_LKP_EVENT_TYPES");
        builder.HasKey(x => x.EventType);

        builder.Property(x => x.EventType).HasMaxLength(30);
        builder.Property(x => x.EventDescEn).HasMaxLength(200);
        builder.Property(x => x.EventDescAr).HasMaxLength(200);

        builder.ConfigureAudit();
    }
}
