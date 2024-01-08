#  Contentful Caching via Database Process

## Overview

- Webhook posts to Azure function
- Azure function writes to queue
- Another Azure function reads from queue + writes to database
- Data is read from DB where applicable, if any failure then an attempt to load from Contentful is performed

## Cloud Architexcture

![Contentful to database architecture][/docs/diagrams/cms-to-db-process-flow.png]

## Reading from database

### Mapping

### Read navigation links

### Read page

## Caching

TO DO