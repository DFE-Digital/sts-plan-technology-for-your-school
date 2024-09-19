# 0038 - SQL Caching Mechanism

* **Status**: proposed

## Context and Problem Statement

We currently use EFCoreSecondLevelCache to cache database calls.
This caches the results for EF commands so the same command doesn't need executing again.
However we discovered during load testing that this still makes a connection to the database before returning the cached results:

A standard call with debugging logs enabled looks like this:
- Opened a connection to database ...
- Creating DbCommand for 'ExecuteReader'
- Executing DbCommand
- EFCoreSecondLevelCacheInterceptor suppressed the result with TableRows from the cache
- Returning the cached TableRows
- Closing data reader
- A data reader is disposed after spending 0ms reading results
- Closing connection to database
- Closed connection to database

The connection happens regardless of whether the results are cached, which limits the number of concurrent calls that can be made. We're therefore considering alternatives.

## Decision Drivers

- Ease of development
- Reliability of the solution
- Scalability of the solution

## Considered Options

### Continue using EFCoreSecondLevelCache

- Pros
    - No code changes
    - Its a popular external library that reliably does the bulk of what we need
- Cons
    - Unnecessary database connections made for every query
    - Since we don't make changes to the CMS data, we don't need the query tracking for these tables that EFCoreSecondLevelCache includes

### Implement our own caching mechanism

- Pros
    - Gives us the most flexibility, and guarantees solving the problem
    - Allows us to add in additional options like query compiling for improved performance
    - Forces us to make all fetching of queryable results consistent
    - As we don't require cache invalidation, the complexity is relatively low
- Cons
    - Relies on remembering to use the cached methods or extensions for the CMSDBContext and not the default ones

## Decision Outcome

TBC
