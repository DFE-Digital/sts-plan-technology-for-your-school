# Dfe.PlanTech.Infrastructure.Contentful

## Overview

Concrete implementations of IContentRepository using Contentful, along with helper functions for querying, setup, etc.

## How to use

Set the following environment variables:

```plaintext
Contentful:DeliveryApiKey = "<content_delivery_api_key>",
Contentful:PreviewApiKey, "<content_preview_api_key>"
Contentful:SpaceId = "<space_id>",

//OPTIONAL
Contentful:UsePreviewApi = true OR false
```

These are found under Settings -> API Keys.

Note: This assumes you setup the services using the section name "Contentful". E.g.:

```csharp
builder.Services.SetupContentfulClient(builder.Configuration, "Contentful");
```

Note: Depending on the OS, you will not be able to set an env variable with a colon - use double underscores instead. E.g. ```Contentful:DeliveryApiKey``` would become ```Contentful__DeliveryApiKey```
