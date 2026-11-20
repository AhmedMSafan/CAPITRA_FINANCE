# Plan: replacing the copied APEX pages with native modules

## Where things stand (develop, November 2026)

CAPITRA has two kinds of module:

- **Native**: C# services with EF Core, Vue pages, unit tests. Lending, CRM / Customer 360, Treasury, Factoring, Workflow, Administration, Security.
- **APEX port**: the Oracle APEX pages copied nearly as they were. The page HTML and JavaScript live in `web/src/modules/<m>/apex/wizard/wizardPages.ts` and run through an APEX/jQuery imitation (`web/src/modules/apex/apexRuntime.ts`, which uses `new Function`). The server answers with `IApexProcess` handlers that run translated SQL (`ApexSql`, `IApexSqlReader`).

| Module | C# lines | APEX handlers | Copied page code | Vue pages | Tests |
| --- | ---: | ---: | ---: | ---: | --- |
| GL | 6,250 | 8 | 104 KB | 13 | few |
| Cheques (Chq) | 3,260 | 18 | 156 KB | 1 | life cycle, upload parsing |
| MFF | 4,532 | 25 | 352 KB | 1 | calculator |
| Mortgage / Purchased Portfolio (Mtg) | 13,071 | 79 | 932 KB | 1 | review maths, workflow |

What the APEX port costs:

1. **Security**: page scripts run through `new Function`, so the app can't have a strict Content Security Policy. Server-built HTML regions are rendered with `v-html`.
2. **Testing**: the logic is spread across page scripts, SQL strings and handlers, which makes it hard to unit test. These modules had the fewest tests.
3. **Speed and size**: about 1.5 MB of page code is in the front-end bundle. Reports now page in the database (PR "Page APEX SQL reports in PostgreSQL"), but each page still makes many small calls.
4. **Change**: each change has to be made in the copied HTML, its script and its SQL together.

## Order

Smallest risk and highest value first. Each module is a set of pull requests that can be released one at a time, while the rest of the module still runs as APEX pages.

1. **GL**: about half done already (13 Vue pages), and it is the core of every other module's posting. Finish journals entry/inquiry, periods, chart of accounts and reports as Vue pages on `GlPostingApi` / `GlJournalProcessing`. Then remove `gl/apex`.
2. **Cheques**: small (5 page groups). The rules are already in testable C# (`ChqEventPackage`, `ChqUploadPackage`). Build the cheque register, events, upload batches and wallet screens in Vue.
3. **MFF**: the calculator is already pure C# (`MffCalcPackage`) and tested. Then the application, payments, delinquency, restructuring, early settlement and legal action pages.
4. **Mortgage / Purchased Portfolio**: the largest. Split it by workflow step, since every step is already a BPMN task: case and Excel import, AML and inquiries, legal review, valuation, coverage and cheque coverage, financial review, board review, purchase agreement, disbursement and settlement, portfolio overview and reports. Do the disbursement maths (`MtgDisbursementPackage`, IRR) first, with tests, because it moves money.

## For each page

1. **Pin the behaviour first**: write tests for the handler's rules and numbers before changing anything (as done for `FinancialReviewMath`, `ChqEventPackage`).
2. **Server**: move the SQL of `ApexSqlReport` / `ApexSqlLoad` / `ApexSqlLov` into a service with EF queries and typed DTOs, and a controller with `[HasPermission]` per action. Keep the same permission codes so roles don't change.
3. **Client**: a Vue page using the shared components (tables, forms, workflow step panel), with Arabic/English labels from the dictionary.
4. **Switch the route** to the Vue page. Keep the APEX page reachable behind a feature flag for one release, for comparison.
5. **Remove** the page's entries from `wizardPages.ts`, its `IApexProcess` handlers and its `ApexSql` definitions.

## Done when

- `wizardPages.ts` and `apexRuntime.ts` are gone, along with `ApexController` / `ApexFilesController` and `Capitra.Application/Modules/Apex`.
- The web app sends a Content Security Policy without `unsafe-eval` or `unsafe-inline` scripts.
- Every module has unit tests for its calculations and rules (`STANDARDS.md`).

## Rules while the port exists

- No new features are built as APEX pages. A change to an APEX page that is bigger than a fix is the moment to rewrite that page.
- Fixes to APEX pages still come with a test for the rule they fix.
