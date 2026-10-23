using Capitra.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations;

/// <summary>Shape of every lookup table (naming standard, section 5).</summary>
public abstract class LookupConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : LookupEntity
{
    /// <summary>Length of CODE and of every column that references a lookup by code.</summary>
    protected const int LookupCodeLength = LookupEntity.CodeMaxLength;

    protected abstract string TableName { get; }

    /// <summary>NAME_AR / NAME_EN length, taken from the APEX form item.</summary>
    protected virtual int NameMaxLength => 200;

    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => x.Code);

        builder.Property(x => x.Code).HasMaxLength(LookupCodeLength);
        builder.Property(x => x.NameAr).HasMaxLength(NameMaxLength).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(NameMaxLength).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.ConfigureAudit();
    }
}
