using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffLoanEventConfiguration : IEntityTypeConfiguration<MffLoanEvent>
{
    public void Configure(EntityTypeBuilder<MffLoanEvent> builder)
    {
        builder.ToTable("MFF_LOAN_EVENTS");
        builder.HasKey(x => x.LoanEventId);
        builder.HasIndex(x => x.EventTypeCode);
        builder.HasIndex(x => x.LoanId);

        builder.Property(x => x.LoanEventId).UseIdentityByDefaultColumn();
        builder.Property(x => x.EventTypeCode).HasMaxLength(30);
        builder.Property(x => x.LoanEventStatus).HasMaxLength(30);
        builder.Property(x => x.DetailsAr).HasMaxLength(2000);
        builder.Property(x => x.ResolutionAr).HasMaxLength(2000);

        builder.HasOne<MffLkpEventType>().WithMany().HasForeignKey(x => x.EventTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLoan>().WithMany().HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
