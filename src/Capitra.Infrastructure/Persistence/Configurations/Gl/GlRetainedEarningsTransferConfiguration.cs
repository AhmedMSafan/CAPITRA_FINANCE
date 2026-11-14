using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlRetainedEarningsTransferConfiguration : IEntityTypeConfiguration<GlRetainedEarningsTransfer>
{
    public void Configure(EntityTypeBuilder<GlRetainedEarningsTransfer> builder)
    {
        builder.ToTable("GL_RETAINED_EARNINGS_TRANSFERS");
        builder.HasKey(x => x.RetainedTransferId);

        builder.Property(x => x.RetainedTransferId).UseIdentityByDefaultColumn();
        builder.Property(x => x.TransferDrAmount).HasPrecision(18, 2);
        builder.Property(x => x.TransferCrAmount).HasPrecision(18, 2);
        builder.Property(x => x.TransferStatus).HasMaxLength(30);

        builder.HasOne<GlYearEndCloseRun>().WithMany().HasForeignKey(x => x.YearEndRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.RetainedEarningsAccountCombId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.JournalLineId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
