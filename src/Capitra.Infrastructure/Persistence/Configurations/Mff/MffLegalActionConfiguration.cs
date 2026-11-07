using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffLegalActionConfiguration : IEntityTypeConfiguration<MffLegalAction>
{
    public void Configure(EntityTypeBuilder<MffLegalAction> builder)
    {
        builder.ToTable("MFF_LEGAL_ACTIONS");
        builder.HasKey(x => x.LegalActionId);
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.LoanId);

        builder.Property(x => x.LegalActionId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ActionType).HasMaxLength(50);
        builder.Property(x => x.CaseNo).HasMaxLength(100);
        builder.Property(x => x.LegalActionStatus).HasMaxLength(30);
        builder.Property(x => x.OutcomeAr).HasMaxLength(2000);

        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLoan>().WithMany().HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
