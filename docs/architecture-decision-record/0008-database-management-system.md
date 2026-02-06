# 0008 - Database Management System

- **Status**: Accepted

## Context and Problem Statement

There are a range of database vendors used with the DfE. Which should be used on this project to secure audit data that is tailored to invvidual Schools?

## Decision Drivers

- Compatible with selected hosting platform
- Compatible with selected application framework
- Cloud first as per Technology Code of Practice
  - 99.9% uptime SLA
- Prominent and popular within industry
- Can be easily be stubbed in a local Development environment

## Considered Options

- Azure SQL Database
- PostgreSQL

## Decision Outcome

Chosen option: [Azure SQL Database](https://azure.microsoft.com/en-us/products/azure-sql/database) as this is standard within the .NET ecosystem with rich application support.
