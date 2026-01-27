# Dfe.PlanTech.Data.Contentful.UnitTests

## Overview

Project containing Unit Tests for the [Dfe.PlanTech.Data.Contentful](../../src/Dfe.PlanTech.Data.Contentful/) project

## Packages

| Package     | Purpose               |
| ----------- | --------------------- |
| XUnit       | Test runner           |
| NSubstitute | Substituting services |

## How to run

To simply run the tests, run `dotnet test` from the project root folder

### With Code Coverage

To run the tests and generate unit coverage, run `dotnet test --collect:"XPlat Code Coverage"` from the project root folder instead

#### Generating HTML Reports

Run the following command from the project root folder:

```shell
reportgenerator \
-reports:"TestResults/{guid}/coverage.cobertura.xml" \
-targetdir:"coveragereport" \
-reporttypes:Html
```

Where "guid" is the relevant folder GUID from the generated tests. Alternatively, navigate to the relevant folder in your terminal, and replace the reports argument with just `"coverage.cobertuna.xml"`

**Note**: Requires dotnet-reportgenerator-globaltool. To install this globally, run `dotnet tool install -g dotnet-reportgenerator-globaltool`

---

## Additional Info

- Warning CS8625 (nullability warning) has been disabled in the csproj file. This is due to tests that _intentionally_ violate this, to ensure that they are handled correctly by the receiving method.

-

### Further Reading

- [Microsoft's "Use Code Coverage for unit testing"](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage)
