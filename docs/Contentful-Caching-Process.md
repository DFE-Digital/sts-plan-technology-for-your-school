#  Contentful Caching via Database Process

## Overview

- Webhook posts to Azure function
- Azure function writes to queue
- Another Azure function reads from queue + writes to database
- Data is read from DB where applicable, if any failure then an attempt to load from Contentful is performed

## Contentful -> DB Process

![Contentful to database architecture][/docs/diagrams/cms-to-db-process-flow.png]

- Code is contained in the [/src/Dfe.PlanTech.AzureFunctions/](/src/Dfe.PlanTech.AzureFunctions/) project.

### Webhook -> Queue

- We have a webhook on Contentful setup for entries
- The webhook is setup to trigger on all events (Create, Save, Autosave, Archive, Unarchive, Publish, Unpublish, Delete) for an entry
- The webhook points to an API hosted as an Azure Function [/src/Dfe.PlanTech.AzureFunctions/ContentfulWebHook.cs](/src/Dfe.PlanTech.AzureFunctions/ContentfulWebHook.cs)
- The API receives the JSON payload from the webhook, and writes it to an Azure Servicebus queue.
  - There is no validation of the JSON payload at this point.

- The Azure Function for this is secured by the inbuilt Azure Function HTTP authentication

### Queue -> DB

- We have another Azure Function which is triggered by messages on the Azure Servicebus (/src/Dfe.PlanTech.AzureFunctions/QueueReceiver.cs)
  - These messages are batched where applicable, using the default batch settings

- The Azure Function reads the message from the queue and, for each message,
  1. Strips out unnecessary information from the JSON payload, converting the JSON to just be what the entry value is
  2. Adds necessary relationship data for an entry, if any, into the JSON
  3. Deerialises the JSON to the _database entity model_
  4. Updates the entry status columns where appropriate (i.e. archived/published/deleted)
  5. Upserts the entry in the database

#### Mapping

- JSON is normalised
- Main entity is deserialised to class from JSON
- If the entity exists in DB, we copy values over
  - Attribute ignores certain things
  - Ignore properties ending in "Id"
- Any related entities are created + attached to DB. Then we make the necessary changes.
  - Where this is done
  - Why this is done

## DB Architecture

### Schema

![CMS DB Schema](/docs/diagrams/published/PTFYS%20CMS%20Schema.png)

### Functions

## Reading from database

We use EF Core as our ORM for reading/writing to the database. The DbContext used is [/src/Dfe.PlanTech.Infrastructure.Data/CmsDbContext.cs](/src/Dfe.PlanTech.Infrastructure.Data/CmsDbContext.cs).

### Mapping

AutoMapper is used for the majority of the mapping, as the DB models + Contentful models are 1-1 mapped for the vast majority of the fields/properties. The mapping profile for this is located in [/src/Dfe.PlanTech.Application/Mappings/MappingProfile.cs](/src/Dfe.PlanTech.Application/Mappings/MappingProfile.cs).

There are _some_ custom mappings done using LINQ `select` projections, due to various issues such as cyclical navigations with certain tables, or simply to limit what data is returned.

### Read navigation links

[/src/Dfe.PlanTech.Application/Content/Queries/GetNavigationQuery.cs] is where we retrieve navigation links for the footer. It is a simple query, merely returning the entire database table.

### Read page

[/src/Dfe.PlanTech.Application/Content/Queries/GetPageQuery.cs](/src/Dfe.PlanTech.Application/Content/Queries/GetPageQuery.cs) is responsible for retrieving Page data.

There are several steps to it:

1. We retrieve the `Page` matching the Slug from the table, and Include all `BeforeTitleContent` and `Content`. Note: there are various AutoIncludes defined in the [/src/Dfe.PlanTech.Infrastructure.Data/CmsDbContext.cs](/src/Dfe.PlanTech.Infrastructure.Data/CmsDbContext.cs) for the majority of the content tables, to ensure all data is pulled in.
  
2. Due to various issues with certain navigations, we execute various other queries and then merge the data together. These are completed using various `IGetPageChildrenQuery` objects that are injected into the `GetPageQuery` in the constructor using DI.

  -  If there is any content which has a `RichTextContent` property (using the `IHasText` interface to check), we execute a query to retrieve all the `RichTextContent` for the Page. The `RichTextMark` and `RichTextData` tables are joined automatically. This is handled by the [GetRichTextsQuery](/src/Dfe.PlanTech.Application/Content/Queries/GetRichTextsQuery.cs) class.
  
  - If there is any `ButtonWithEntryReference` content, then we retrieve the `LinkToEntry` property from it manually. This is to ensure we do not have a cartesian explosion (i.e. retrieving that entire page, + content, etc.), and to minimise the data we retrieve from the database (we only retrieve the slug and Id fields for the `LinkToEntry` property).  This is handled by the [GetButtonWithEntryReferencesQuery](/src/Dfe.PlanTech.Application/Content/Queries/GetButtonWithEntryReferencesQuery.cs) class.

  - If there are any `Category` content, we retrieve the `Sections` for them manually. This is because we also need to retrieve the `Question`s and `Recommendation`s for each Section, but only certain pieces of information. To prevent execessive data being retrieved, we only query the necessary fields. This is handled by the [GetCategorySectionsQuery](/src/Dfe.PlanTech.Application/Content/Queries/GetCategorySectionsQuery.cs) class.

3. We then use AutoMapper to map the database models to the Contentful models, as previously described.

## Caching

- Not implemented currently.