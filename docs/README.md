# Documentation

Technical documentation for the Plan Technology for Your School service.

## Contents

| Path | What it covers |
|---|---|
| [`architecture-decision-record/`](architecture-decision-record/README.md) | All ADRs — 44 decisions from web framework choice to GIAS data refresh |
| [`cms/`](cms/README.md) | Contentful content types, how CMS data is read and mapped, Redis caching strategy |
| [`Authentication.md`](Authentication.md) | DfE Sign-in integration and authentication flow |
| [`Conventions.md`](Conventions.md) | Coding and naming conventions used across the project |
| [`Routers.md`](Routers.md) | How page routing works, including slug-based routing and the `PageModelBinder` |

## Architecture Decision Records

ADRs are written in [MADR format](https://adr.github.io/madr/) and live in `architecture-decision-record/`. Each record covers context, the decision made, and the consequences. See the [full list](architecture-decision-record/README.md).

## CMS documentation

The `cms/` directory covers:
- **Content type schema** — a PNG diagram of all Contentful content types and their relationships
- **[`contentful-content-usage.md`](cms/contentful-content-usage.md)** — how Contentful data is fetched, resolved, and mapped to C# models
- **[`contentful-redis-caching.md`](cms/contentful-redis-caching.md)** — the Redis caching strategy, dependency tracking, and cache invalidation flow

## See also

- [Authentication overview](Authentication.md)
- [Coding conventions](Conventions.md)
- [Page routing](Routers.md)
- [CMS documentation](cms/README.md)
- [Architecture Decision Records](architecture-decision-record/README.md)
