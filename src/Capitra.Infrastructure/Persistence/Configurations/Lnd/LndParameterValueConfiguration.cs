using Capitra.Domain.Modules.Lnd;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Lnd;

public sealed class LndParameterValueConfiguration : IEntityTypeConfiguration<LndParameterValue>
{
    public void Configure(EntityTypeBuilder<LndParameterValue> builder)
    {
        builder.ToTable("LND_PARAMETER_VALUES");
        builder.HasKey(x => x.ParameterValueId);
        builder.HasIndex(x => new { x.ParameterCode, x.ScopeCode, x.StatusCode });

        builder.Property(x => x.ParameterValueId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ParameterCode).HasMaxLength(80);
        builder.Property(x => x.ScopeCode).HasMaxLength(20);
        builder.Property(x => x.ValueText).HasMaxLength(200);
        builder.Property(x => x.DecisionReference).HasMaxLength(200);
        builder.Property(x => x.RequestReason).HasMaxLength(1000);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.RequestedBy).HasMaxLength(256);
        builder.Property(x => x.DecidedBy).HasMaxLength(256);
        builder.Property(x => x.DecisionNotes).HasMaxLength(1000);
        builder.ConfigureAudit();
    }
}
