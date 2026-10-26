using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Chq;

public sealed class ChqUploadLineConfiguration : IEntityTypeConfiguration<ChqUploadLine>
{
    public void Configure(EntityTypeBuilder<ChqUploadLine> builder)
    {
        builder.ToTable("CHQ_UPLOAD_LINES");
        builder.HasKey(x => x.LineId);
        builder.HasIndex(x => new { x.ChequeNo, x.IssuerBankCode });
        builder.HasIndex(x => new { x.BatchId, x.LineStatus });
        builder.HasIndex(x => new { x.BatchId, x.LineNo }).IsUnique();

        builder.Property(x => x.LineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RawSr).HasMaxLength(100);
        builder.Property(x => x.RawClientId).HasMaxLength(100);
        builder.Property(x => x.RawPartyTaxId).HasMaxLength(100);
        builder.Property(x => x.RawChequeNo).HasMaxLength(100);
        builder.Property(x => x.RawDueDate).HasMaxLength(100);
        builder.Property(x => x.RawChequeAmount).HasMaxLength(100);
        builder.Property(x => x.RawIssuerBankCode).HasMaxLength(100);
        builder.Property(x => x.ClientId).HasMaxLength(100);
        builder.Property(x => x.PartyTaxId).HasMaxLength(20);
        builder.Property(x => x.ChequeNo).HasMaxLength(100);
        builder.Property(x => x.ChequeAmount).HasPrecision(18, 2);
        builder.Property(x => x.IssuerBankCode).HasMaxLength(30);
        builder.Property(x => x.IssuerBankName).HasMaxLength(300);
        builder.Property(x => x.LineStatus).HasMaxLength(30);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);
        builder.Property(x => x.TargetChequeRef).HasMaxLength(50);

        builder.HasOne<ChqUploadBatch>().WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
