# Terraform

The following article discusses developer tools and how to run the **Terraform** and **Teraform-Docs** tools. [Terraform Configuration](./terraform-configuration.md) page contains a detailed view of the Terraform configuration.  The [GitHub Workflows](../docs/GitHub-Workflows.md) page contains details of the Terraform pull request workflow.

## Contents

- [Terraform](#terraform)
  - [Contents](#contents)
  - [Developer tools](#developer-tools)
  - [When Updating The Terraform Config](#when-updating-the-terraform-config)
  - [Running Terraform locally](#running-terraform-locally)
    - [Authenticating using Service Principle](#authenticating-using-service-principle)
    - [Terraform Init](#terraform-init)
    - [Terraform Plan](#terraform-plan)
    - [Terraform Validate](#terraform-validate)
    - [Terraform Format](#terraform-format)
  - [Terraform Documentation](#terraform-documentation)

## Developer tools

The following tools are recommended/required to work with Terraform locally

* [Visual Studio Code](https://code.visualstudio.com/)
  * VSCode Terraform Extension
* [Terraform](https://www.terraform.io/)
  * MacOS: 
    * Xcode: `xcode-select --install` 
    * Terraform: `brew tap hashicorp/tap`  
* [Terraform-Docs](https://terraform-docs.io/)
  * MacOS:
    * `brew install terraform-docs`
* [TFLint](https://github.com/terraform-linters/tflint)
  * MacOS:
    * `brew install tflint`
* [jq](https://jqlang.github.io/jq/)
  * MacOS:
    * `brew install jq`
* [tfsec](https://aquasecurity.github.io/tfsec/v1.28.1/)
  * MacOS:
    * `brew install tfsec`
* [coreutils](https://www.gnu.org/software/coreutils/)
  * MacOS:
    * `brew install coreutils`
* [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/) 
  * MacOS
    * `brew update && brew install azure-cli`

## When Updating The Terraform Config

When you make changes to the Terraform Config, ensure you run the following as these are checked by the `terraform-pr-check` GitHub workflow:

1. Terraform validate to validate your changes
2. Terraform plan to check you're making the correct changes 
3. Terraform format to reformate the TF files
4. Terraform lint to check for possible errors
5. Terraform documentation to regenerate the documentation

## Running Terraform locally

This section discusses how to set-up and run Terraform locally on a development machine.

⚠️ do not update the example Terraform configuration files with sensitive information and commit to repo ⚠️ 

### Authenticating using Service Principle
The Terraform configuration can be executed using an Azure Service Principle.  To do this you'll need to set the following environment variables:

MacOS:
```
export ARM_TENANT_ID = <tenant_id> 
export ARM_SUBSCRIPTION_ID = <subscription_id> 
export ARM_CLIENT_ID = <client_id>
export ARM_CLIENT_SECRET = <client_secret>
```

And sign in to Azure using the Azure CLI, as the Terraform module uses this for part of the infrastructure deployoyment:

```
az login --service-principal -u <client_id> -p <client_secret> --tenant <tenant_id> 
```

### Terraform Init 
Terraform needs to be initialised on your local machine before you can use it. To do this rename the `backend.tfvars.example` file to `backend.tfvars` and complete the configuration so that Terraform connects to the correct Azure Storage instance.  

Run the following command to initialise Terraform.

`terraform init -backend-config=backend.tfvars`

⚠️ tfvars files are ignored by git, but please ensure they do not get committed to the repo by accident ⚠️ 

### Terraform Plan
To run the plan command, first rename the `terraform.tfvars.example` file to `terraform.tfvars` and complete the following configuration.  

| Title          | description            |
| -------------- | ---------------------- |
| az_environment | The environment prefix |
| az_location    | The Azure location     |

Run the following command to execute the Plan commande: 

`terraform plan -var-file="terraform.tfvars"`

### Terraform Validate

The terraform validate command validates the configuration files.

`terraform validate`

### Terraform Format

Any changes to the Terraform configuration should be formatted correctly.  This can be done by running the following command:

`terraform fmt`

## Terraform Documentation

When the Terraform configuration is updated, the module documentation will need to be re-generated, to do this run the following command within a terminal window:

`terraform-docs markdown .` 

This will generate the `terraform-configuration.md` markdown file.