using Capitra.Domain.Modules.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Crm;

public sealed class CrmLkpStatusCodeConfiguration : IEntityTypeConfiguration<CrmLkpStatusCode>
{
    public void Configure(EntityTypeBuilder<CrmLkpStatusCode> builder)
    {
        builder.ToTable("CRM_LKP_STATUS_CODES");
        builder.HasKey(x => x.StatusId);
        builder.HasIndex(x => x.EntityType);
        builder.HasIndex(x => x.ModuleCode);
        builder.HasIndex(x => new { x.StatusCode, x.ModuleCode, x.EntityType }).IsUnique();

        builder.Property(x => x.StatusId).UseIdentityByDefaultColumn();
        builder.Property(x => x.StatusCode).HasMaxLength(30);
        builder.Property(x => x.StatusName).HasMaxLength(200);
        builder.Property(x => x.StatusNameAr).HasMaxLength(200);
        builder.Property(x => x.StatusGroup).HasMaxLength(50);
        builder.Property(x => x.StatusGroupAr).HasMaxLength(50);
        builder.Property(x => x.ModuleCode).HasMaxLength(50);
        builder.Property(x => x.EntityType).HasMaxLength(50);
        builder.Property(x => x.ColorCode).HasMaxLength(20);
        builder.Property(x => x.IconClass).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.DescriptionAr).HasMaxLength(1000);

        builder.ConfigureAudit();
    }
}
