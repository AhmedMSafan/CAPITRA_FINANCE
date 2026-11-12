using Capitra.Domain.Modules.Lnd;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Lnd;

public sealed class LndWriteOffConfiguration : IEntityTypeConfiguration<LndWriteOff>
{
    public void Configure(EntityTypeBuilder<LndWriteOff> builder)
    {
        builder.ToTable("LND_WRITE_OFFS");
        builder.HasKey(x => x.WriteOffId);
        builder.HasIndex(x => new { x.LoanId, x.StatusCode });
        builder.HasIndex(x => x.StatusCode);

        builder.Property(x => x.WriteOffId).UseIdentityByDefaultColumn();
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.RequestedBy).HasMaxLength(100);
        builder.Property(x => x.DecidedBy).HasMaxLength(100);
        builder.Property(x => x.DecisionNotes).HasMaxLength(1000);
        builder.Property(x => x.GlTranSetId).HasMaxLength(40);
        builder.HasOne<LndLoan>().WithMany().HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
