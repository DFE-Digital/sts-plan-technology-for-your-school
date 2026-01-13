# 0021 - Deploy Database Schema Changes

* **Status**: accepted

## Context and Problem Statement

How should database schema changes be deployed?

## Decision Drivers

* Within DfE’s Technical Guidance
  * [Independent database schema changes](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli)
  * Note: it stipulates that **any schema change should be done outside of any code change**
* DBUp is already used within DfE

## Considered Options

* [DbUp](https://github.com/DbUp/DbUp)
* [EF Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli)

## Decision Outcome

The team will be using DBUp for database upgrades as this is already used by other DfE projects and is the standard used by the Data team and Data Analyst could in the future be give the SQL generation tasks.  This was requested by SLT Lead Tech (Mamood Suyltan).