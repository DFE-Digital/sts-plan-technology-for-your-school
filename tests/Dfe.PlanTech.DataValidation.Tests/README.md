# Dfe.PlanTech.DataValidation.Tests

## Overview

Test project that compares an exported copy of Contentful data vs data in the database.

## How to use

1. Export Contentful data using the Contentful CLI. See [the Contentful docs](https://www.contentful.com/developers/docs/tutorials/cli/import-and-export/) for information on how to do this.
2. Save the export as `contentful-export.json` in the project directory (e.g. [/tests/Dfe.PlanTech.DataValidation.Tests](/tests/Dfe.PlanTech.DataValidation.Tests))
3. Add the connection string for the relevant environment's database (e.g. dev, test, etc.) as an environment variable `ConnectionStrings:Database`. This can be done either via `appsettings.json`, or via [dotnet user-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows).
4. Run the tests via `dotnet test`