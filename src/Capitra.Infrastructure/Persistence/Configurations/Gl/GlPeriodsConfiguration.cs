using Capitra.Domain.Common;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

/// <summary>Calendars, fiscal years, accounting periods, ledgers and ledger periods. Lengths follow the APEX App 120 page 30 forms.</summary>
public sealed class GlCalendarConfiguration : IEntityTypeConfiguration<GlCalendar>
{
    public void Configure(EntityTypeBuilder<GlCalendar> builder)
    {
        builder.ToTable("GL_CALENDARS");
        builder.HasKey(x => x.CalendarId);
        builder.HasIndex(x => x.CalendarCode).IsUnique();

        builder.Property(x => x.CalendarCode).HasMaxLength(30);
        builder.Property(x => x.CalendarNameEn).HasMaxLength(150);
        builder.Property(x => x.CalendarNameAr).HasMaxLength(150);
        builder.Property(x => x.CalendarType).HasMaxLength(30);
        builder.Property(x => x.PeriodFrequency).HasMaxLength(30);

        builder.ConfigureAudit();
    }
}

public sealed class GlFiscalYearConfiguration : IEntityTypeConfiguration<GlFiscalYear>
{
    public void Configure(EntityTypeBuilder<GlFiscalYear> builder)
    {
        builder.ToTable("GL_FISCAL_YEARS");
        builder.HasKey(x => x.FiscalYearId);
        builder.HasIndex(x => new { x.CalendarId, x.FiscalYear }).IsUnique();

        builder.Property(x => x.YearName).HasMaxLength(50);
        builder.Property(x => x.YearStatus).HasMaxLength(30);

        builder.HasOne<GlCalendar>().WithMany().HasForeignKey(x => x.CalendarId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class GlAccountingPeriodConfiguration : IEntityTypeConfiguration<GlAccountingPeriod>
{
    public void Configure(EntityTypeBuilder<GlAccountingPeriod> builder)
    {
        builder.ToTable("GL_ACCOUNTING_PERIODS");
        builder.HasKey(x => x.PeriodId);
        builder.HasIndex(x => new { x.CalendarId, x.PeriodCode }).IsUnique();
        builder.HasIndex(x => new { x.CalendarId, x.StartDate });

        builder.Property(x => x.PeriodCode).HasMaxLength(30);
        builder.Property(x => x.PeriodNameEn).HasMaxLength(80);
        builder.Property(x => x.PeriodNameAr).HasMaxLength(80);
        builder.Property(x => x.DefaultPeriodStatus).HasMaxLength(30);

        builder.HasOne<GlFiscalYear>().WithMany().HasForeignKey(x => x.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCalendar>().WithMany().HasForeignKey(x => x.CalendarId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class GlLedgerConfiguration : IEntityTypeConfiguration<GlLedger>
{
    public const int AccountCodeMaxLength = 100;

    public void Configure(EntityTypeBuilder<GlLedger> builder)
    {
        builder.ToTable("GL_LEDGERS");
        builder.HasKey(x => x.LedgerId);
        builder.HasIndex(x => x.LedgerCode).IsUnique();

        builder.Property(x => x.LegalEntityCode).HasMaxLength(LookupEntity.CodeMaxLength);
        builder.Property(x => x.LedgerCode).HasMaxLength(30);
        builder.Property(x => x.LedgerNameEn).HasMaxLength(150);
        builder.Property(x => x.LedgerNameAr).HasMaxLength(150);
        builder.Property(x => x.LedgerType).HasMaxLength(30);
        builder.Property(x => x.LedgerCurrencyCode).HasMaxLength(LookupEntity.CodeMaxLength);
        builder.Property(x => x.LedgerStatus).HasMaxLength(30);
        builder.Property(x => x.RetainedEarningsAcctCode).HasMaxLength(AccountCodeMaxLength);
        builder.Property(x => x.SuspenseAcctCode).HasMaxLength(AccountCodeMaxLength);
        builder.Property(x => x.RoundingDiffAcctCode).HasMaxLength(AccountCodeMaxLength);
        builder.Property(x => x.FxGainAcctCode).HasMaxLength(AccountCodeMaxLength);
        builder.Property(x => x.FxLossAcctCode).HasMaxLength(AccountCodeMaxLength);
        builder.Property(x => x.OpeningBalanceAcctCode).HasMaxLength(AccountCodeMaxLength);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<AdmLkpLegalEntity>().WithMany().HasForeignKey(x => x.LegalEntityCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpCurrency>().WithMany().HasForeignKey(x => x.LedgerCurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCalendar>().WithMany().HasForeignKey(x => x.CalendarId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaStructure>().WithMany().HasForeignKey(x => x.CoaId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class GlLedgerPeriodConfiguration : IEntityTypeConfiguration<GlLedgerPeriod>
{
    public void Configure(EntityTypeBuilder<GlLedgerPeriod> builder)
    {
        builder.ToTable("GL_LEDGER_PERIODS");
        builder.HasKey(x => x.LedgerPeriodId);
        builder.HasIndex(x => new { x.LedgerId, x.PeriodId }).IsUnique();

        builder.Property(x => x.PeriodStatus).HasMaxLength(30);
        builder.Property(x => x.PostingMode).HasMaxLength(30);
        builder.Property(x => x.OpenedBy).HasMaxLength(AuditableConfiguration.UserNameMaxLength);
        builder.Property(x => x.ClosedBy).HasMaxLength(AuditableConfiguration.UserNameMaxLength);
        builder.Property(x => x.PermanentlyClosedBy).HasMaxLength(AuditableConfiguration.UserNameMaxLength);
        builder.Property(x => x.CloseNotes).HasMaxLength(1000);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountingPeriod>().WithMany().HasForeignKey(x => x.PeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class GlLedgerPeriodHistoryConfiguration : IEntityTypeConfiguration<GlLedgerPeriodHistory>
{
    public void Configure(EntityTypeBuilder<GlLedgerPeriodHistory> builder)
    {
        builder.ToTable("GL_LEDGER_PERIODS_HIST");
        builder.HasKey(x => x.LedgerPeriodHistoryId);
        builder.HasIndex(x => x.LedgerPeriodId);

        builder.Property(x => x.OldPeriodStatus).HasMaxLength(30);
        builder.Property(x => x.NewPeriodStatus).HasMaxLength(30);
        builder.Property(x => x.ActionCode).HasMaxLength(30);
        builder.Property(x => x.ActionReason).HasMaxLength(1000);

        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
