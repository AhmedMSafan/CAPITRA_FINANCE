using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffProtocolPartyConfiguration : IEntityTypeConfiguration<MffProtocolParty>
{
    public void Configure(EntityTypeBuilder<MffProtocolParty> builder)
    {
        builder.ToTable("MFF_PROTOCOL_PARTIES");
        builder.HasKey(x => x.ProtocolPartyId);
        builder.HasIndex(x => new { x.ProtocolId, x.OrgId }).IsUnique();
        builder.HasIndex(x => x.ProtocolId);
        builder.HasIndex(x => x.OrgId);

        builder.Property(x => x.ProtocolPartyId).UseIdentityByDefaultColumn();
        builder.Property(x => x.PartyRole).HasMaxLength(100);

        builder.HasOne<MffProtocol>().WithMany().HasForeignKey(x => x.ProtocolId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffOrganization>().WithMany().HasForeignKey(x => x.OrgId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
