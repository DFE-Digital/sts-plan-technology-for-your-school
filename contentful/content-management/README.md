# Scripting Content Management

Guidance for creating, updating and deleting content programatically from contentful

Unfortunately the contentful-migration api does not support deleting entries, so this is something that has to be done
with the content management api instead. Migrations, where possible, should be done with contentful-migration.
This can be used as a fallback

## Setup

1. Setup `.env` (copy `.env.example` and setup fields as necessary)
2. cd into the `content-management` directory
3. run `npm install`

## Usage

1. Add your content update script following the convention of `YYYYMMDD-HHMM-description-of-crud-operation.js`
2. run the update with
    ```bash
    npm run crud-operation YYYYMMDD-HHMM-description-of-crud-operation.js
    ```
____

## Importing Content

- importing content can be achieved using the `import-content` script, which will import content from a json file into a specified environment. The json file should be in the format of the `export-processor` export outputs
- `import-content` uses the `contentful-import` npm package to do the importing


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