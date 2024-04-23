# DFE PlanTech CMS DB Data Validator

- Compares exported data from Contentful vs Database data to ensure DB is correct.

## How to

- Export Contentful content
- Save as "contentful-export.json" in root of project
- Add the DB connection string as a user secret by running `dotnet user-secrets set ConnectionStrings:Database CONNECTION-STRING`
- dotnet run

## FAQ

## Cavats