using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAutoAccountingConditionConfiguration : IEntityTypeConfiguration<GlAutoAccountingCondition>
{
    public void Configure(EntityTypeBuilder<GlAutoAccountingCondition> builder)
    {
        builder.ToTable("GL_AUTO_ACCOUNTING_CONDITIONS");
        builder.HasKey(x => x.ConditionId);
        builder.HasIndex(x => new { x.OwnerType, x.OwnerId, x.ConditionGroupNo, x.SequenceNo }).IsUnique();
        builder.HasIndex(x => new { x.OwnerType, x.OwnerId, x.IsActive });

        builder.Property(x => x.ConditionId).UseIdentityByDefaultColumn();
        builder.Property(x => x.OwnerType).HasMaxLength(30);
        builder.Property(x => x.LeftValueCode).HasMaxLength(60);
        builder.Property(x => x.OperatorCode).HasMaxLength(30);
        builder.Property(x => x.CompareType).HasMaxLength(30);
        builder.Property(x => x.CompareValueCode).HasMaxLength(60);
        builder.Property(x => x.CompareTextValue).HasMaxLength(4000);
        builder.Property(x => x.CompareNumberValue).HasPrecision(22, 8);
        builder.Property(x => x.CompareFromNumber).HasPrecision(22, 8);
        builder.Property(x => x.CompareToNumber).HasPrecision(22, 8);
        builder.Property(x => x.JoinOperator).HasMaxLength(10);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<GlSourceValueDefinition>().WithMany().HasForeignKey(x => x.CompareSourceValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlSourceValueDefinition>().WithMany().HasForeignKey(x => x.LeftSourceValueId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
