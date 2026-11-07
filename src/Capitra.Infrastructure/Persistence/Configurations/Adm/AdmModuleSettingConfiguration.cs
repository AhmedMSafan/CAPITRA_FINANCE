using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmModuleSettingConfiguration : IEntityTypeConfiguration<AdmModuleSetting>
{
    public void Configure(EntityTypeBuilder<AdmModuleSetting> builder)
    {
        builder.ToTable("ADM_MODULE_SETTINGS");
        builder.HasKey(x => x.SettingId);
        builder.HasIndex(x => x.SettingCode).IsUnique();

        builder.Property(x => x.SettingId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ModuleCode).HasMaxLength(20);
        builder.Property(x => x.SettingCode).HasMaxLength(80);
        builder.Property(x => x.ValueText).HasMaxLength(200);
        builder.Property(x => x.PendingValue).HasMaxLength(200);
        builder.Property(x => x.PendingReason).HasMaxLength(1000);
        builder.Property(x => x.RequestedBy).HasMaxLength(256);
        builder.Property(x => x.ChangedBy).HasMaxLength(256);
        builder.Property(x => x.ApprovedBy).HasMaxLength(256);
        builder.ConfigureAudit();
    }
}

public sealed class AdmModuleSettingHistoryConfiguration : IEntityTypeConfiguration<AdmModuleSettingHistory>
{
    public void Configure(EntityTypeBuilder<AdmModuleSettingHistory> builder)
    {
        builder.ToTable("ADM_MODULE_SETTING_HISTORY");
        builder.HasKey(x => x.HistoryId);
        builder.HasIndex(x => x.SettingCode);

        builder.Property(x => x.HistoryId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SettingCode).HasMaxLength(80);
        builder.Property(x => x.ActionCode).HasMaxLength(20);
        builder.Property(x => x.OldValue).HasMaxLength(200);
        builder.Property(x => x.NewValue).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.ConfigureAudit();
    }
}
