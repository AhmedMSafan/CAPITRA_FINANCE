using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffEarlySettlementConfiguration : IEntityTypeConfiguration<MffEarlySettlement>
{
    public void Configure(EntityTypeBuilder<MffEarlySettlement> builder)
    {
        builder.ToTable("MFF_EARLY_SETTLEMENTS");
        builder.HasKey(x => x.SettlementId);
        builder.HasIndex(x => x.LoanId);

        builder.Property(x => x.SettlementId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SettlementType).HasMaxLength(20);
        builder.Property(x => x.PrincipalSettled).HasPrecision(18, 2);
        builder.Property(x => x.FinancePctSettled).HasPrecision(9, 4);
        builder.Property(x => x.SubsidyRefundPct).HasPrecision(9, 6);
        builder.Property(x => x.SubsidyRefundAmount).HasPrecision(18, 2);
        builder.Property(x => x.EarlySettlementStatus).HasMaxLength(30);
        builder.Property(x => x.ApprovedBy).HasMaxLength(100);
        builder.Property(x => x.NotesAr).HasMaxLength(1000);

        builder.HasOne<MffLoan>().WithMany().HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
