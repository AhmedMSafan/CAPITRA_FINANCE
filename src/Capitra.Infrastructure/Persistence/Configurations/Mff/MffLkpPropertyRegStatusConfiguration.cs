using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffLkpPropertyRegStatusConfiguration : IEntityTypeConfiguration<MffLkpPropertyRegStatus>
{
    public void Configure(EntityTypeBuilder<MffLkpPropertyRegStatus> builder)
    {
        builder.ToTable("MFF_LKP_PROPERTY_REG_STATUSES");
        builder.HasKey(x => x.Code);

        builder.Property(x => x.Code).HasMaxLength(50);
        builder.Property(x => x.NameAr).HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);

        builder.ConfigureAudit();
    }
}
