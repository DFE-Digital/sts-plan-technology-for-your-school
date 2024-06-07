# 0035 - Caching

* **Status**: accepted

## Context and Problem Statement

Now that we are using our DB for retrieving content for our web app, how do we cache the data we retrieve?

## Decision Drivers

- Open source:
- Development best practices:
- Speed of development

## Considered Options

Three primary options were evaluated in the decision-making process:

- Manual:
  - We could manually create functionality that stores data in a memory cache, on the web app.

- [EFCoreSecondLevelCacheInterceptor](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor)
  - One of the most well used open-source second level caching packages for EF Core


## Decision Outcome

We have chosen to use the EFCoreSecondLevelCacheInterceptor package. It is easy to setup, provides all functionality that we need, highly configurable, and open source. It fits our use cases perfectly.