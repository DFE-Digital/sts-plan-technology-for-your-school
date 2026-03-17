# Tests

All automated test projects for the Plan Technology for Your School service.

## Projects

| Project | Type | What it tests |
|---|---|---|
| `Dfe.PlanTech.Core.UnitTests` | xUnit unit tests | Core models, helpers, and extensions |
| `Dfe.PlanTech.Application.UnitTests` | xUnit unit tests | Application services, workflows, and renderers |
| `Dfe.PlanTech.Data.Contentful.UnitTests` | xUnit unit tests | Contentful repository, caching, and entry resolution |
| `Dfe.PlanTech.Data.Sql.UnitTests` | xUnit unit tests | SQL repositories, entity configuration |
| `Dfe.PlanTech.Data.Sql.IntegrationTests` | xUnit integration tests | EF Core database operations (requires SQL Server) |
| `Dfe.PlanTech.DatabaseUpgrader.UnitTests` | xUnit unit tests | DbUp migration executor and argument parsing |
| `Dfe.PlanTech.Infrastructure.Redis.UnitTests` | xUnit unit tests | Redis cache, dependency manager, lock provider |
| `Dfe.PlanTech.Infrastructure.ServiceBus.UnitTests` | xUnit unit tests | Service Bus message processor and retry logic |
| `Dfe.PlanTech.Infrastructure.SignIn.UnitTests` | xUnit unit tests | DfE Sign-in OIDC events and claims parsing |
| `Dfe.PlanTech.Web.Node.UnitTests` | Jest unit tests | Node.js frontend build scripts |
| `Dfe.PlanTech.Web.UnitTests` | xUnit unit tests | Controllers, ViewBuilders, tag helpers, authorisation |
| `Dfe.PlanTech.Web.E2ETests` | Cypress E2E tests | Full browser journey tests via DfE Sign-in |
| `Dfe.PlanTech.Web.E2ETests.Beta` | Cucumber/Playwright E2E tests | Regression and smoke E2E suite |
| `Dfe.PlanTech.Web.SeedTestData` | .NET console app | Creates a local SQL database with test data |
| `Dfe.PlanTech.UnitTests.Shared` | Shared library | Logging, NSubstitute, and reflection helpers for all unit tests |

> `Dfe.PlanTech.Infrastructure.Data.UnitTests` exists as an empty project — the `Infrastructure.Data` source project was removed and tests were not migrated.

## Running unit tests

From the repository root:

```bash
dotnet test
```

To run with code coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

To generate an HTML coverage report (requires [reportgenerator](https://github.com/danielpalme/ReportGenerator)):

```bash
reportgenerator -reports:"**/TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

Install the tool globally with:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

## E2E tests

There are two Cypress/Playwright suites at different stages of maturity:

### `Dfe.PlanTech.Web.E2ETests` (Cypress)

The original E2E suite. Uses Cypress with `cypress-axe` for accessibility testing. Requires real DfE Sign-in credentials and Contentful API keys. Includes a **Dynamic Page Validator** that exports Contentful data and validates every page component's rendered output against the CMS content — see [`Dfe.PlanTech.Web.E2ETests/cypress/e2e/dynamic-page-validator/dynamic-page-validator-readme.md`](Dfe.PlanTech.Web.E2ETests/cypress/e2e/dynamic-page-validator/dynamic-page-validator-readme.md) for setup.

See [`Dfe.PlanTech.Web.E2ETests/README.md`](Dfe.PlanTech.Web.E2ETests/README.md) for setup and environment variables.

### `Dfe.PlanTech.Web.E2ETests.Beta` (Cucumber + Playwright)

A newer regression and smoke suite written with Cucumber feature files and Playwright. Tests are tagged `@parallel` or `@serial` (parallel tests have no inter-test state; serial tests interact with assessments). Two VS Code workspaces separate regression and smoke to keep Cucumber step definition intellisense unambiguous.

See [`Dfe.PlanTech.Web.E2ETests.Beta/README.md`](Dfe.PlanTech.Web.E2ETests.Beta/README.md) for setup, environment variables, and running commands.

## Local test database

`Dfe.PlanTech.Web.SeedTestData` creates a local SQL Server database seeded with the PlanTech schema and initial test data. Useful for running the application locally without needing access to a shared environment database.

See [`Dfe.PlanTech.Web.SeedTestData/README.md`](Dfe.PlanTech.Web.SeedTestData/README.md) for Docker and manual setup instructions.
