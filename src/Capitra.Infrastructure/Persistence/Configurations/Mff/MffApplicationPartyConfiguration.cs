using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffApplicationPartyConfiguration : IEntityTypeConfiguration<MffApplicationParty>
{
    public void Configure(EntityTypeBuilder<MffApplicationParty> builder)
    {
        builder.ToTable("MFF_APPLICATION_PARTIES");
        builder.HasKey(x => x.ApplicationPartyId);
        builder.HasIndex(x => new { x.ApplicationId, x.PersonId, x.PartyRoleCode }).IsUnique();
        builder.HasIndex(x => x.PersonId);
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.PartyRoleCode);

        builder.Property(x => x.ApplicationPartyId).UseIdentityByDefaultColumn();
        builder.Property(x => x.PartyRoleCode).HasMaxLength(50);
        builder.Property(x => x.OwnershipSharePct).HasPrecision(9, 6);

        builder.HasOne<MffLkpPartyRole>().WithMany().HasForeignKey(x => x.PartyRoleCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffPerson>().WithMany().HasForeignKey(x => x.PersonId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
