# Database content

## Schema

![CMS DB Schema](/docs/diagrams/published/cms-db-schema.svg)

## Retrieving content

We use EF Core as our ORM for reading/writing to the database. The DbContext used is [/src/Dfe.PlanTech.Infrastructure.Data/CmsDbContext.cs](/src/Dfe.PlanTech.Infrastructure.Data/CmsDbContext.cs).

All of our DB queries are seperated into their own separate "query" classes, which are located in the [Dfe.PlanTech.Application](/src/Dfe.PlanTech.Application/) project. E.g. [GetPageFromDbQuery](/src/Dfe.PlanTech.Application/Content/Queries/GetPageFromDbQuery.cs)

We attempt to retrieve content from our DB first, if that fails we attempt to retrieve the content from Contentful. This is normally handled by a "parent" query, that will try to execute the DB query before executing a matching Contentful query if that fails. E.g. [GetPageQuery](/src/Dfe.PlanTech.Application/Content/Queries/GetPageFromDbQuery.cs).

### Retrieving navigation links

[/src/Dfe.PlanTech.Application/Content/Queries/GetNavigationQuery.cs] is where we retrieve navigation links for the footer. It is a simple query, merely returning the entire database table.

### Retrieving page

[/src/Dfe.PlanTech.Application/Content/Queries/GetPageQuery.cs](/src/Dfe.PlanTech.Application/Content/Queries/GetPageQuery.cs) is responsible for retrieving Page data.

There are several steps to it:

1. We retrieve the `Page` matching the Slug from the table, and Include all `BeforeTitleContent` and `Content`. Note: there are various AutoIncludes defined in the [/src/Dfe.PlanTech.Infrastructure.Data/CmsDbContext.cs](/src/Dfe.PlanTech.Infrastructure.Data/CmsDbContext.cs) for the majority of the content tables, to ensure all data is pulled in.
  
2. Due to various issues with certain navigations, we execute various other queries and then merge the data together. These are completed using various `IGetPageChildrenQuery` objects that are injected into the `GetPageQuery` in the constructor using DI.

  -  If there is any content which has a `RichTextContent` property (using the `IHasText` interface to check), we execute a query to retrieve all the `RichTextContent` for the Page. The `RichTextMark` and `RichTextData` tables are joined automatically. This is handled by the [GetRichTextsQuery](/src/Dfe.PlanTech.Application/Content/Queries/GetRichTextsQuery.cs) class.
  
  - If there is any `ButtonWithEntryReference` content, then we retrieve the `LinkToEntry` property from it manually. This is to ensure we do not have a cartesian explosion (i.e. retrieving that entire page, + content, etc.), and to minimise the data we retrieve from the database (we only retrieve the slug and Id fields for the `LinkToEntry` property).  This is handled by the [GetButtonWithEntryReferencesQuery](/src/Dfe.PlanTech.Application/Content/Queries/GetButtonWithEntryReferencesQuery.cs) class.

  - If there are any `Category` content, we retrieve the `Sections` for them manually. This is because we also need to retrieve the `Question`s and `Recommendation`s for each Section, but only certain pieces of information. To prevent execessive data being retrieved, we only query the necessary fields. This is handled by the [GetCategorySectionsQuery](/src/Dfe.PlanTech.Application/Content/Queries/GetCategorySectionsQuery.cs) class.

3. We then use AutoMapper to map the database models to the Contentful models, as previously described.

## Mapping

AutoMapper is used for the majority of the mapping, as the DB models + Contentful models are 1-1 mapped for the vast majority of the fields/properties. The mapping profile for this is located in [/src/Dfe.PlanTech.Application/Mappings/MappingProfile.cs](/src/Dfe.PlanTech.Application/Mappings/MappingProfile.cs).

There are _some_ custom mappings done using LINQ `select` projections, due to various issues such as cyclical navigations with certain tables, or simply to limit what data is returned.
