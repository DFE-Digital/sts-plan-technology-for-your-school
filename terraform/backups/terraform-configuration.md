# Terraform Module Configuration

> Note: This file is a companion file to [container-app/terraform-configuration](../container-app/terraform-configuration.md).

## Production-only

We only take backups for production, so tfvars are not split into environments here.

## Shared variables

Several variables are shared across the `backups` and `container-app` stacks.

Whenever variables are changed in `/container-app/terraform-*.tfvars`, they should also be changed in `/backups/terraform-*.tfvars`.
