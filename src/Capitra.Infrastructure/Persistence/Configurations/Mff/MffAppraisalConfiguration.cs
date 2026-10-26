using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffAppraisalConfiguration : IEntityTypeConfiguration<MffAppraisal>
{
    public void Configure(EntityTypeBuilder<MffAppraisal> builder)
    {
        builder.ToTable("MFF_APPRAISALS");
        builder.HasKey(x => x.AppraisalId);
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.AppraiserOrgId);
        builder.HasIndex(x => x.PropertyId);
        builder.HasIndex(x => x.ReportDocumentId);

        builder.Property(x => x.AppraisalId).UseIdentityByDefaultColumn();
        builder.Property(x => x.AppraiserName).HasMaxLength(300);
        builder.Property(x => x.LicenseNo).HasMaxLength(100);
        builder.Property(x => x.MarketValue).HasPrecision(18, 2);
        builder.Property(x => x.ForcedSaleValue).HasPrecision(18, 2);
        builder.Property(x => x.AcceptedValue).HasPrecision(18, 2);
        builder.Property(x => x.AppraisalStatus).HasMaxLength(30);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffOrganization>().WithMany().HasForeignKey(x => x.AppraiserOrgId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffProperty>().WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffApplicationDocument>().WithMany().HasForeignKey(x => x.ReportDocumentId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
