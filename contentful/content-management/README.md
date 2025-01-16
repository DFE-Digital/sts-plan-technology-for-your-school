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
