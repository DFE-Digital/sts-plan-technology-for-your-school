# 0025 - Hybrid Matrix Deployments

- **Status**: accepted

## Context and Problem Statement

What's the best way we can achieve a hybrid deployment strategy which would allow us to deploy to development and test
environments automatically but let us control releases into the Staging and Production environment.

## Decision Drivers

- Automation
- Governance

## Considered Options

### Having Seperate workflow for each environment

- Requires a different branching strategy, possibly having a release branch.
- Introduces an additional level of complexity and possibility of duplication in workflows

### Matrix Deployments

- Requires new workflow to orchestrate deployments to dedicated environments
- Gives us the flexibility to add/remove environments without having to create new workflows.

## Decision Outcome

The decision has been made to add a new parent workflow which will allow us to use matrix deployments for dev/test environment
but also include manual/optional jobs to deploy to staging and production environment.

Staging/Production will have additional rules configured in github to add approvals when manual releases are made to these environments.
