using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffLkpDelinquencyStageConfiguration : IEntityTypeConfiguration<MffLkpDelinquencyStage>
{
    public void Configure(EntityTypeBuilder<MffLkpDelinquencyStage> builder)
    {
        builder.ToTable("MFF_LKP_DELINQUENCY_STAGES");
        builder.HasKey(x => x.Code);

        builder.Property(x => x.Code).HasMaxLength(50);
        builder.Property(x => x.NameAr).HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);

        builder.ConfigureAudit();
    }
}
