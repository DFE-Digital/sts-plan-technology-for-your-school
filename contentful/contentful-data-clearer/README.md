# Contentful Data Clearer

This is a small, quick script to clear _all_ data from an environment on Contentful.

## Setup

1. Copy `.env.example` and rename to `.env`
2. Populate the required variables in the file
3. Run the script via your terminal by running `node ./main.mjs` to delete _all content types_

_Note: If you want to just delete content for specific content type(s), add a variable called *CONTENT_TYPES_TO_DELETE* in your .env file which should contain a comma separated list of content types to delete_

## Warnings

There is absolutely no confirmation before deletion or anything like that. Make sure you use the right environment variables before running it!

You don't want to accidentally delete everything somewhere else.

Highly recommend backing up your data first!