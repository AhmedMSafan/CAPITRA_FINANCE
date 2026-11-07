using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffProgramVersionConfiguration : IEntityTypeConfiguration<MffProgramVersion>
{
    public void Configure(EntityTypeBuilder<MffProgramVersion> builder)
    {
        builder.ToTable("MFF_PROGRAM_VERSIONS");
        builder.HasKey(x => x.ProgramVersionId);
        builder.HasIndex(x => new { x.ProgramCode, x.VersionNo }).IsUnique();
        builder.HasIndex(x => x.ProtocolId);

        builder.Property(x => x.ProgramVersionId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ProgramCode).HasMaxLength(50);
        builder.Property(x => x.VersionNo).HasMaxLength(30);
        builder.Property(x => x.ProgramNameAr).HasMaxLength(300);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.SourceReference).HasMaxLength(500);

        builder.HasOne<MffProtocol>().WithMany().HasForeignKey(x => x.ProtocolId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
