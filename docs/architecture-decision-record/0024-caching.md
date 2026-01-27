# 0024 - Caching

- **Status**: accepted

## Context and Problem Statement

How do we best cache data within our web app?

## Decision Drivers

- Performance
- Scalability

## Considered Options

### In-memory Caching

- Requires minimal setup
- Less scalable than distributed

### Distributed Caching

- Requires infrastructure work
  - E.g. either an in-memory caching database such as `Redis`, or using SQL Server, etc.
- More scalable

## Decision Outcome

Distributed caching is the only solution of the two that will work, due to incorrect caching across instances/containers/revisions.

For distributed caching, there are 3 main options:

- Redis
- SQL Server
- NCache

Both `Redis` and `NCache` would require additional infrastructure, which itself involves extra development time, and further costing.

There has been mentioned a priority of _minimal costing_ from a key stakeholder, so the only viable option therefore is using our existing `SQL Server`.

We are currently using the _Basic DTU_ tier, which would cost _66p_ per DTU. Assuming an entire DTU is used just for caching per month, this means it would only cost 66p per month for caching. This is an overestimation for MVS, due to limited schools being part of the beta.

There are possible implications on storage performance on the database, however this would be unlikely in the initial stages. If the performance does start to become an issue, there are various routes available to us such as either increasing the database service tier, or created a database _solely_ for caching, or switching to a different caching solution (e.g. Redis).

## Addendum

We are still to use caching in the app at this moment in time.
