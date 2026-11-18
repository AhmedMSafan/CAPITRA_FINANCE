using System.Reflection;

namespace Capitra.Infrastructure.Persistence.Migrations;

/// <summary>
/// The reference data of the Baseline migration: what the 74 migrations it replaced inserted (workflow definitions and their
/// versions, document lookups, business lines, goods categories, GL transaction set sequences), dumped by pg_dump from a database
/// those migrations built, with the identity sequences set after it. Embedded from Baseline.Seed.sql.
/// </summary>
public static class BaselineSeed
{
    public const string ResourceName = "Capitra.Infrastructure.Baseline.Seed.sql";

    public static string Sql
    {
        get
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
                ?? throw new InvalidOperationException($"Embedded resource {ResourceName} is missing.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
