using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAccountingEventTypeConfiguration : IEntityTypeConfiguration<GlAccountingEventType>
{
    public void Configure(EntityTypeBuilder<GlAccountingEventType> builder)
    {
        builder.ToTable("GL_ACCOUNTING_EVENT_TYPES");
        builder.HasKey(x => x.EventTypeId);
        builder.HasIndex(x => new { x.SourceId, x.EventTypeCode }).IsUnique();

        builder.Property(x => x.EventTypeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.EventTypeCode).HasMaxLength(60);
        builder.Property(x => x.EventTypeNameEn).HasMaxLength(240);
        builder.Property(x => x.EventTypeNameAr).HasMaxLength(240);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<GlJournalCategory>().WithMany().HasForeignKey(x => x.DefaultCategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalSource>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
