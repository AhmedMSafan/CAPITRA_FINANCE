using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgLkpInquiryTypeConfiguration : IEntityTypeConfiguration<MtgLkpInquiryType>
{
    public void Configure(EntityTypeBuilder<MtgLkpInquiryType> builder)
    {
        builder.ToTable("MTG_LKP_INQUIRY_TYPES");
        builder.HasKey(x => x.InquiryTypeCode);

        builder.Property(x => x.InquiryTypeCode).HasMaxLength(50);
        builder.Property(x => x.InquiryNameAr).HasMaxLength(200);
        builder.Property(x => x.InquiryNameEn).HasMaxLength(200);

        builder.ConfigureAudit();
    }
}
