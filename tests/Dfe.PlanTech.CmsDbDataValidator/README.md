# DFE PlanTech CMS DB Data Validator

- Compares exported data from Contentful vs Database data to ensure DB is correct.

## Usage

1. Setup your [local environment variables](#Setting-up-environment-variables)
2. Navigate to the project's root folder in your terminal
3. Run the project via `dotnet run`

### Setting up environment variables

Set the values below in [dotnet's user-secrets settings](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows). 
To do this, navigate to the project directory in your terminal, then run `dotnet user-secrets set KEY VALUE`

| Key                        | Description                                                                                                        | Optional? |
| -------------------------- | ------------------------------------------------------------------------------------------------------------------ | --------- |
| Contentful:Environment     | The Contentful environment to compare data against (e.g. dev, test, master)                                        | No        |
| Contentful:UsePreviewApi   | Whether to compare against draft content or not. False by default (i.e. will only validate against published data) | Yes       |
| Contentful:SpaceId         | The Contentful Space ID to export data from                                                                        | No        |
| Contentful:PreviewApiKey   | The API key for preview Content (only necessary if UsePreviewApi is set to true)                                   | Yes       |
| Contentful:DeliveryApiKey  | Delivery API key for the Contentful space                                                                          | No        |
| ConnectionStrings:Database | Connection string for the DB to compare data against                                                               | No        |
| Contentful:SaveExport      | Set to true to save the exported Contentful data as a JSON file for comparison                                     | Yes       |
