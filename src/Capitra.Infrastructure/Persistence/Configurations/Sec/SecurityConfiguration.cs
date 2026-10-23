using Capitra.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Sec;

/// <summary>
/// Maps the ASP.NET Core Identity tables onto the SEC_ module.
/// Identity's own property names are renamed where the naming standard requires it
/// (primary keys ENTITY_ID, flags IS_, date-times _AT, no bare NAME or VALUE columns).
/// </summary>
public sealed class SecUserConfiguration : IEntityTypeConfiguration<SecUser>
{
    public void Configure(EntityTypeBuilder<SecUser> builder)
    {
        builder.ToTable("SEC_USER");
        builder.Property(x => x.Id).HasColumnName("USER_ID");
        builder.Property(x => x.PhoneNumber).HasColumnName("PHONE_NO").HasMaxLength(30);
        builder.Property(x => x.EmailConfirmed).HasColumnName("IS_EMAIL_CONFIRMED");
        builder.Property(x => x.PhoneNumberConfirmed).HasColumnName("IS_PHONE_NO_CONFIRMED");
        builder.Property(x => x.TwoFactorEnabled).HasColumnName("IS_TWO_FACTOR_ENABLED");
        builder.Property(x => x.LockoutEnabled).HasColumnName("IS_LOCKOUT_ENABLED");
        builder.Property(x => x.LockoutEnd).HasColumnName("LOCKOUT_END_AT");
        builder.Property(x => x.FullName).HasMaxLength(200);
        builder.Property(x => x.EmployeeNo).HasMaxLength(30);
        builder.Property(x => x.Department).HasMaxLength(100);
        builder.Property(x => x.JobTitle).HasMaxLength(100);
        builder.ConfigureAudit();
    }
}

public sealed class SecRoleConfiguration : IEntityTypeConfiguration<SecRole>
{
    public void Configure(EntityTypeBuilder<SecRole> builder)
    {
        builder.ToTable("SEC_ROLE");
        builder.Property(x => x.Id).HasColumnName("ROLE_ID");
        builder.Property(x => x.Name).HasColumnName("ROLE_NAME");
        builder.Property(x => x.NormalizedName).HasColumnName("NORMALIZED_ROLE_NAME");
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.ConfigureAudit();
    }
}

public sealed class SecUserClaimConfiguration : IEntityTypeConfiguration<SecUserClaim>
{
    public void Configure(EntityTypeBuilder<SecUserClaim> builder)
    {
        builder.ToTable("SEC_USER_CLAIM");
        builder.Property(x => x.Id).HasColumnName("USER_CLAIM_ID");
        builder.ConfigureAudit();
    }
}

public sealed class SecRoleClaimConfiguration : IEntityTypeConfiguration<SecRoleClaim>
{
    public void Configure(EntityTypeBuilder<SecRoleClaim> builder)
    {
        builder.ToTable("SEC_ROLE_CLAIM");
        builder.Property(x => x.Id).HasColumnName("ROLE_CLAIM_ID");
        builder.ConfigureAudit();
    }
}

public sealed class SecUserRoleConfiguration : IEntityTypeConfiguration<SecUserRole>
{
    public void Configure(EntityTypeBuilder<SecUserRole> builder)
    {
        builder.ToTable("SEC_USER_ROLE");
        builder.ConfigureAudit();
    }
}

public sealed class SecUserLoginConfiguration : IEntityTypeConfiguration<SecUserLogin>
{
    /// <summary>Same as Identity's runtime MaxLengthForKeys, set here so design-time and runtime models match.</summary>
    public const int KeyLength = 128;

    public void Configure(EntityTypeBuilder<SecUserLogin> builder)
    {
        builder.ToTable("SEC_USER_LOGIN");
        builder.Property(x => x.LoginProvider).HasMaxLength(KeyLength);
        builder.Property(x => x.ProviderKey).HasMaxLength(KeyLength);
        builder.ConfigureAudit();
    }
}

public sealed class SecUserTokenConfiguration : IEntityTypeConfiguration<SecUserToken>
{
    public void Configure(EntityTypeBuilder<SecUserToken> builder)
    {
        builder.ToTable("SEC_USER_TOKEN");
        builder.Property(x => x.LoginProvider).HasMaxLength(SecUserLoginConfiguration.KeyLength);
        builder.Property(x => x.Name).HasColumnName("TOKEN_NAME").HasMaxLength(SecUserLoginConfiguration.KeyLength);
        builder.Property(x => x.Value).HasColumnName("TOKEN_VALUE");
        builder.ConfigureAudit();
    }
}

public sealed class SecUserPasskeyConfiguration : IEntityTypeConfiguration<SecUserPasskey>
{
    public void Configure(EntityTypeBuilder<SecUserPasskey> builder)
    {
        builder.ToTable("SEC_USER_PASSKEY");
        builder.HasKey(x => x.CredentialId);
        builder.Property(x => x.CredentialId).HasMaxLength(1024);
        builder.HasOne<SecUser>().WithMany().HasForeignKey(x => x.UserId).IsRequired();
        builder.OwnsOne(x => x.Data, data => data.ToJson("PASSKEY_DATA"));
        builder.ConfigureAudit();
    }
}
