using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlJournalCategoryConfiguration : IEntityTypeConfiguration<GlJournalCategory>
{
    public void Configure(EntityTypeBuilder<GlJournalCategory> builder)
    {
        builder.ToTable("GL_JOURNAL_CATEGORIES");
        builder.HasKey(x => x.CategoryId);
        builder.HasIndex(x => x.CategoryCode).IsUnique();

        builder.Property(x => x.CategoryId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CategoryCode).HasMaxLength(30);
        builder.Property(x => x.CategoryNameEn).HasMaxLength(200);
        builder.Property(x => x.CategoryNameAr).HasMaxLength(200);
        builder.Property(x => x.CategoryType).HasMaxLength(30);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<GlJournalSource>().WithMany().HasForeignKey(x => x.DefaultSourceId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
