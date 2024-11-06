# Contentful

We use [Contentful](https://contentful.com/) as our CMS of choice.

## Content types

We have a wide number of content types, used for a variety of purposes.

Here is an overview of our Contentful content types:
![CMS DB Schema](./plan-tech-contentful-content-types-schema.png)

## Reading and using Contentful data

Please read our [Contentful content usage](./contentful-content-usage.md) documentation to learn how we retrieve data from Contentful, how we map it to C# classes, use it in views, etc.

## Caching

We cache content data that was retrieved with a redis cache in our web app.

When content is cached, any nested content components within treat the parent component as a dependency.
If a child component needs invalidating, it is removed from the cache, as well as any of its depencencies.
This is explained in [contentful redis caching documentation](docs/cms/contentful-redis-caching.md)
