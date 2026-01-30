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
| [clear-submission-data-from-db](#clear-user-data-from-db) | Clears all submission-related data from a DB on a target environment for a selected test establishment                                                           |

### clear-user-data-from-db

This workflow runs an [SQL script](/.github/scripts/clear-submission-data-from-db.sql) that clears all recommendation histories, responses and submissions from the database. This gives a 'clean slate' for testing, demos etc.

It can only be triggered manually from either the `Actions` tab, or using the GitHub CLI tool. It takes two inputs; the target `environment` and the `establishment` (ie the establishment for which to clear submission data). The `environment` input takes Dev, Tst, StagingUnprotected and ProductionUnprotected, the latter two environments are specifically designed for running actions. The input options for `establishment` are limited to the three main DSI Test establishments: Community School, Foundation School and Miscalleous.

See below for information on testing manual workflows using GitHub CLI.

## Workflow Actions

Reusable workflow actions are located within the `.github/actions` directory

## Testing workflows with GitHub CLI

To test a new workflow using GitHub CLI without merging into development/main, the workflow file must first be pushed to the feature branch with `push:` set as a trigger (for a manual workflow, this can be added above `workflow-dispatch:` temporarily). This makes the workflow file discoverable as GitHub Actions will attempt to run it.

Once the file has been discovered, `push:` can be removed (if appropriate) and run manually with GitHub CLI, using the commands below. It is not necessary to add `push:` again once the file has been discovered, any pushed changes will be reflected.

To execute the workflow manually using the GitHub CLI tool, you can execute the following command (if preferred, `'<name of workflow>'` can be replaced with the filename):

```shell
gh workflow run '<name of workflow>' --ref <branch to run from>
```

If it is a manual workflow, inputs can be added following the branch name, using a `-f` flag for each, as in the example below:

```shell
gh workflow run clear-submission-data-from-db.yml --ref development -f environment="Dev" -f establishment="DSI TEST Establishment (001) Community School"
```
NB: Input names passed using `-f` must match exactly those declared in the YAML file.

The above run the workflow as it exists on the `development` branch, using `Dev` as the target environment and `DSI TEST Establishment (001) Community School` as the establishment to be cleared.
