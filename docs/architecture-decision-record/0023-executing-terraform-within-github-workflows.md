# 0023 - Execute Terraform Within Github Workflows

- **Status**: accepted

## Context and Problem Statement

What option should the PlanTech service use to execute Terraform within GitHub workflows?

## Decision Drivers

- Improve transparency of changes during the PR process
- Use the Terraform prefered approach

## Considered Options

- Terraform Docker Image ( [docker://hashicorp/terraform](https://hub.docker.com/r/hashicorp/terraform/#!) )
  - Other DfE projects use this approach
  - Approach not well documented by Terraform
  - Terraform advice that unless you need container isolation, we recommend using the non-containerized Terraform CLI packages.
  - Difficult to access the docker stdout output
- Setup Terraform Action ( [hashicorp/setup-terraform@v2](https://github.com/hashicorp/setup-terraform) )
  - The hashicorp/setup-terraform action is a JavaScript action that sets up Terraform CLI in your GitHub Actions workflow
  - This approach is well documented and is currently
  - Full access to stderr and stdout streams
  - Terraform provide a script that updates the PR improving transparency of Infrastructure changes during PRs

## Decision Outcome

We have made the decision to implement a new workflow `terraform-deploy.yml` which uses the Setup terraform action, within this new workflow we validate, plan and apply any terraform
changes automatically.

The benefits of this are:

- This will allow us to keep our infrastructure upto date as soon as there is a change to any of the terraform files.

- Having the terraform execute automatically will also save any dev setup required for a developer to run these manually from a local machine.

- Save the dev team digging around for secrets and environment variables; these will now be held in github secrets for each environment.
