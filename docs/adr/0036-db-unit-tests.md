# 0036 - DB Unit Tests

* **Status**: proposed

## Context and Problem Statement

Unit testing against a database is difficult. It either involves mocking various components, substituting the database for one in memory, or creating a whole new local database just for testing.

Microsoft particularly recommend [the latter](https://learn.microsoft.com/en-us/ef/ef6/fundamentals/testing/mocking).

## Decision Drivers

- Ease of development
- Development best practices
- Meets requirements currently

## Considered Options

- Mocking:
  - Not recommended by Microsoft
  - We could mock the various DB Sets in the database
  - This is easily done with external Nuget packages such as [MockQueryable](https://www.nuget.org/packages/MockQueryable.Core)
 
- We could test against a local in memory database copy (e.g. SQLite):
  - Not recommended by Microsoft
  - Database provider used is unlikely to match MSSQL/Azure SQL 1-to-1, meaning errors could occur

- We could create a new database locally (or in CI/CD pipelines), using Docker for example, and perform unit tests against that.
  - Recommended solution by Microsoft
  - A lot of initial work
  - Would mean our unit tests would test against an _exact copy_ of the functionality of the database.


## Decision Outcome

Currently we are using a combination of all 3, depending on the exact needs of each unit tests.

However, we should probably move towards the last option where possible. Although it may take a bit of initial work, once done it should involve minimal changes to keep up-to-date (as we can, for example, just apply the DBUpgrader project against the local SQL instance).