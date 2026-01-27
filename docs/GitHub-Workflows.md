# GitHub Workflows

- [PR Checks](#pr-checks)
- [Deployment pipelines](#deployment-pipelines)
- [Other workflows](#other-workflows)

## PR Checks

The following GitHub workflows are used during the PR process to validate the changes being merged into the `main` branch

| Workflow           | Description                                                                         |
| ------------------ | ----------------------------------------------------------------------------------- |
| code-pr-check      | Builds projects, runs unit tests, runs E2E tests, pushes test results to SonarCloud |
| terraform-pr-check | Validates the Terraform configuration                                               |
| e2e-tests          | Runs E2E tests (using Cypress)                                                      |

### code-pr-check workflow

- Builds the main solution file (`plan-technology-for-your-school.sln`), and runs all its unit tests
- Builds the database upgrader project
- Builds and runs the unit tests for the `Dfe.PlanTech.Web.Node` project

### e2e-tests workflow

- Clears out all submissions for a particular testing establishment reference, so that all tests are fresh
- Runs end-to-end tests using Cypress

### terraform-pr-check workflow

This workflow validates the following:

- Validates the Terraform configuration by running Init/Plan
- Checks the Terraform format
- Runs Terraform Linter
- Validates that the Terraform configuration doc is upto date
- Runs a Terraform Security Check

And will update the PR with the Plan results so reviews can easily see what changes will be applied to the infrastructure.

## Deployment pipelines

The following GitHub workflows are used when a PR is merged into the `main` branch

| Workflow                                  | Description                                                                                                                                                      |
| ----------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| matrix-deploy                             | Automatically runs when code is merged into main to kick off build and deploy job for dev and test environments (Can be manually triggered for Staging and Prod) |
| [create-tag-release](#create-tag-release) | Tag and create GitHub releases based on the commits/merges into `main` and `development` branches                                                                |
| build-and-deploy                          | Builds and deploys application for passed in environment                                                                                                         |
| terraform-deploy                          | Deploys Terraform to Azure environments                                                                                                                          |

### create-tag-release

This workflow is called by the `Multi stage build & deploy` workflow, and is therefore triggered on commits/merges to the `development` and `main` branches.

It uses [Semantic Release](https://github.com/semantic-release/semantic-release) to:

1. Find out which commit(s) are have been added to the branch since the last tagged release
2. Look through those commits and, based on the commit title, figure out whether the release is a major, minor, or patch release
3. If necessary, i.e. there is a major/minor/patch release change, create the new release and tag it with the new appropriate version.

The process uses [semantic versioning](https://semver.org/) for the release numbers, and expects [conventional commits](https://www.conventionalcommits.org/en/v1.0.0/) for the commit details. As a result, it will only process commits that have an expected conventional commit title (e.g. `feat: ` or `test:`).

To ensure that the process works correctly as expected, you should ensure your commits follow the conventional commits specification.

## Other Workflows

| Name                                                | Description                                                                                                      |
| --------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| build-web-assets                                    | Builds JS + CSS files (and bundles, minifies, etc. as necessary) on PR, then pushes changes to the source branch |
| [clear-user-data-from-db](#clear-user-data-from-db) | Clears all user data from a DB on a target environment                                                           |

### clear-user-data-from-db

This workflow runs an [SQL script](/.github/scripts/clear-user-data-from-db.sql) that clears all user-based data from the database.

It can only be triggered manually from either the `Actions` tab, or using the GitHub CLI tool. It takes one input; the target `environment` i.e. dev/tst/staging. If `production` is used, it will not run.

To execute the workflow manually using the GitHub CLI tool, you can execute the following command :

```shell
gh workflow run 'Clear user data from DB' --ref development -f environment=development
```

Which would run the workflow as it exists in the `development` branch, using `development` as the target environment. To execute against tst, you would do

```shell
gh workflow run 'Clear user data from DB' --ref development -f environment=tst
```

etc.

## Workflow Actions

Reusable workflow actions are located within the `.github/actions` directory
