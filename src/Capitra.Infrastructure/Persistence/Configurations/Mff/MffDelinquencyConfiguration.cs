using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffDelinquencyConfiguration : IEntityTypeConfiguration<MffDelinquency>
{
    public void Configure(EntityTypeBuilder<MffDelinquency> builder)
    {
        builder.ToTable("MFF_DELINQUENCIES");
        builder.HasKey(x => x.DelinquencyId);
        builder.HasIndex(x => x.LoanId);
        builder.HasIndex(x => x.StageCode);

        builder.Property(x => x.DelinquencyId).UseIdentityByDefaultColumn();
        builder.Property(x => x.StageCode).HasMaxLength(30);
        builder.Property(x => x.OverdueAmount).HasPrecision(18, 2);
        builder.Property(x => x.DelinquencyStatus).HasMaxLength(30);
        builder.Property(x => x.CollectionActionAr).HasMaxLength(1000);

        builder.HasOne<MffLoan>().WithMany().HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpDelinquencyStage>().WithMany().HasForeignKey(x => x.StageCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
