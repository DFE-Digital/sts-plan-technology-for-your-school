# Broken Link Checker

The broken link checker pulls down the Contentful data and isolates any links. It will check any external links within the content and then produce a list of internal links (e.g. /contact-us or https://plan-tech...) to check manually.

These are written to the console as well as outputted into two .csv files inside the /report/ directory.


### Environment Variables

- MANAGEMENT_TOKEN - This is the contentful management token 
- SPACE_ID - Contentful space ID
- CONTENTFUL_ENVIRONMENT - Contentful environment to target e.g. (work_in_progress/master/development)

### Running the script.

Navigate to the root broken-link-checker folder and type the command `npm run checker`

The pipeline for this script is in `.github/workflows/broken-link-checker.yaml`

Delete the export/contentful-data.json file if you require a fresh import.

### Scripts

`contentful-data-loader.js` - This script uses the export-processor to obtain the full contentful-data-json. It outputs the json to the /export directory.
`get-links.js` - This script isolates the links from the contentful-export.json and creates a new json file with those links inside the directory /result.
`checker.js` - This script uses the links.json from above and does a fetch on each link and outputs the results to the console/csv files.