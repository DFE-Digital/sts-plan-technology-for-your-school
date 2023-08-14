# GitHub Workflows

## PR Checks

The following GitHub workflows are used during the PR process to validate the changes being merged into the `main` branch

| Workflow           | Description                                                                         |
| ------------------ | ----------------------------------------------------------------------------------- |
| code-pr-check      | Builds projects, runs unit tests, runs E2E tests, pushes test results to SonarCloud |
| terraform-pr-check | Validates the Terraform configuration                                               |
| e2e-tests          | Runs E2E tests (using Cypress)                                                      |

### terraform-pr-check workflow

This workflow validates the following:

* Validates the Terraform configuration by running Init/Plan
* Checks the Terraform format 
* Runs Terraform Linter
* Validates that the Terraform configuration doc is upto date
* Runs a Terraform Security Check

And will update the PR with the Plan results so reviews can easily see what changes will be applied to the infrastructure.

## Deployment pipelines

The following GitHub workflows are used when a PR is merged into the `main` branch

| Workflow         | Description                                                                                                                                                      |
| ---------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| matrix-deploy    | Automatically runs when code is merged into main to kick off build and deploy job for dev and test environments (Can be manually triggered for Staging and Prod) |
| build-and-deploy | Builds and deploys application for passed in environment                                                                                                         |
| terraform-deploy | Deploys Terraform to Azure environments                                                                                                                          |

## Other Workflows

| Name             | Description                                                                                                      |
| ---------------- | ---------------------------------------------------------------------------------------------------------------- |
| build-web-assets | Builds JS + CSS files (and bundles, minifies, etc. as necessary) on PR, then pushes changes to the source branch |

## Workflow Actions

Reusable workflow actions are located within the `.github/actions` directory