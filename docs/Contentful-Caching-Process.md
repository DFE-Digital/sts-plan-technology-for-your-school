#  Contentful Caching via Database Process

## Overview

- Webhook posts to Azure function
- Azure function writes to queue
- Another Azure function reads from queue + writes to database
- Data is read from DB where applicable, if any failure then an attempt to load from Contentful is performed

## Cloud Architexcture

![Contentful to database architecture][/docs/diagrams/cms-to-db-process-flow.png]

### Webhook -> Queue

### Queue -> DB

### Mapping

## DB Architecture

### Schema

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