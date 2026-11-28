using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlJournalValidationRuleConfiguration : IEntityTypeConfiguration<GlJournalValidationRule>
{
    public void Configure(EntityTypeBuilder<GlJournalValidationRule> builder)
    {
        builder.ToTable("GL_JOURNAL_VALIDATION_RULES");
        builder.HasKey(x => x.ValidationRuleId);
        builder.HasIndex(x => x.RuleCode).IsUnique();

        builder.Property(x => x.ValidationRuleId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RuleCode).HasMaxLength(60);
        builder.Property(x => x.RuleNameEn).HasMaxLength(240);
        builder.Property(x => x.RuleNameAr).HasMaxLength(240);
        builder.Property(x => x.Severity).HasMaxLength(20);
        builder.Property(x => x.ValidationMessage).HasMaxLength(1000);

        builder.HasOne<GlJournalCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalSource>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
