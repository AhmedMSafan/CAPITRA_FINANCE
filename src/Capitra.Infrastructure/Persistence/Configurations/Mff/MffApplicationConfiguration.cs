using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffApplicationConfiguration : IEntityTypeConfiguration<MffApplication>
{
    public void Configure(EntityTypeBuilder<MffApplication> builder)
    {
        builder.ToTable("MFF_APPLICATIONS");
        builder.HasKey(x => x.ApplicationId);
        builder.HasIndex(x => x.ApplicationNo).IsUnique();
        builder.HasIndex(x => x.FinancierId);
        builder.HasIndex(x => x.FundOrgId);
        builder.HasIndex(x => x.ImportStagingId);
        builder.HasIndex(x => x.PrimaryPersonId);
        builder.HasIndex(x => x.ProgramVersionId);
        builder.HasIndex(x => x.SourceChannelCode);
        builder.HasIndex(x => x.StatusCode);

        builder.Property(x => x.ApplicationId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ApplicationNo).HasMaxLength(50);
        builder.Property(x => x.SourceChannelCode).HasMaxLength(50);
        builder.Property(x => x.StatusCode).HasMaxLength(50);
        builder.Property(x => x.FundFileNo).HasMaxLength(100);
        builder.Property(x => x.EligibilityCertNo).HasMaxLength(100);
        builder.Property(x => x.RequestedFinanceAmount).HasPrecision(18, 2);
        builder.Property(x => x.ApprovedFinanceAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalSubsidyAmount).HasPrecision(18, 2);
        builder.Property(x => x.PropertyPrice).HasPrecision(18, 2);
        builder.Property(x => x.DownpaymentAmount).HasPrecision(18, 2);
        builder.Property(x => x.MonthlyIncomeTotal).HasPrecision(18, 2);
        builder.Property(x => x.RejectionReason).HasMaxLength(2000);
        builder.Property(x => x.CurrentOwnerUser).HasMaxLength(100);
        builder.Property(x => x.BranchCode).HasMaxLength(50);

        builder.HasOne<MffOrganization>().WithMany().HasForeignKey(x => x.FinancierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffOrganization>().WithMany().HasForeignKey(x => x.FundOrgId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffImportStaging>().WithMany().HasForeignKey(x => x.ImportStagingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffPerson>().WithMany().HasForeignKey(x => x.PrimaryPersonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffProgramVersion>().WithMany().HasForeignKey(x => x.ProgramVersionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpSourceChannel>().WithMany().HasForeignKey(x => x.SourceChannelCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpApplicationStatus>().WithMany().HasForeignKey(x => x.StatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
