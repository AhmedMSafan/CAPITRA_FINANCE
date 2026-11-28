using Capitra.Domain.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations;

internal static class AuditableConfiguration
{
    public const int UserNameMaxLength = 100;

    /// <summary>Standard audit columns (naming standard, section 4).</summary>
    public static void ConfigureAudit<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IAuditable
    {
        builder.Property(x => x.CreatedBy).HasMaxLength(UserNameMaxLength).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(UserNameMaxLength);
        builder.Property(x => x.RowVersion).IsRequired();
    }
}
