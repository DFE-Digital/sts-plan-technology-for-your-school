# Content Management

Scripts for creating, updating, and deleting Contentful content entries programmatically using the [Contentful Management API](https://www.contentful.com/developers/docs/references/content-management-api/).

> **Why this exists alongside `content-migrations`:** The `contentful-migration` library (used in `content-migrations`) shows a plan before executing — similar to `terraform plan`. The `contentful-management` library executes changes immediately. They are kept separate to reduce the risk of accidentally running immediate changes during a migration planning step. Use `content-migrations` where possible; use this as a fallback for operations that the migration library doesn't support (such as deleting entries).

## Setup

```bash
cd contentful/content-management
cp .env.example .env   # fill in values
npm install
```

| Variable | Required | Description |
|---|---|---|
| `MANAGEMENT_TOKEN` | Yes | Contentful management (CMA) token |
| `SPACE_ID` | Yes | Contentful space ID |
| `ENVIRONMENT` | Yes | Target environment (e.g. `PlanTech_Development`) |
| `DELIVERY_TOKEN` | For imports | Contentful delivery token |
| `CONTENT_FILE` | For imports | Path to JSON file to import |
| `SKIP_CONTENT_MODEL` | No | Skip content model import (default: `true`) |
| `DELETE_ALL_DATA` | No | Delete all content before import (default: `false`) |
| `CONTENT_TYPES_TO_DELETE` | No | Comma-separated content types to limit deletion scope |

> **Important:** The `contentful-management` library silently falls back to the `master` environment if `ENVIRONMENT` contains a typo. `get-client.js` guards against this by validating the value against the list of environments in the space before proceeding.

## Running a change script

Change scripts live in `changes/` and follow the naming convention `YYYYMMDD-HHMM-description.js`.

```bash
npm run crud-operation -- YYYYMMDD-HHMM-description.js
```

To add a new change:

1. Create a new file in `changes/` following the naming convention
2. Export a default async function that receives a Contentful client
3. Run it with the command above

## Importing content

Imports a JSON file (in Contentful export format) into the target environment. The JSON file should be in the format of the `export-processor` export outputs. By default, this adds or overwrites matching entries and leaves unrelated content alone.

Set `DELETE_ALL_DATA=true` to wipe the environment first so it exactly matches the import file.

```bash
node import-content.js
```

## Deleting content

### Delete all content

```bash
node delete-all-content.js
```

> **⚠ WARNING: This deletes content immediately with no confirmation prompt.** Double-check your `ENVIRONMENT` variable before running. Back up your data first. To scope deletion to specific content types, set `CONTENT_TYPES_TO_DELETE` to a comma-separated list.

### Delete specific content types

Set `CONTENT_TYPES_TO_DELETE=contentType1,contentType2` in `.env` before running `delete-all-content.js`.

## Tests

Tests live in `../tests/tests/content-management/`. To run them, navigate to `/contentful` and run:

```bash
npm run test
```

See [../tests/README.md](../tests/README.md) for how to run tests in debug mode.

## See also

- [Content migrations](../content-migrations/README.md) — prefer this tool for schema changes; it shows a plan before executing
- [Contentful tests](../tests/README.md) — test suite for this tool
- [Contentful tooling overview](../README.md)
