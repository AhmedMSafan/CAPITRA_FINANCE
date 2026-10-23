# Capitra Finance

Financing platform rebuilt from the Oracle APEX application (App 100 and its sub-apps) as a three-tier system:
ASP.NET Core API, PostgreSQL database `CAPITRA_DB`, and a Vue 3 + PrimeVue front end.
All business logic lives in the C# services; the database holds data only.

| Folder | What it is |
| --- | --- |
| `src/Capitra.Domain` | Entities (no framework dependencies) |
| `src/Capitra.Application` | Services, DTOs, permissions, business rules |
| `src/Capitra.Infrastructure` | EF Core + PostgreSQL, Identity, migrations, naming conventions |
| `src/Capitra.Api` | Controllers, JWT security, startup |
| `tests/Capitra.Tests` | Naming-standard, migration, security and audit tests |
| `web` | Vue 3 front end |

The naming standard (v1.0) is enforced by `tests/Capitra.Tests/Persistence/NamingStandardTests.cs`:
UPPERCASE table and column names with a module prefix (`ADM_`, `MTG_`, `WFE_`, `GL_`, `SEC_`...), audit columns on every table,
and type rules by suffix (`IS_`, `_AT`, `_DATE`, `_AMOUNT`, `_PCT`). A table that breaks the standard fails the build.

## Requirements

- .NET SDK 10
- PostgreSQL 16 or later
- Node.js 22 or later

## Run the API

Secrets are never committed. Set them once with user-secrets:

```bash
cd src/Capitra.Api
dotnet user-secrets set "ConnectionStrings:CapitraDb" "Host=localhost;Database=CAPITRA_DB;Username=capitra;Password=<password>"
dotnet user-secrets set "Jwt:Key" "<random string, at least 32 characters>"

# First run only: creates the ADMIN role (all permissions) and this user. Remove afterwards.
dotnet user-secrets set "Seed:AdminUserName" "admin"
dotnet user-secrets set "Seed:AdminPassword" "<strong password>"

dotnet run
```

In Development the API applies pending migrations on startup and listens on `https://localhost:7159`.
In other environments, set the same keys as environment variables (`ConnectionStrings__CapitraDb`, `Jwt__Key`)
and apply migrations explicitly:

```bash
dotnet tool install -g dotnet-ef
CAPITRA_DB_CONNECTION="Host=...;Database=CAPITRA_DB;..." \
  dotnet ef database update -p src/Capitra.Infrastructure -s src/Capitra.Infrastructure
```

## Run the web app

```bash
cd web
npm ci
cp .env.example .env.local   # VITE_API_BASE_URL, default https://localhost:7159
npm run dev                  # http://localhost:5173
```

## Security model

- ASP.NET Core Identity stores users and roles (`SEC_USER`, `SEC_ROLE`...). Users and roles are created by an administrator.
- `POST api/sec/auth/login` is the only anonymous endpoint. Every other endpoint requires a valid JWT.
- Permissions are codes such as `ADM.LOOKUP.EDIT`, granted to roles as role claims and checked with `[HasPermission]`.
- The signed-in user always comes from the token, never from a request body. Audit columns are filled by the server.

## Adding a migration

```bash
dotnet ef migrations add <Name> -p src/Capitra.Infrastructure -s src/Capitra.Infrastructure -o Persistence/Migrations
```

`MigrationTests` fails if the model changed without a migration.
