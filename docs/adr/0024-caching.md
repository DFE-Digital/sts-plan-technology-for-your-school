# 0024 - Caching

* **Status**: proposed

## Context and Problem Statement

How do we best cache data within our web app?

## Decision Drivers

* Performance
* Scalability

## Considered Options

### In-memory Caching

- Requires minimal setup
- Less scalable than distributed

### Distributed Caching

- Requires infrastructure work
  - E.g. either an in-memory caching database such as `Redis`, or using SQL Server, etc.
- More scalable

## Decision Outcome

TBD