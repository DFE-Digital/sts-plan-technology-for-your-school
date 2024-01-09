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

## DB Architecture

### Schema

![CMS DB Schema](/docs/diagrams/published/PTFYS%20CMS%20Schema.png)

### Functions

## Reading from database

- Uses EF Core

### Mapping

- Uses AutoMapper

### Read navigation links

### Read page

- Read page with content
- Most navigations autoincluded
- Some navigations left manually:
  - RichTextContent: was causing cyclical queries
  - ButtonWithEntryReference: need the LinkToEntry field but not ideal to autoinclude that entire piece. Done manually to minimise query
  - Sections: Requires loading specific pieces of data for questions + recommendations. Not ideal to load all of it so loaded manually

## Caching

TO DO