using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlSourceValueDefinitionConfiguration : IEntityTypeConfiguration<GlSourceValueDefinition>
{
    public void Configure(EntityTypeBuilder<GlSourceValueDefinition> builder)
    {
        builder.ToTable("GL_SOURCE_VALUE_DEFINITIONS");
        builder.HasKey(x => x.SourceValueId);
        builder.HasIndex(x => new { x.SourceId, x.EventTypeId, x.ValueCode }).IsUnique();
        builder.HasIndex(x => new { x.SourceModule, x.ValueCode });
        builder.HasIndex(x => new { x.EventTypeId, x.ValueKind, x.IsActive });

        builder.Property(x => x.SourceValueId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SourceModule).HasMaxLength(30);
        builder.Property(x => x.ValueCode).HasMaxLength(60);
        builder.Property(x => x.ValueNameEn).HasMaxLength(240);
        builder.Property(x => x.ValueNameAr).HasMaxLength(240);
        builder.Property(x => x.ValueKind).HasMaxLength(30);
        builder.Property(x => x.ValueRole).HasMaxLength(60);
        builder.Property(x => x.DefaultCurrencyCode).HasMaxLength(3);
        builder.Property(x => x.SourceTableName).HasMaxLength(128);
        builder.Property(x => x.SourceColumnName).HasMaxLength(128);
        builder.Property(x => x.SourcePath).HasMaxLength(500);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.DefaultCurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountingEventType>().WithMany().HasForeignKey(x => x.EventTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalSource>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
