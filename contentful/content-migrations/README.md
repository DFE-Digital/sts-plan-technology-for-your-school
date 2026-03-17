# Content Migrations

Scripts for making programmatic changes to Contentful content type schemas and entries using the [`contentful-migration`](https://github.com/contentful/contentful-migration) library.

> **`contentful-migration` vs `content-management`:** This library shows a plan of changes before executing — you confirm with `Y/N` before anything is applied. Use this in preference to `content-management` wherever possible.

## Setup

```bash
cd contentful/content-migrations
cp .env.example .env   # fill in values
npm install
```

| Variable | Description |
|---|---|
| `MANAGEMENT_TOKEN` | Contentful management API token |
| `SPACE_ID` | Contentful space ID |
| `ENVIRONMENT` | Target environment (e.g. `PlanTech_DevAndTest`) |

## Running a migration

```bash
npm run migration -- YYYYMMDD-HHMM-description-of-migration.js
```

The tool will display a plan of the changes to be made. Type `Y` to confirm or `N` to abort — **no changes are applied until you confirm**.

## Important: disable the webhook before migrating

Contentful webhooks fire on every content change. Running a migration with the webhook active will flood the application's Service Bus queue and potentially trigger excessive cache invalidations. Before running any migration:

1. Go to `https://app.contentful.com/spaces/<space_id>/settings/webhooks`
2. Click the webhook for the target environment
3. Set **Active** to `false` and save
4. Run the migration
5. Re-enable the webhook

## Adding a new migration

1. Create a file in `migrations/` following the naming convention: `YYYYMMDD-HHMM-description.js`
2. Write the migration using the `contentful-migration` API (see [official docs](https://www.contentful.com/developers/docs/tutorials/cli/scripting-migrations/) and [examples](https://github.com/contentful/contentful-migration/tree/main/examples))
3. Run it against a non-production environment first to verify the plan

## Existing migrations

| Script | What it does |
|---|---|
| `20240913-1700-simplify-recommendation-headers.js` | Converts recommendation chunk headers from linked entries to direct text fields |
| `20241010-2222-amend-references-view.js` | Updates reference field display configs and adds releases widget sidebar |
| `20241014-1117-fix-releases-widget.js` | Fixes releases widget positioning across all content types |
| `20241118-1239-add-content-reference-to-nav-link.js` | Adds entry reference field to `navigationLink` for internal page linking |
| `20241128-1400-add-validation-to-short-text-fields.js` | Adds regex validation to text fields (no leading/trailing whitespace; questions must end with `?`) |
| `20241219-1300-merge-text-body-models.js` | Migrates `CSBodyText` entries to `textBody` |
| `20241220-1200-amend-cspage-headings.js` | Adds subtitle field to `ContentSupportPage` |
| `20250108-1400-cspage-title-to-title-component.js` | Converts `CSHeading` entries to `title` content type |

## See also

- [Content management](../content-management/README.md) — fallback for operations the migration library doesn't support
- [Contentful tests](../tests/README.md) — test suite covering migrations
- [Contentful tooling overview](../README.md)
