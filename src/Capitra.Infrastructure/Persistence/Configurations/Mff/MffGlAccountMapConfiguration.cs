using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffGlAccountMapConfiguration : IEntityTypeConfiguration<MffGlAccountMap>
{
    public void Configure(EntityTypeBuilder<MffGlAccountMap> builder)
    {
        builder.ToTable("MFF_GL_ACCOUNT_MAPS");
        builder.HasKey(x => x.MapId);
        builder.HasIndex(x => new { x.FinancierId, x.EventCode, x.EffectiveFromDate }).IsUnique();
        builder.HasIndex(x => x.EventCode);
        builder.HasIndex(x => x.FinancierId);

        builder.Property(x => x.MapId).UseIdentityByDefaultColumn();
        builder.Property(x => x.EventCode).HasMaxLength(50);
        builder.Property(x => x.DebitAccount).HasMaxLength(100);
        builder.Property(x => x.CreditAccount).HasMaxLength(100);

        builder.HasOne<MffLkpAccountEvent>().WithMany().HasForeignKey(x => x.EventCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffOrganization>().WithMany().HasForeignKey(x => x.FinancierId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
