# GitHub Workflows


## PR Checks

The following GitHub workflows are used during the PR process to validate the changes being merged into the `main` branch

| Workflow           | description                                                             |
| ------------------ | ----------------------------------------------------------------------- |
| code-pr-check      | Validates that the code and docker image build, and that unit test pass |
| terraform-pr-check | Validates the Terraform configuration                                   |

### terraform-pr-check workflow

This workflow validates the following:

* Validates the Terraform configuration by running Init/Plan
* Checks the Terraform format 
* Runs Terraform Linter
* Validates that the Terraform configuration doc is upto date
* Runs a Terraform Security Check

And will update the PR with the Plan results so reviews can easily see what changes will be applied to the infrastructure.

## Workflow Actions

Reusable workflow actions are located within the `.github/actions` directory