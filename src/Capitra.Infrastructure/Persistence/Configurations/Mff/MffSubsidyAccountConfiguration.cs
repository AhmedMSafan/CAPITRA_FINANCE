using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffSubsidyAccountConfiguration : IEntityTypeConfiguration<MffSubsidyAccount>
{
    public void Configure(EntityTypeBuilder<MffSubsidyAccount> builder)
    {
        builder.ToTable("MFF_SUBSIDY_ACCOUNTS");
        builder.HasKey(x => x.SubsidyAccountId);
        builder.HasIndex(x => x.AccountNo).IsUnique();
        builder.HasIndex(x => x.LoanId);

        builder.Property(x => x.SubsidyAccountId).UseIdentityByDefaultColumn();
        builder.Property(x => x.AccountNo).HasMaxLength(100);
        builder.Property(x => x.OpeningAmount).HasPrecision(18, 2);
        builder.Property(x => x.CurrentBalance).HasPrecision(18, 2);
        builder.Property(x => x.AccountStatus).HasMaxLength(30);
        builder.Property(x => x.CoreSystemRef).HasMaxLength(200);

        builder.HasOne<MffLoan>().WithMany().HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
