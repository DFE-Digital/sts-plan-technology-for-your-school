# Seeding Test Data

This project contains the code to seed an empty plan tech database with mock content.

The database can then be used with the plan tech web app to run more complex and detailed end to end tests with cypress that rely on content being consistent.

## Overview

The standard E2E tests that run against each environment are kept quite generic so that if content is changed, questions reordered, or topics added/removed, they continue to pass.

The purpose of this project, seeding with realistic mock content, is to support writing more complex and detailed end to end tests that rely on content being consistent.

This uses a very similar setup to the Azure Function E2E tests. See the [Readme](/tests/Dfe.PlanTech.CmsDbMigrations.E2ETests/README.md)
for a detailed guide on setting a mock database manually that this project can use.

## How to use

To use this project to get a seeded database for testing you need to:
1. setup a test database
2. run the database upgrader against it to initialise it with the PlanTech schema
3. run this project to populate the database with test data.

There is a bash script to do this, or you can run each step manually

### Bash script

#### Macbooks

If you're running this locally on a macbook with an M1 chip, the setup script won't work as `sqlcmd` is not available on Arm64 chips.

Instead you can install `sql-cli` to your machine with
```bash
npm install -g sql-cli
```

Then run `./scripts/setup-mac.sh` from the project root and pass a server name and password as arguments. For example:
```bash
./scripts/setup.sh azuresqledge Pa5ssw0rd@G0esH3r3
```

#### Standard Setup

Other machines can use `sqlcmd` so you just need to run `scripts/setup.sh` with a server name and admin password as arguments. For example:
```bash
./scripts/setup.sh azuresqledge Pa5ssw0rd@G0esH3r3
```

#### Note
- if you have permission errors when running either script, you can add execute permissions with chmod, for example:
```bash
chmod +x ./scripts/setup.sh
```


### Alternative setup

If having issues with the bash scripts or wanting more control over each step you can instead

1. Follow the steps in the [Azure E2E Tests setup](tests/Dfe.PlanTech.CmsDbMigrations.E2ETests/README.md) to create a database and run the database upgrader against it
2. Setup dotnet secrets for this project with the same connection string the Azure E2E test setup would require
3. Run this project to populate the database


### Using Plan Tech With Seeded Test Database

After running the above to create a database with mock content, you can try it against plan tech by editing your dotnet secrets for `src/Dfe.PlanTech.Web`
and changing your connection string to match the one set above.

So using the above name and password for example:
```
Server=tcp:localhost,1433;Persist Security Info=False;User ID=sa;Password=Pa5ssw0rd@G0esH3r3;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;Max Pool Size=1000;Database=plantech-mock-db
```

Any required content that has not been mocked, (e.g. at the moment, the index page) will fallback to Contentful to find it,
so Plan tech continues to operate off dev content unless it has been replaced with a fixed test alternative.
