using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffSubsidyApprovalConfiguration : IEntityTypeConfiguration<MffSubsidyApproval>
{
    public void Configure(EntityTypeBuilder<MffSubsidyApproval> builder)
    {
        builder.ToTable("MFF_SUBSIDY_APPROVALS");
        builder.HasKey(x => x.SubsidyApprovalId);
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.SupportTypeCode);

        builder.Property(x => x.SubsidyApprovalId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SupportTypeCode).HasMaxLength(50);
        builder.Property(x => x.ApprovalNo).HasMaxLength(100);
        builder.Property(x => x.InstallmentSupportNpv).HasPrecision(18, 2);
        builder.Property(x => x.DownpaymentSupport).HasPrecision(18, 2);
        builder.Property(x => x.TotalSupport).HasPrecision(18, 2);
        builder.Property(x => x.ApprovalStatus).HasMaxLength(30);
        builder.Property(x => x.FundNotes).HasMaxLength(2000);

        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpSupportType>().WithMany().HasForeignKey(x => x.SupportTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
