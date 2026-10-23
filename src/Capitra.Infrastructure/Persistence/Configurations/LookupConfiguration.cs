using Capitra.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations;

/// <summary>Shape of every lookup table (naming standard, section 5).</summary>
public abstract class LookupConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : LookupEntity
{
    protected abstract string TableName { get; }

    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => x.Code);

        builder.Property(x => x.Code).HasMaxLength(30);
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.ConfigureAudit();
    }
}
