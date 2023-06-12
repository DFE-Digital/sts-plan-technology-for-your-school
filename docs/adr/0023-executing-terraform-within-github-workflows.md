# 0023 - Deploy Database Schema Changes

* **Status**: proposed

## Context and Problem Statement

What option should the PlanTech service use to execute Terraform within GitHub workflows?

## Decision Drivers

* Improve transparency of changes during the PR process
* Use the Terraform prefered approach

## Considered Options

* Terraform Docker Image ( [docker://hashicorp/terraform](https://hub.docker.com/r/hashicorp/terraform/#!) )
  * Other DfE projects use this approach
  * Approach not well documented by Terraform
  * Terraform advice that unless you need container isolation, we recommend using the non-containerized Terraform CLI packages.
  * Difficult to access the docker stdout output
* Setup Terraform Action ( [hashicorp/setup-terraform@v2](https://github.com/hashicorp/setup-terraform) )
  * The hashicorp/setup-terraform action is a JavaScript action that sets up Terraform CLI in your GitHub Actions workflow
  * This approach is well documented and is currently 
  * Full access to stderr and stdout streams
  * Terraform provide a script that updates the PR improving transparency of Infrastructure changes during PRs

## Decision Outcome

TBD