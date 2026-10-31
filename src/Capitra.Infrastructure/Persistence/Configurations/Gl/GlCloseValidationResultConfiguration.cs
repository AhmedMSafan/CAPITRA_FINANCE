using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseValidationResultConfiguration : IEntityTypeConfiguration<GlCloseValidationResult>
{
    public void Configure(EntityTypeBuilder<GlCloseValidationResult> builder)
    {
        builder.ToTable("GL_CLOSE_VALIDATION_RESULTS");
        builder.HasKey(x => x.ValidationResultId);
        builder.HasIndex(x => new { x.CloseRunId, x.ResultStatus, x.Severity });

        builder.Property(x => x.ValidationResultId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RuleCode).HasMaxLength(50);
        builder.Property(x => x.SourceModule).HasMaxLength(50);
        builder.Property(x => x.ResultStatus).HasMaxLength(30);
        builder.Property(x => x.Severity).HasMaxLength(20);
        builder.Property(x => x.ResultAmount).HasPrecision(18, 2);
        builder.Property(x => x.ThresholdAmount).HasPrecision(18, 2);
        builder.Property(x => x.VarianceAmount).HasPrecision(18, 2);
        builder.Property(x => x.ResultMessage).HasMaxLength(2000);
        builder.Property(x => x.DetailReferenceType).HasMaxLength(50);
        builder.Property(x => x.DetailReferenceNo).HasMaxLength(100);
        builder.Property(x => x.ExecutedBy).HasMaxLength(100);
        builder.Property(x => x.ResolvedBy).HasMaxLength(100);
        builder.Property(x => x.ResolutionNotes).HasMaxLength(1000);

        builder.HasOne<GlCloseRun>().WithMany().HasForeignKey(x => x.CloseRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCloseRunTask>().WithMany().HasForeignKey(x => x.CloseRunTaskId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCloseValidationRule>().WithMany().HasForeignKey(x => x.ValidationRuleId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
