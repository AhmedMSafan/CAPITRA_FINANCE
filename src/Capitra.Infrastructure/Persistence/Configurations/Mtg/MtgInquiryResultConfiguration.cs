using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgInquiryResultConfiguration : IEntityTypeConfiguration<MtgInquiryResult>
{
    public void Configure(EntityTypeBuilder<MtgInquiryResult> builder)
    {
        builder.ToTable("MTG_INQUIRY_RESULTS");
        builder.HasKey(x => x.InquiryId);
        builder.HasIndex(x => x.PortfolioId);

        builder.Property(x => x.InquiryId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SubjectType).HasMaxLength(30);
        builder.Property(x => x.InquiryTypeCode).HasMaxLength(50);
        builder.Property(x => x.ReferenceNo).HasMaxLength(100);
        builder.Property(x => x.ResultCode).HasMaxLength(30);
        builder.Property(x => x.RiskLevelCode).HasMaxLength(20);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpRiskLevel>().WithMany().HasForeignKey(x => x.RiskLevelCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgLkpInquiryType>().WithMany().HasForeignKey(x => x.InquiryTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
