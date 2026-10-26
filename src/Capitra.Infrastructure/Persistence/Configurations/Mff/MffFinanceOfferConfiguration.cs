using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffFinanceOfferConfiguration : IEntityTypeConfiguration<MffFinanceOffer>
{
    public void Configure(EntityTypeBuilder<MffFinanceOffer> builder)
    {
        builder.ToTable("MFF_FINANCE_OFFERS");
        builder.HasKey(x => x.OfferId);
        builder.HasIndex(x => x.OfferNo).IsUnique();
        builder.HasIndex(x => x.ApplicationId);

        builder.Property(x => x.OfferId).UseIdentityByDefaultColumn();
        builder.Property(x => x.OfferNo).HasMaxLength(100);
        builder.Property(x => x.FinanceAmount).HasPrecision(18, 2);
        builder.Property(x => x.AnnualRatePct).HasPrecision(9, 6);
        builder.Property(x => x.InitialInstallment).HasPrecision(18, 2);
        builder.Property(x => x.DownpaymentAmount).HasPrecision(18, 2);
        builder.Property(x => x.SubsidyAmount).HasPrecision(18, 2);
        builder.Property(x => x.LtvPct).HasPrecision(9, 6);
        builder.Property(x => x.OfferStatus).HasMaxLength(30);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
