# 0021 - Deploy Database Schema Changes

* **Status**: proposed

## Context and Problem Statement

How should database schema changes be deployed?

## Decision Drivers

* Within DfE’s Technical Guidance
  * [Independent database schema changes](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli)
  * Note: it stipulates that **any schema change should be done outside of any code change**


## Considered Options

* [DbUp](https://github.com/DbUp/DbUp)
* [EF Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli)

## Decision Outcome

TBD