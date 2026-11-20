using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffProtocolConfiguration : IEntityTypeConfiguration<MffProtocol>
{
    public void Configure(EntityTypeBuilder<MffProtocol> builder)
    {
        builder.ToTable("MFF_PROTOCOLS");
        builder.HasKey(x => x.ProtocolId);
        builder.HasIndex(x => x.ProtocolCode).IsUnique();
        builder.HasIndex(x => x.FundOrgId);

        builder.Property(x => x.ProtocolId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ProtocolCode).HasMaxLength(50);
        builder.Property(x => x.ProtocolNameAr).HasMaxLength(300);
        builder.Property(x => x.LegalReference).HasMaxLength(500);
        builder.Property(x => x.SourceReference).HasMaxLength(500);

        builder.HasOne<MffOrganization>().WithMany().HasForeignKey(x => x.FundOrgId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
