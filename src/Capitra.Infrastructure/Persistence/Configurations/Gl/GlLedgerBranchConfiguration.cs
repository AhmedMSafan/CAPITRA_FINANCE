using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlLedgerBranchConfiguration : IEntityTypeConfiguration<GlLedgerBranch>
{
    public void Configure(EntityTypeBuilder<GlLedgerBranch> builder)
    {
        builder.ToTable("GL_LEDGER_BRANCHES");
        builder.HasKey(x => x.LedgerBranchId);
        builder.HasIndex(x => new { x.LedgerId, x.BranchId }).IsUnique();
        builder.HasIndex(x => new { x.BranchId, x.IsActive });

        builder.Property(x => x.LedgerBranchId).UseIdentityByDefaultColumn();

        builder.HasOne<FinBranch>().WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
