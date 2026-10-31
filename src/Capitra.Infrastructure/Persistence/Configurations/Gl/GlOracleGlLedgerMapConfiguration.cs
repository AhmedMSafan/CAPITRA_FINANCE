using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlOracleGlLedgerMapConfiguration : IEntityTypeConfiguration<GlOracleGlLedgerMap>
{
    public void Configure(EntityTypeBuilder<GlOracleGlLedgerMap> builder)
    {
        builder.ToTable("GL_ORACLE_GL_LEDGER_MAPS");
        builder.HasKey(x => x.LedgerMapId);
        builder.HasIndex(x => new { x.ExternalSystemId, x.LedgerId }).IsUnique();

        builder.Property(x => x.LedgerMapId).UseIdentityByDefaultColumn();
        builder.Property(x => x.OracleLedgerName).HasMaxLength(200);
        builder.Property(x => x.OracleLedgerCurrency).HasMaxLength(15);

        builder.HasOne<GlExternalGlSystem>().WithMany().HasForeignKey(x => x.ExternalSystemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
