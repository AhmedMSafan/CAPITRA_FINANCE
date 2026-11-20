using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAccountingAccountLookupConfiguration : IEntityTypeConfiguration<GlAccountingAccountLookup>
{
    public void Configure(EntityTypeBuilder<GlAccountingAccountLookup> builder)
    {
        builder.ToTable("GL_ACCOUNTING_ACCOUNT_LOOKUPS");
        builder.HasKey(x => x.AccountLookupId);
        builder.HasIndex(x => x.LookupCode).IsUnique();

        builder.Property(x => x.AccountLookupId).UseIdentityByDefaultColumn();
        builder.Property(x => x.LookupCode).HasMaxLength(80);
        builder.Property(x => x.LookupNameEn).HasMaxLength(240);
        builder.Property(x => x.LookupNameAr).HasMaxLength(240);
        builder.Property(x => x.LookupResultType).HasMaxLength(30);
        builder.Property(x => x.NoMatchBehavior).HasMaxLength(30);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<GlAccount>().WithMany().HasForeignKey(x => x.DefaultAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.DefaultAccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountingEventType>().WithMany().HasForeignKey(x => x.EventTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalSource>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
