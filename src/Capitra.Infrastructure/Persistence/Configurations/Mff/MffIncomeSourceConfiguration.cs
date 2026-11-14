using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffIncomeSourceConfiguration : IEntityTypeConfiguration<MffIncomeSource>
{
    public void Configure(EntityTypeBuilder<MffIncomeSource> builder)
    {
        builder.ToTable("MFF_INCOME_SOURCES");
        builder.HasKey(x => x.IncomeId);
        builder.HasIndex(x => x.ApplicationPartyId);
        builder.HasIndex(x => x.IncomeTypeCode);

        builder.Property(x => x.IncomeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.IncomeTypeCode).HasMaxLength(50);
        builder.Property(x => x.EmployerName).HasMaxLength(300);
        builder.Property(x => x.MonthlyNetIncome).HasPrecision(18, 2);
        builder.Property(x => x.AnnualNetIncome).HasPrecision(18, 2);
        builder.Property(x => x.SourceReference).HasMaxLength(500);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<MffApplicationParty>().WithMany().HasForeignKey(x => x.ApplicationPartyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpIncomeType>().WithMany().HasForeignKey(x => x.IncomeTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
