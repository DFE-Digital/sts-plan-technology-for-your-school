## [Sprint 33](./sprint_33/main.sh)

### Usage

The script for sprint 33 can be ran using the following bash command:

```bash
bash upgrade-scripts/sprint_33/main.sh -g RESOURCE_GROUP_NAME -t PATH_TO_TERRAFORM_VARIABLES
```

Note: This assumes you are running the script from the [`container-app`](../) folder.

- **RESOURCE_GROUP_NAME**: The name of the resource group to apply changes to (e.g. "ENV-plantech")
- **PATH_TO_TERRAFORM_VARIABLES**: The relative path (from where you are executing the script) to your TF variables file for the specific environment. E.g.`terraform-test.tfvars`, or `../dev.tfvars` etc.

### Changes performed

This script performs the following changes:

1. Imports the following Key Vault secrets into the existing TF state:
   1. `csp--connectsrc`, `csp--imgsrc`, `csp--defaultsrc`, `csp--framesrc`