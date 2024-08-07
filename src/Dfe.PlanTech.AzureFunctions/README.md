# Dfe.PlanTech.AzureFunctions

Azure Function App that receives Contentful webhook calls, and save the content updates to an SQL database.

## Setup

### Environment variables

You can add environment variables to the `Values` key in your `local.settings.json` file.

**If you are able to connect to the Azure Key Vault locally:**

Simply set a value `Key_Vault_Name`, which matches the simple name of the Key Vault for the environment.

**If you are _unable_ to connect to the Key Vault locally:**

The following environment values should be set:

- **ConnectionStrings__Database**: The SQL connection string for the SQL database
- **Contentful__UsePreview**: Whether the Azure Function should process non-published messages

The following environment values _can_ be set, but are optional:

- **CacheClear__Endpoint**: The URL endpoint for clearing the container app cache
- **CacheClear__ApiKeyName**: The header name for the cache clear route's authorisation
- **CacheClear__ApiKeyValue**: The value for the cache clear route's authorisation
- **MessageRetryHandlingOptions__MaxMessageDeliveryAttempts**: How many times a failed message should be retried before dead lettering
- **MessageRetryHandlingOptions__MessageDeliveryDelayInSeconds**: Minimum amount of delay before a failed message should be tried again