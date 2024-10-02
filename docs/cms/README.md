# Contentful

We use [Contentful](https://contentful.com/) as our CMS of choice.

## Content types

We have a wide number of content types, used for a variety of purposes.

Here is an overview of our Contentful content types:
![CMS DB Schema](./plan-tech-contentful-content-types-schema.png)

## Reading and using Contentful data

Please read our [Contentful content usage](./contentful-content-usage.md) documentation to learn how we retrieve data from Contentful, how we map it to C# classes, use it in views, etc.

## Database

We store content from Contentful in our database, and retrieve content from our DB as our first port of call.

For more information read the [Contentful to DB](./contentful-to-db.md) documentation for an overview of why, and how, we save to the database.
Or our [database content](./db-content.md) documentation for information on how we store and retrieve the content.

## Caching

We cache content data that was retrieved by the DB in-memory in our web app.

Currently caching is handled by an in memory cache defined in the QueryCacher class in [Dfe.PlanTech.Application/Caching/Models/QueryCacher.cs](/src/Dfe.PlanTech.Application/Caching/Models/QueryCacher.cs)
The only cache invalidation is invalidation of the whole cache as it intended only for use with the `CmsDbContext` and not any of the `dbo` tables which will be frequently updated. See [Conventions](/docs/Conventions.md) for more information.
The cache is invalided anytime a cms message with a content update is successfully processed.
