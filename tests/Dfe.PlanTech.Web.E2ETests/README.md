# Dfe.PlanTech.Web.E2ETests

## Overview

End-to-end testing for the [Dfe.PlanTech.Web](../../src/Dfe.PlanTech.Web/) project.

Created using [Cypress](https://cypress.io)

## Setup

1. Copy the `cypress.env.json.example` file and rename it to `cypress.env.json` in the root of the `Dfe.PlanTech.Web.E2ETests` folder
2. Populate the environment variables within it (guidance below).
3. Install the necessary packages by running `npm install` from `Dfe.PlanTech.Web.E2ETests`.
4. Run Cypress by running `npx cypress open --config "baseUrl=URL"` where URL is the same as in the variables mentioned below.

Further setup is required to use the Dynamic Page Validator - see the dedicated [README](./cypress/e2e/dynamic-page-validator/dynamic-page-validator-readme.md).

If testing on the Staging environment, the USE_PREVIEW and PREVIEW_TOKEN environment variables should be set to allow testing of preview content.

### Variables

| Name                   | Description                                                                                                                 |
| ---------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| URL                    | URL on which to run tests                                                                                                   |
| DSi_Url                | URL for DfE Sign-in Interactions                                                                                            |
| DSi_Email              | DfE Sign-in Username                                                                                                        |
| DSi_Password           | DfE Sign-in Password                                                                                                        |
| DSi_NoOrg_Email        | DfE Sign-in Username for a user who has no organisation                                                                     |
| DSi_NoOrg_Password     | Password for the DSi_NoOrg_Email user                                                                                       |
| SPACE_ID               | Id of the PlanTech Contentful space                                                                                         |
| DELIVERY_TOKEN         | Content Delivery API access token                                                                                           |
| MANAGEMENT_TOKEN       | Contentful personal access token                                                                                            |
| CONTENTFUL_ENVIRONMENT | Corresponding Contentful environment eg 'dev', 'master'                                                                     |
| PREVIEW_TOKEN          | Contentful Preview API access token - only required if testing preview content                                              |
| USE_PREVIEW            | Flag to indicate if preview content is to be tested (ie 'Draft' and 'Changed' on Contentful) - defaults to false if not set |
| APP_ENVIRONMENT        | What environment the web app is running under, e.g. 'dev', 'test', 'staging', 'production' etc                              |

To obtain DELIVERY_TOKEN, PREVIEW_TOKEN (if required) and SPACE_ID, log in to Contentful and select 'API Keys' from the Settings menu. Ensure that you select the environment that you will be testing and copy the hidden value from 'Content Delivery API - access token' or 'Content Preview API - access token'. This page also displays the space ID.

To populate MANAGEMENT_TOKEN, select 'CMA tokens', also found in 'Settings' and create a personal access token. Copy the token when prompted during creation as this cannot be viewed again.

## Packages Used

- [Cypress-Axe]() - Used for accessibility testing
