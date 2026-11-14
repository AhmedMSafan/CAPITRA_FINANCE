using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlTranSetSequenceConfiguration : IEntityTypeConfiguration<GlTranSetSequence>
{
    public void Configure(EntityTypeBuilder<GlTranSetSequence> builder)
    {
        builder.ToTable("GL_TRAN_SET_SEQUENCES");
        builder.HasKey(x => new { x.ModuleCode, x.YearNo });

        builder.Property(x => x.ModuleCode).HasMaxLength(3);
        builder.Property(x => x.AppCode).HasMaxLength(10);
        builder.Property(x => x.ModuleNameEn).HasMaxLength(200);
        builder.Property(x => x.ModuleNameAr).HasMaxLength(200);

        builder.ConfigureAudit();
    }
}
