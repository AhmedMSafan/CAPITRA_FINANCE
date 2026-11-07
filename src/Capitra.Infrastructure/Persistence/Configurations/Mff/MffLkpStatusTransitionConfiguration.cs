using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffLkpStatusTransitionConfiguration : IEntityTypeConfiguration<MffLkpStatusTransition>
{
    public void Configure(EntityTypeBuilder<MffLkpStatusTransition> builder)
    {
        builder.ToTable("MFF_LKP_STATUS_TRANSITIONS");
        builder.HasKey(x => new { x.FromStatusCode, x.ToStatusCode });
        builder.HasIndex(x => x.FromStatusCode);
        builder.HasIndex(x => x.RoleCode);
        builder.HasIndex(x => x.ToStatusCode);
        builder.HasIndex(x => new { x.FromStatusCode, x.ToStatusCode }).IsUnique();

        builder.Property(x => x.FromStatusCode).HasMaxLength(50);
        builder.Property(x => x.ToStatusCode).HasMaxLength(50);
        builder.Property(x => x.RoleCode).HasMaxLength(50);
        builder.Property(x => x.ActionCode).HasMaxLength(50);

        builder.HasOne<MffLkpUserRole>().WithMany().HasForeignKey(x => x.RoleCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpApplicationStatus>().WithMany().HasForeignKey(x => x.ToStatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpApplicationStatus>().WithMany().HasForeignKey(x => x.FromStatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
