using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffAuditLogConfiguration : IEntityTypeConfiguration<MffAuditLog>
{
    public void Configure(EntityTypeBuilder<MffAuditLog> builder)
    {
        builder.ToTable("MFF_AUDIT_LOGS");
        builder.HasKey(x => x.AuditId);

        builder.Property(x => x.AuditId).UseIdentityByDefaultColumn();
        builder.Property(x => x.TableName).HasMaxLength(128);
        builder.Property(x => x.RecordKey).HasMaxLength(500);
        builder.Property(x => x.ActionType).HasMaxLength(20);
        builder.Property(x => x.ActionBy).HasMaxLength(100);
        builder.Property(x => x.SessionId).HasMaxLength(100);
        builder.Property(x => x.ClientIp).HasMaxLength(100);

        builder.ConfigureAudit();
    }
}
