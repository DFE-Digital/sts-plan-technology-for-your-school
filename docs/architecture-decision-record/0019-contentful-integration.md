# 0019 - Contentful Integration

- **Status**: accepted

## Context and Problem Statement

There are a variety of ways we could manage integration with Contentful; which method should we use for this project?

## Decision Drivers

- Open source
- Within DfE’s Technical Guidance
- Ease of development
- Extensibility + portability; ideally the functionality we create should, in the future, be able to be shared, and re-used, elsewhere

## Considered Options

- Contentful's existing Nuget package ([contentful.net](https://github.com/contentful/contentful.net))
- Developing our own service(s) to connect to Contentful's HTTP APIs
- Developing our own service(s) to connect to Contentful's REST APIs

## Pros and Cons of the Options

### Contentful.net

- Good; functionality already exists, so development should be quicker
- Good; it is open-source, so fits within DfE's technical guidance

- Neutral; it has a lot of functionality that we are unlikely to use (such as their Content Management API).

- Bad; we would want to abstract their implementation, which would require a lot of extra development regardless
- Bad; their implementation uses JSON.Net, which is considered outdated. Modern frameworks tend to use Microsoft's own System.Text.JSON

### Develop own service to access Contentful's HTTP or GraphQL APIs

- Good; Complete freedom to do what we want, and ensure the functionality we develop is exactly what is needed

- Bad; potentially more development time

## Decision Outcome

We are using the contentful.net C# package for our code, as it easily fulfills our requirements for MVS, and is already being used in DFE projects.
