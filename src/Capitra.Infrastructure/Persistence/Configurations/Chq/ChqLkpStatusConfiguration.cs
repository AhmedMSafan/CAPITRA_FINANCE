using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Chq;

public sealed class ChqLkpStatusConfiguration : IEntityTypeConfiguration<ChqLkpStatus>
{
    public void Configure(EntityTypeBuilder<ChqLkpStatus> builder)
    {
        builder.ToTable("CHQ_LKP_STATUSES");
        builder.HasKey(x => x.StatusCode);

        builder.Property(x => x.StatusCode).HasMaxLength(50);
        builder.Property(x => x.NameEn).HasMaxLength(300);
        builder.Property(x => x.NameAr).HasMaxLength(300);

        builder.ConfigureAudit();
    }
}
