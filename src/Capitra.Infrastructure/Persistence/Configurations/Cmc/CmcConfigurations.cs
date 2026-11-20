using Capitra.Domain.Modules.Cmc;
using Capitra.Infrastructure.Communication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Cmc;

public sealed class CmcMailServerConfiguration : IEntityTypeConfiguration<CmcMailServer>
{
    public void Configure(EntityTypeBuilder<CmcMailServer> builder)
    {
        builder.ToTable("CMC_MAIL_SERVERS");
        builder.HasKey(x => x.MailServerId);
        builder.Property(x => x.MailServerId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ServerName).HasMaxLength(100);
        builder.Property(x => x.HostName).HasMaxLength(255);
        builder.Property(x => x.UserName).HasMaxLength(256);
        builder.Property(x => x.PasswordProtected).HasMaxLength(2000);
        builder.Property(x => x.FromAddress).HasMaxLength(256);
        builder.Property(x => x.FromName).HasMaxLength(100);
        builder.Property(x => x.LastTestStatusCode).HasMaxLength(30);
        builder.Property(x => x.LastTestError).HasMaxLength(1000);
        // one active server at most
        builder.HasIndex(x => x.IsActive).IsUnique().HasFilter("\"IS_ACTIVE\"");
        builder.ConfigureAudit();
    }
}

public sealed class CmcEmailLogConfiguration : IEntityTypeConfiguration<CmcEmailLog>
{
    public void Configure(EntityTypeBuilder<CmcEmailLog> builder)
    {
        builder.ToTable("CMC_EMAIL_LOGS");
        builder.HasKey(x => x.EmailLogId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.AttemptedAt);
        builder.Property(x => x.EmailLogId).UseIdentityByDefaultColumn();
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.Property(x => x.ToAddress).HasMaxLength(256);
        builder.Property(x => x.KindCode).HasMaxLength(30);
        builder.Property(x => x.Subject).HasMaxLength(300);
        builder.Property(x => x.StatusCode).HasMaxLength(30);
        builder.Property(x => x.ErrorText).HasMaxLength(1000);
        builder.HasOne<CmcMailServer>().WithMany().HasForeignKey(x => x.MailServerId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class SysDataProtectionKeyConfiguration : IEntityTypeConfiguration<SysDataProtectionKey>
{
    public void Configure(EntityTypeBuilder<SysDataProtectionKey> builder)
    {
        builder.ToTable("SYS_DATA_PROTECTION_KEYS");
        builder.HasKey(x => x.DataProtectionKeyId);
        builder.Property(x => x.DataProtectionKeyId).UseIdentityByDefaultColumn();
        builder.Property(x => x.FriendlyName).HasMaxLength(200);
        builder.ConfigureAudit();
    }
}
