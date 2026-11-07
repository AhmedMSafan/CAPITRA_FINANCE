using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffApplicationStatusHistConfiguration : IEntityTypeConfiguration<MffApplicationStatusHist>
{
    public void Configure(EntityTypeBuilder<MffApplicationStatusHist> builder)
    {
        builder.ToTable("MFF_APPLICATION_STATUS_HIST");
        builder.HasKey(x => x.StatusHistId);
        builder.HasIndex(x => x.RoleCode);
        builder.HasIndex(x => x.OldStatusCode);
        builder.HasIndex(x => x.NewStatusCode);
        builder.HasIndex(x => x.ApplicationId);

        builder.Property(x => x.StatusHistId).UseIdentityByDefaultColumn();
        builder.Property(x => x.OldStatusCode).HasMaxLength(30);
        builder.Property(x => x.NewStatusCode).HasMaxLength(30);
        builder.Property(x => x.ChangedBy).HasMaxLength(100);
        builder.Property(x => x.RoleCode).HasMaxLength(30);
        builder.Property(x => x.RemarksAr).HasMaxLength(2000);

        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpApplicationStatus>().WithMany().HasForeignKey(x => x.NewStatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpUserRole>().WithMany().HasForeignKey(x => x.RoleCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpApplicationStatus>().WithMany().HasForeignKey(x => x.OldStatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
