# Dfe.PlanTech.Web.UnitTests

## Overview

Project containing Unit Tests for the [Dfe.PlanTech.Web](../../src/Dfe.PlanTech.Web/) project

## Packages

| Package | Purpose     |
| ------- | ----------- |
| XUnit   | Test runner |

## How to run

To simply run the tests, run `dotnet test` from the project root folder

### With Code Coverage

To run the tests and generate unit coverage, run `dotnet test --collect:"XPlat Code Coverage"` from the project root folder instead

#### Generating HTML Reports

Run the following command from the project root folder:

```shell
reportgenerator \
-reports:"TestResults/{GUID}/coverage.cobertura.xml" \
-targetdir:"coveragereport" \
-reporttypes:Html
```

Where "guid" is the relevant folder GUID from the generated tests. Alternatively, navigate to the relevant folder in your terminal, and replace the reports argument with just `"coverage.cobertuna.xml"`

**Note**: Requires dotnet-reportgenerator-globaltool. To install this globally, run `dotnet tool install -g dotnet-reportgenerator-globaltool`
