using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlRecurringJournalTemplateConfiguration : IEntityTypeConfiguration<GlRecurringJournalTemplate>
{
    public void Configure(EntityTypeBuilder<GlRecurringJournalTemplate> builder)
    {
        builder.ToTable("GL_RECURRING_JOURNAL_TEMPLATES");
        builder.HasKey(x => x.RecurringTemplateId);
        builder.HasIndex(x => new { x.LedgerId, x.TemplateCode }).IsUnique();

        builder.Property(x => x.RecurringTemplateId).UseIdentityByDefaultColumn();
        builder.Property(x => x.TemplateCode).HasMaxLength(60);
        builder.Property(x => x.TemplateName).HasMaxLength(240);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.RecurrenceFrequency).HasMaxLength(30);

        builder.HasOne<GlJournalCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalSource>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
