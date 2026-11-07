using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffIntegrationLogConfiguration : IEntityTypeConfiguration<MffIntegrationLog>
{
    public void Configure(EntityTypeBuilder<MffIntegrationLog> builder)
    {
        builder.ToTable("MFF_INTEGRATION_LOGS");
        builder.HasKey(x => x.IntegrationLogId);

        builder.Property(x => x.IntegrationLogId).UseIdentityByDefaultColumn();
        builder.Property(x => x.InterfaceName).HasMaxLength(100);
        builder.Property(x => x.Direction).HasMaxLength(10);
        builder.Property(x => x.ReferenceKey).HasMaxLength(200);
        builder.Property(x => x.IntegrationLogStatus).HasMaxLength(30);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.ConfigureAudit();
    }
}
