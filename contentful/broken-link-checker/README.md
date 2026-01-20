# Broken Link Checker

The broken link checker pulls down the contentful data and isolates any links. It will check any external links within the content and then produce a list of internal links (e.g. /contact-us or https://plan-tech...) to check manually.


### Environment Variables

- MANAGEMENT_TOKEN - This is he contentful management token 
- SPACE_ID - Contentful space ID
- CONTENTFUL_ENVIRONMENT - Contentful environment to target e.g. (work_in_progress/master/development)

### Running the script.

Navigate to the root broken-link-checker folder and type the command `npm run checker`

The pipeline for this script is in `.github/workflows/broken-link-checker.yaml`

Delete the export/contentful-data.json file if you require a fresh import.