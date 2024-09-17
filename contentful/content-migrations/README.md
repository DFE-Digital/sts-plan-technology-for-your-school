# Scripting Contentful Migrations

Guidance for making modifications to content types and updating entries programatically in contentful

## Setup

1. Setup `.env` (copy `.env.example` and setup fields as necessary)
2. cd into the `content-migrations` directory
3. run `npm install`

## Usage

Currently migrations are run manually, there will likely be a follow on piece to run these as part of a pipeline

1. Add your migration script following the convention of `YYYYMMDD-HHMM-description-of-migration.js`
2. **Ensure you've disabled the Contentful Webhook** so that changes won't be posted during the migration or stacked up and posted when it's turned back on
    - Go to `https://app.contentful.com/spaces/<space_id>/settings/webhooks`
    - Click on the webhook for the relevant environment
    - In Webhook settings, set `Active` to false and hit `Save`
3. run the migration script
    ```bash
    npm run migration YYYYMMDD-HHMM-description-of-migration.js
    ```
4. This will show you a plan for the migration about to happen, type `Y/N` to confirm
5. Re-enable the webhook

## Errors

In the event of an error that causes the database to be updated with incorrect content,
you can refresh the database from contentful using the [export-processor](contentful/export-processor/README.md)

## References

This follows the guidance in the contentful documentation on [scripting migrations](https://www.contentful.com/developers/docs/tutorials/cli/scripting-migrations/)
Code examples for various migrations can be found [here](https://github.com/contentful/contentful-migration/tree/main/examples)
