# GitHub Workflows


## PR Checks

The following GitHub workflows are used during the PR process to validate the changes being merged into the `main` branch

| Workflow           | description                                                             |
| ------------------ | ----------------------------------------------------------------------- |
| code-pr-check      | Validates that the code and docker image build, and that unit test pass |
| terraform-pr-check | Validates the Terraform configuration                                   |

## Workflow Actions

Reusable workflow actions are located within the `.github/actions` directory