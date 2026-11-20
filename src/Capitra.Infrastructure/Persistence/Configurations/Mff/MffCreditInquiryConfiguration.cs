using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffCreditInquiryConfiguration : IEntityTypeConfiguration<MffCreditInquiry>
{
    public void Configure(EntityTypeBuilder<MffCreditInquiry> builder)
    {
        builder.ToTable("MFF_CREDIT_INQUIRIES");
        builder.HasKey(x => x.InquiryId);
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.BureauOrgId);
        builder.HasIndex(x => x.ReportDocumentId);

        builder.Property(x => x.InquiryId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CreditScore).HasPrecision(10, 2);
        builder.Property(x => x.ResultCode).HasMaxLength(50);
        builder.Property(x => x.ReferenceNo).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<MffOrganization>().WithMany().HasForeignKey(x => x.BureauOrgId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffApplicationDocument>().WithMany().HasForeignKey(x => x.ReportDocumentId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
