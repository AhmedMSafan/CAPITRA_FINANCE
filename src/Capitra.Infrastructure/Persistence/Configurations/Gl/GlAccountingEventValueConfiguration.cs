using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAccountingEventValueConfiguration : IEntityTypeConfiguration<GlAccountingEventValue>
{
    public void Configure(EntityTypeBuilder<GlAccountingEventValue> builder)
    {
        builder.ToTable("GL_ACCOUNTING_EVENT_VALUES");
        builder.HasKey(x => x.EventValueId);
        builder.HasIndex(x => new { x.AccountingEventId, x.ValueCode, x.ValueSequence }).IsUnique();
        builder.HasIndex(x => new { x.AccountingEventId, x.ValueCode });
        builder.HasIndex(x => x.SourceValueId);

        builder.Property(x => x.EventValueId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ValueCode).HasMaxLength(60);
        builder.Property(x => x.ValueSequence).HasPrecision(28, 2);
        builder.Property(x => x.ValueKind).HasMaxLength(30);
        builder.Property(x => x.AmountValue).HasPrecision(22, 5);
        builder.Property(x => x.NumberValue).HasPrecision(22, 8);
        builder.Property(x => x.PercentValue).HasPrecision(12, 8);
        builder.Property(x => x.TextValue).HasMaxLength(4000);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.SourceReference).HasMaxLength(240);

        builder.HasOne<GlAccount>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.CurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlSourceValueDefinition>().WithMany().HasForeignKey(x => x.SourceValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountingEvent>().WithMany().HasForeignKey(x => x.AccountingEventId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.SegmentValueId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
