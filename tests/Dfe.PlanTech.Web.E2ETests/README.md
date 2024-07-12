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


### Variables

| Name                    | Description                                              |
| ----------------------- | -------------------------------------------------------- |
| URL                     | URL on which to run tests                                |
| DSi_Url                 | URL for DfE Sign-in Interactions                         |
| DSi_Email               | DfE Sign-in Username                                     |
| DSi_Password            | DfE Sign-in Password                                     |
| SPACE_ID                | Id of the PlanTech Contentful space                      | 
| DELIVERY_TOKEN          | Content Delivery API access token                        | 
| MANAGEMENT_TOKEN        | Contentful personal access token                         |
| CONTENTFUL_ENVIRONMENT  | Corresponding Contentful environment eg 'dev', 'master'  |

To obtain DELIVERY_TOKEN and SPACE_ID, log in to Contentful and select 'API Keys' from the Settings menu. Ensure the environment that you will be testing is selected and saved, and copy the hidden value from 'Content Delivery API - access token'. This page also displays the space ID.

To populate MANAGEMENT_TOKEN, select 'CMA tokens', also found in 'Settings' and create a personal access token. Copy the token when prompted during creation as this cannot be viewed again.


## Packages Used

- [Cypress-Axe]() - Used for accessibility testing