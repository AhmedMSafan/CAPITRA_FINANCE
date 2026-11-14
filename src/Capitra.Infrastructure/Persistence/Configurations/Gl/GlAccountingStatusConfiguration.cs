using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAccountingStatusConfiguration : IEntityTypeConfiguration<GlAccountingStatus>
{
    public void Configure(EntityTypeBuilder<GlAccountingStatus> builder)
    {
        builder.ToTable("GL_ACCOUNTING_STATUSES");
        builder.HasKey(x => x.StatusCode);

        builder.Property(x => x.StatusCode).HasMaxLength(30);
        builder.Property(x => x.StatusNameEn).HasMaxLength(100);
        builder.Property(x => x.StatusNameAr).HasMaxLength(100);

        builder.ConfigureAudit();
    }
}
