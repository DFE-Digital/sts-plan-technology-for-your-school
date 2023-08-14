# Dfe.PlanTech.Web.E2ETests

## Overview

End-to-end testing for the [Dfe.PlanTech.Web](../../src/Dfe.PlanTech.Web/) project.

Created using [Cypress](https://cypress.io)

## Setup

1. Copy the `cypress.env.json.example` file and rename it to `cypress.env.json` in the root of the `Dfe.PlanTech.Web.E2ETests` folder
2. Populate the variables within it
3. Install the necessary packages by running `npm install`
4. Run Cypress by running `npx cypress open --config "baseUrl=URL"` where URL is the same as in the variables mentioned below


### Variables

| Name         | Description                                  |
| ------------ | -------------------------------------------- |
| URL          | URL to run tests on (e.g. www.plan-tech.com) |
| DSi_Url      | URL for DFE Sign-in Interactions             |
| DSi_Email    | DFE Sign-in Username                         |
| DSi_Password | DFE Sign-in Password                         |

## Packages Used

- [Cypress-Axe]() - Used for accessibility testing