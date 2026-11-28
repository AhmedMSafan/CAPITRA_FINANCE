using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffLkpApplicationStatusConfiguration : IEntityTypeConfiguration<MffLkpApplicationStatus>
{
    public void Configure(EntityTypeBuilder<MffLkpApplicationStatus> builder)
    {
        builder.ToTable("MFF_LKP_APPLICATION_STATUSES");
        builder.HasKey(x => x.Code);

        builder.Property(x => x.Code).HasMaxLength(50);
        builder.Property(x => x.NameAr).HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);

        builder.ConfigureAudit();
    }
}
