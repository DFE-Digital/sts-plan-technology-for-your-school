# User Journey Info Scripts

## Overview

Javascript code that takes data that has been exported from Contentful, and for each section provides information about:

1. The total possible number of user journeys, grouped by maturity result
2. The minimum number of paths required to display every possible question
3. The shortest possible user journey possible to get each possible recommendation page (for each maturity) for each section
4. Optional: every single possible user journey.

It will also check for potential data issues, and output them in the console. Currently it validates:
1. That all questions for a section can be navigated to in at least one user journey.
2. All maturities for each section can be reached from at least one user journey

## Running

1. [Export the Contentful data via their CLI](https://www.contentful.com/developers/docs/tutorials/cli/import-and-export/)
2. Ensure the exported file JSON is in this directory (i.e. [/contentful/user-journey-info-scripts](/contentful/user-journey-info-scripts/))
3. Navigate to the directory above in a terminal
4. Run the script by running `node test-suit-generator.mjs '{CONTENTFUL_EXPORT_JSON_PATH}'` in the terminal. Where `{CONTENTFUL_EXPORT_JSON_PATH}` is the file path for your exported Contentful data. E.g. `node test-suit-generator.mjs 'contentfulexport.json'`, if your Contentful data is exported as `contentfulexport.json`.

By default, the script will _not_ export all possible user journeys per section. If you wish to do this, simply add another `true` as another argument to the run command on step 4. I.e. `node test-suit-generator.mjs '{CONTENTFUL_EXPORT_JSON_PATH}' true`

