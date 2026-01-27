# Scripting Content Management

**Note - get-client.js has been updated to return an environment rather than a raw client - helper functions will not behave until these have been updated.**

Guidance for creating, updating and deleting content programatically from contentful

Unfortunately the contentful-migration api does not support deleting entries, so this is something that has to be done
with the content management api instead. Migrations, where possible, should be done with contentful-migration.
This can be used as a fallback.

The project is currently separate from the content-migrations project because the contentful-migration library doesn't execute changes when the script is run, instead, it outputs a plan for the migration which you can accept or reject (similar to terraform). The contentful-management library does execute changes immediately, so would run during plan creation rather than at the intended stage. To minimise the risk of developer error, these are kept apart.

## Setup

1. Setup `.env` (copy `.env.example` and setup fields as necessary)
2. cd into the `content-management` directory
3. run `npm install`

Note that the `contentful-management` library defaults to the `master` environment if there is a typo in the environment name. This is very undesirable so `validate-environment.js` will fetch all environments from contentful and ensure that `ENVIRONMENT` is one of them.

### Usage

1. Add your content update script following the convention of `YYYYMMDD-HHMM-description-of-crud-operation.js`
2. run the update with
   ```bash
   npm run crud-operation YYYYMMDD-HHMM-description-of-crud-operation.js
   ```

---

## Importing Content

- importing content can be achieved using the `import-content` script, which will import content from a json file into a specified environment. The json file should be in the format of the `export-processor` export outputs
- `import-content` uses the `contentful-import` npm package to do the importing
- By default a content import will add to an environment and not remove unrelated data
  - It will overwrite entries with the same id as one in the import file, and leave other entries alone
  - To delete all existing data prior to import so that the environment exactly matches the import file, set `DELETE_ALL_DATA` to true in `.env`

### Usage

1. Setup `.env` (copy `.env.example` and setup fields as necessary)
2. cd into the `content-management` directory
3. Run `npm install`
4. run `node import-content`

Required Environment variables
`CONTENT_FILE`
`SPACE_ID`
`MANAGEMENT_TOKEN`
`ENVIRONMENT` (environmentId default is 'master')
`SKIP_CONTENT_MODEL` (optional, default is false)
`DELETE_ALL_DATA` (optional, default is false)

## Deleting all content

You may wish to remove all content from an environment, for example, before importing from a backup.
This is an environment variable option in the import process, or it can be run standalone with the `delete-all-content` script, which will clear down the entire environment.

### Setup

1. Copy `.env.example` and rename to `.env`
2. Populate the required variables in the file
3. Run the script via your terminal by running `node delete-all-content.js` to delete _all content types_

_Note: If you want to just delete content for specific content type(s), add a variable called *CONTENT_TYPES_TO_DELETE* in your .env file which should contain a comma separated list of content types to delete_

### Warnings

There is absolutely no confirmation before deletion or anything like that. Make sure you use the right environment variables before running it!

You don't want to accidentally delete everything somewhere else.

Highly recommend backing up your data first!

## Tests

The test suite can be found in `../tests/tests/`. To run them, navigate to /contentful in the terminal and type `npm run test`.

For details of how to run in debug mode, see the [README.md file](../tests/README.md) in `../tests/`.
