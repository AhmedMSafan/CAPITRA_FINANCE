using Capitra.Domain.Modules.Lnd;
using Capitra.Domain.Modules.Trs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Lnd;

public sealed class LndDisbursementConfiguration : IEntityTypeConfiguration<LndDisbursement>
{
    public void Configure(EntityTypeBuilder<LndDisbursement> builder)
    {
        builder.ToTable("LND_DISBURSEMENTS");
        builder.HasKey(x => x.DisbursementId);
        builder.HasIndex(x => new { x.LoanId, x.StatusCode });
        builder.HasIndex(x => x.StatusCode);

        builder.Property(x => x.DisbursementId).UseIdentityByDefaultColumn();
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.PayoutMethodCode).HasMaxLength(20);
        builder.Property(x => x.PayeeName).HasMaxLength(200);
        builder.Property(x => x.PayeeAccount).HasMaxLength(100);
        builder.Property(x => x.GlTranSetId).HasMaxLength(40);
        builder.Property(x => x.RequestedBy).HasMaxLength(256);
        builder.Property(x => x.DecidedBy).HasMaxLength(256);
        builder.Property(x => x.DecisionNotes).HasMaxLength(1000);
        builder.HasOne<LndLoan>().WithMany().HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TrsBankAccount>().WithMany().HasForeignKey(x => x.PaidFromAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class LndDisbursementChargeConfiguration : IEntityTypeConfiguration<LndDisbursementCharge>
{
    public void Configure(EntityTypeBuilder<LndDisbursementCharge> builder)
    {
        builder.ToTable("LND_DISBURSEMENT_CHARGES");
        builder.HasKey(x => x.DisbursementChargeId);
        builder.HasIndex(x => x.DisbursementId);

        builder.Property(x => x.DisbursementChargeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ChargeCode).HasMaxLength(50);
        builder.Property(x => x.ChargeName).HasMaxLength(200);
        builder.Property(x => x.IncomeRecognitionCode).HasMaxLength(20);
        builder.HasOne<LndDisbursement>().WithMany().HasForeignKey(x => x.DisbursementId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class LndLoanEventConfiguration : IEntityTypeConfiguration<LndLoanEvent>
{
    public void Configure(EntityTypeBuilder<LndLoanEvent> builder)
    {
        builder.ToTable("LND_LOAN_EVENTS");
        builder.HasKey(x => x.LoanEventId);
        builder.HasIndex(x => new { x.LoanId, x.EventDate });
        builder.HasIndex(x => x.GlTranSetId);

        builder.Property(x => x.LoanEventId).UseIdentityByDefaultColumn();
        builder.Property(x => x.EventCode).HasMaxLength(30);
        builder.Property(x => x.GlTranSetId).HasMaxLength(40);
        builder.Property(x => x.SourceDocumentType).HasMaxLength(30);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasOne<LndLoan>().WithMany().HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
