# 0041 - Redis Cache

- **Status**: accepted

## Context and Problem Statement

We currently cache Contentful API responses in the SQL Database which is architecturally suboptimal because a central cache server provides a more transient, flexible and appropriate cache store for these items.

Redis was previously considered as an alternative option but not chosen due to the additional costs it would incur as well as the additional infrastructure.

Revisiting the option, a distributed cache would solve the problem of cache invalidation between replicas,
and if we use the basic plan, the cost is low, and simplifies our code.

## Decision Drivers

- Ease of use
- Performance
- Cost of infrastructure

## Decision Outcome

Switching to a redis cache means we can avoid in-memory cache strategies, which in turn will enable greater scalability of the service.
