using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffWorkAssignmentConfiguration : IEntityTypeConfiguration<MffWorkAssignment>
{
    public void Configure(EntityTypeBuilder<MffWorkAssignment> builder)
    {
        builder.ToTable("MFF_WORK_ASSIGNMENTS");
        builder.HasKey(x => x.AssignmentId);
        builder.HasIndex(x => x.ApplicationId);

        builder.Property(x => x.AssignmentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.AssignmentType).HasMaxLength(50);
        builder.Property(x => x.AssignedTo).HasMaxLength(100);
        builder.Property(x => x.AssignedBy).HasMaxLength(100);
        builder.Property(x => x.AssignmentStatus).HasMaxLength(30);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
