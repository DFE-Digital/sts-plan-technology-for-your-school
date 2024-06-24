# DFE - Plan Tech - Azure Functions - E2E Tests

This project contains end-to-end tests for the Azure Functions.

## Detailed Overview

Currently this project only contains test that the `QueueReceiver.cs` from start to finish for every single content type currently used.

For each content type, the `QueueReceiver.cs` is ran with tests that:
1. Test publishing content
2. Test published content is updated when it receives another `publish` event
3. Test publishing content, then ensuring `save` and `auto-save` changes are _ignored_ when the relevant configuration options are set to `false`
4. Test publishing then unpublishing content
5. Test publishing content, then ensuring `save` and `auto-save` changes _update the database_ when the relevant configuration options are set to `true`
6. Test publishing content and then archiving it
7. Test publishing content, archiving it, then unarchiving it
8. Test deleting content

For each of these tests it will create JSON payloads that would match the Contentful Webhook payloads, converts them into the Service Bus message class type, and runs the QueueReceiver. Since we use a local database (described below) for the tests, we can be ensured that the tests are running against the same database as the production code, and work as they should in production.

Each content type test is created by inherting [`EntityTests`](/EntityTests/EntityTests.cs), which contains the common setup and teardown methods for each content type, along with the actual tests themselves. Each derived class should not have much code.

In addition to the above, there are various [entity generators](/Generators/BaseGenerator.cs), which are used by the entity tests to generate JSON payloads for each content type. They use [Bogus](https://github.com/bchavez/Bogus) to generate random data.

For each content type test, there are potentially _thousands_ of various contents being created (e.g. related entities), meaning the tests _may_ take a while to run. At the moment these tests are excluded from the CI/CD pipeline intentionally as a result.

## Usage

### Requirements

- A MSSQL Server instance

### Setup

_Note: These instructions require Docker, but that is not required to run the tests. You can skip the Docker instructions and move on to the next section if you already have an MSSQL Server to test against._

#### Setup a database

1. Create an [Azure SQL Edge server] by running the following command in your terminal: `docker run --cap-add SYS_PTRACE -e 'ACCEPT_EULA=1' -e "MSSQL_SA_PASSWORD=Pa5ssw0rd@G0esH3r3" -p 1433:1433 --name azuresqledge -d mcr.microsoft.com/azure-sql-edge`.

Notes: 
- Feel free to change the password, port, name, etc. in the command
- The password used needs to conform to Microsoft's password standards otherwise it will not work

2. Create the connection string for the server. With the above settings the connection string would be `Server=tcp:localhost,1433;Persist Security Info=False;User ID=sa;Password=Pa5ssw0rd@G0esH3r3;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;Max Pool Size=1000;`
3. Initialise the database with our schema by running the [Dfe.PlanTech.DatabaseUpgrader](../../src/Dfe.PlanTech.DatabaseUpgrader/) project against it. View that project's README (linked) for more information on how to do this.

#### Configure environment variables

The project is setup to use [.NET user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows) to retrieve the connection string (and any further secret variables that might be added in the future).

1. Initialise user-secrets against the project by running `dotnet user-secrets init` in the root of the project. Note: this may not need to be done as it is likely already configured
2. Add the database connection string to the database by running `dotnet user-secrets set ConnectionStrings:Database "Server=tcp:localhost,...."` in the terminal

### Run the application

Once the above setup is done, you can run the tests in a variety of ways:
1. `dotnet test` in a shell in the root of the project
2. Use your IDE's built in test runner to run the tests
3. Run specific tests only by running the command `dotnet test --filter "DisplayName...."`. E.g. `dotnet test --filter "DisplayName~RecommendationSection"` would run all tests where the `DisplayName` starts with `RecommendationSection` (i.e. all recommendation section tests), or `dotnet test --filter "DisplayName~RecommendationChunk&DisplayName~Unarchive"` would run all tests that match both of those criteria (i.e. the `unarchive` test for `RecommendationChunk`s in this instance). See [Microsoft's documentation](https://learn.microsoft.com/en-us/dotnet/core/testing/selective-unit-tests?pivots=mstest) for more information on how to run specific tests.