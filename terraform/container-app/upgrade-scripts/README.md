## Overview

When applying new Terraform changes, sometimes there are manual changes that are required, for things such as importing existing Key Vault secrets into state, or tearing down resources in a specific order that TF fails to do.

This folder contains various scripts designed to be ran before a specific sprint/release, to ensure that the required changes are executed correctly, and consistently across every environment.

## Scripts

### Sprints

| Sprint # | Script readme                      |
| -------- | ---------------------------------- |
| 33       | [Sprint 33](./sprint_33/README.md) |


### Other

| Name                                                        | Description                                                                            |
| ----------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| [Key Vault State Import](./shared/keyvault/state_import.sh) | Retrieves a Key Vault secret's ID from Azure using the CLI, then imports into TF state |