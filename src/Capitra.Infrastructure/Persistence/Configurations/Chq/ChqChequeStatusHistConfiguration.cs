using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Chq;

public sealed class ChqChequeStatusHistConfiguration : IEntityTypeConfiguration<ChqChequeStatusHist>
{
    public void Configure(EntityTypeBuilder<ChqChequeStatusHist> builder)
    {
        builder.ToTable("CHQ_CHEQUE_STATUS_HIST");
        builder.HasKey(x => x.ChequeStatusHistoryId);
        builder.HasIndex(x => new { x.ChequeId, x.StatusDate });

        builder.Property(x => x.ChequeStatusHistoryId).UseIdentityByDefaultColumn();
        builder.Property(x => x.OldStatusCode).HasMaxLength(50);
        builder.Property(x => x.NewStatusCode).HasMaxLength(50);
        builder.Property(x => x.BankStatementReference).HasMaxLength(100);
        builder.Property(x => x.ReasonCode).HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.ChangedBy).HasMaxLength(100);

        builder.HasOne<ChqCheque>().WithMany().HasForeignKey(x => x.ChequeId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
