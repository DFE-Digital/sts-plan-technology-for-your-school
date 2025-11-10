# Seeding Test Data

This project contains the code to create an empty plan tech database with the correct schema.

This can be used to run plan tech locally against a local database of your own for testing schema changes, and work that involves database updates.

## Overview

The standard E2E tests that run against each environment are kept quite generic so that if content is changed, questions reordered, or topics added/removed, they continue to pass.

The long term goal of this project, was to add realistic mock content, to support writing more complex and detailed end to end tests that rely on content being consistent.

This project was created before the switch from storing content in SQL to caching content with Redis, so has become more useful as a locally running test database than its initial intention.

## Requirements

- Either an MSSQL server instance
- Or an installation of [docker](https://www.docker.com/) (Recommended)

## How to use

To use this project to get a local plan-tech database you need to:
1. Create a test database
2. Run the database upgrader against it to initialise it with the PlanTech schema
3. Run this project to populate the database with some required initial data.

If you have docker installed, there is a bash script to do this, or you can run each step manually

### Docker instructions

#### Macbooks / Machines without sqlcmd

If you're running this locally on a macbook with an Arm64 chip, or any other machine that doesn't support `sqlcmd`, the `setup.sh` script won't work.

Instead you can install `sql-cli` to your machine with
```bash
npm install -g sql-cli
```

Then run `./scripts/setup-sql-cli.sh` from the project root and pass a server name and password as arguments. For example:
- Mac/Linux
    ```bash
    ./scripts/setup-sql-cli.sh azuresqledge Pa5ssw0rd@G0esH3r3
    ```
- Windows
    ```bash
    sh scripts/setup-sql-cli.sh azuresqledge Pa5ssw0rd@G0esH3r3
    ```

#### Standard Setup

If your machine does support `sqlcmd` you can skip installing `sql-cli` and just run `scripts/setup.sh` with a server name and admin password as arguments. For example:
- Mac/Linux
    ```bash
    ./scripts/setup.sh azuresqledge Pa5ssw0rd@G0esH3r3
    ```
- Windows
    ```bash
    sh scripts/setup.sh azuresqledge Pa5ssw0rd@G0esH3r3
    ```

#### Note
- if you have permission errors when running either script, you can add execute permissions with chmod, for example:
```bash
chmod +x ./scripts/setup.sh
```


### Manual instructions

If you don't have docker and have an MSSQL instance to use instead, you need to

1. Create a new database
2. Follow the instructions in the [Database Upgrader Readme](../../src/Dfe.PlanTech.DatabaseUpgrader/README.md) to run the database upgrader against your database
3. Set the connection string to the database in the dotnet secrets for this project using the command
    ```bash
    dotnet user-secrets set "ConnectionStrings:Database" <connection_string>
    ```
4. Run this project to populate the database with the test data


### Using Plan Tech With Local Test Database

After running the above to create a database with mock content, you can try it against plan tech by editing your dotnet secrets for `src/Dfe.PlanTech.Web`
and changing your connection string to match the one set above.

So using the above name and password for example:
```
Server=tcp:localhost,1433;Persist Security Info=False;User ID=sa;Password=Pa5ssw0rd@G0esH3r3;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;Max Pool Size=1000;Database=plantech-mock-db
```

and then run Plan Tech as normal.
