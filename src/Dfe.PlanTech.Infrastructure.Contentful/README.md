# Dfe.PlanTech.Infrastructure.Contentful

## Overview

Concrete implementations of IContentRepository using Contentful, along with helper functions for querying, setup, etc.

## How to use

Set the following environment variables:

```plaintext
Contentful:DeliveryApiKey = "<content_delivery_api_key>",
Contentful:PreviewApiKey, "<content_preview_api_key>"
Contentful:SpaceId = "<space_id>",
Contentful:SigningSecret = "<contentful_signing_secret>"

//OPTIONAL
Contentful:UsePreviewApi = true OR false
Contentful:Environment = "<environment>">
```

These are found under Settings -> API Keys.

Note: This assumes you setup the services using the section name "Contentful". E.g.:

`csharp
builder.Services.SetupContentfulClient(builder.Configuration, "Contentful");
`

Note: Depending on the OS, you will not be able to set an env variable with a colon - use double underscores instead. E.g. ```Contentful:DeliveryApiKey``` would become ```Contentful__DeliveryApiKey```

### Dotnet User-Secrets

It is advised to use [dotnet user-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-7.0&tabs=windows) to do the above for development.

E.g. `dotnet user-secrets set Contentful:SpaceId SPACEID`
