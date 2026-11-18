# CAPITRA engineering standards

The rules every change follows. Most of them are checked by the build or by a test, so a pull request that breaks one fails CI.

## Branches and pull requests

| Branch | Purpose |
| --- | --- |
| `main` | What is released. Updated only by merging `develop` at a release. |
| `develop` | The integration branch. Every pull request targets it. |
| `feature/<name>`, `fix/<name>`, `chore/<name>`, `docs/<name>`, `test/<name>` | One change each, branched from `develop`. |

- Every change goes through a pull request into `develop`, and CI must pass first (API build and tests, web lint and build).
- Pull requests are squash-merged and the branch is deleted afterwards. In GitHub, turn on **Settings → General → Automatically delete head branches**, so merged branches don't pile up again.
- `main` is updated by a `develop → main` pull request when a version is released, then tagged (`v1.0.0`, ...).

## Money and numbers

- Amounts, rates and percentages are `decimal`, never `double`. The only exception is solving an IRR or a power, and the result is turned back into `decimal` straight away.
- Rounding is **half away from zero** (0.125 → 0.13), as Oracle `ROUND` did: use `Money.Round(value, decimals)` (`Capitra.Application.Common`).
  `Math.Round(decimal)` and `Math.Round(decimal, int)` round half to even and are banned: `BannedSymbols.txt` makes them a build error.
- Decimals: 2 for amounts unless the module's rule says otherwise (GL accounted amounts 5, Treasury sublimit shares 3). The last line of a split takes the rounding, so the parts always add up to the whole.
- Day counts and date rules come from the product or the calculator, never hard-coded. Monthly dates count from the first date (`first + k months`), not from the previous one.

## Tests

- Every calculation, rule or state change gets a unit test in `tests/Capitra.Tests/<Module>` with numbers worked out by hand in a comment. The lending tests are the model to follow.
- A bug fix comes with the test that would have caught it.
- Logic that needs no database is written as static methods or plain classes, so it can be tested without one.
- Internal helpers can be tested directly (`InternalsVisibleTo("Capitra.Tests")`). Don't make things public just for a test.

## Database and migrations

- The model is the source of truth: `dotnet ef migrations add <Name> -p src/Capitra.Infrastructure -s src/Capitra.Infrastructure -o Persistence/Migrations`.
- One migration per pull request, named after the feature. `MigrationTests` fails if the model changed without one.
- Seed or reference data goes in the migration (`migrationBuilder.Sql` or `InsertData`), never in a manual script someone has to remember to run.
- A migration already merged into `develop` is never edited. Write a new one.
- The naming standard (UPPERCASE, module prefix, audit columns, suffix types) is enforced by `NamingStandardTests`. Every new table goes into `database/scripts/reset-all-portfolios.sql` (`ResetScriptTests`).

## Security

- Every endpoint needs a signed-in user. `[AllowAnonymous]` is allowed only on login (`AuthorizationTests`).
- Every action has `[HasPermission(...)]`, on the action or its controller. Generic routes (`ApexController`) check the handler's own permission.
- The user always comes from the token (`ICurrentUser`), never from the request body.
- Each request is checked against the user's current access (active, security stamp, roles, permissions; kept 30 seconds). User and role administration clears it immediately.
- SQL is always parameterised. A column name from a request only reaches SQL after checking it against the query's own columns (`ApexSqlPaging`).
- Segregation of duties is checked on the **person** (employee number), not just on the account, wherever a maker cannot be the checker.

## Front end

- New screens are Vue components in `web/src/modules/<module>`. Don't add new copied APEX page HTML or scripts (`wizardPages.ts`, `runPageScript`).
- No `v-html` with data from the API. Text goes through Vue's interpolation, which escapes it. Icons from code are fine.
- The goal is a strict Content Security Policy (no `unsafe-eval`). It becomes possible once the last APEX page script is gone (see `APEX-REWRITE-PLAN.md`).

## Commits

- Commit messages explain why, not just what: the first line is a short summary, then a paragraph on the problem and the fix.
