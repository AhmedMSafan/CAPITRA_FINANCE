using Capitra.Domain.Modules.Fin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fin;

public sealed class FinBusinessLineConfiguration : IEntityTypeConfiguration<FinBusinessLine>
{
    public void Configure(EntityTypeBuilder<FinBusinessLine> builder)
    {
        builder.ToTable("FIN_BUSINESS_LINES");
        builder.HasKey(x => x.BusinessLineId);
        builder.HasIndex(x => x.BusinessLineCode).IsUnique();

        builder.Property(x => x.BusinessLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.BusinessLineCode).HasMaxLength(30);
        builder.Property(x => x.BusinessLineName).HasMaxLength(300);
        builder.Property(x => x.BusinessLineNameAr).HasMaxLength(300);
        builder.Property(x => x.ModuleCode).HasMaxLength(30);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.DescriptionAr).HasMaxLength(2000);

        builder.ConfigureAudit();
    }
}
