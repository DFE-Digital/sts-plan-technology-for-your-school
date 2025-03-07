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
    - [Terraform Plan](#terraform-plan-1)
    - [Terraform Validate](#terraform-validate)
    - [Terraform Format](#terraform-format)
  - [Terraform Documentation](#terraform-documentation)
  - [Troubleshooting](#troubleshooting)
    - [KeyVault Firewall Error](#keyvault-firewall-error)
    - [Logging](#logging)
    - [Output variables](#output-variables)

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

⚠️ Avoid committing any updates to the repository that include sensitive information in the example Terraform configuration files ⚠️

### Authenticating using Service Principle
The Terraform configuration can be executed using an Azure Service Principle.  To do this you'll need to set the following environment variables:

MacOS:
```
export ARM_TENANT_ID = <tenant_id> 
export ARM_SUBSCRIPTION_ID = <subscription_id> 
export ARM_CLIENT_ID = <client_id>
export ARM_CLIENT_SECRET = <client_secret>
```

| Key                   | Location                                                      |
| --------------------- |---------------------------------------------------------------|
| ARM_TENANT_ID         | DfE Platform Identity Overview (same for all envs)            |
| ARM_SUBSCRIPTION_ID   | Managed identities -> s190XXX -> Subscription ID              |
| ARM_CLIENT_ID         | go to s190-XXX terraform overview, and use the application ID |
| ARM_CLIENT_SECRET     | service-principal-secret from the tf-kv                       |

And sign in to Azure using the Azure CLI, as the Terraform module uses this for part of the infrastructure deployoyment:

```
az login --service-principal -u $ARM_CLIENT_ID -p $ARM_CLIENT_SECRET --tenant $ARM_TENANT_ID
```

### Terraform Init 

Terraform needs to be initialised on your local machine before you can use it. To do this rename the `backend.tfvars.example` file to `backend.tfvars` and complete the configuration so that Terraform connects to the correct Azure Storage instance.  

Run the following command to initialise Terraform.

`terraform init -backend-config=backend.tfvars -upgrade -reconfigure`

⚠️ tfvars files are ignored by git, but please ensure they do not get committed to the repo by accident ⚠️ 

### Terraform Plan

To run the plan command, first rename the `terraform.tfvars.example` file to `terraform.tfvars` and complete the following configuration.  

| Title                                     | description                                                                  |
| ----------------------------------------- | ---------------------------------------------------------------------------- |
| az_environment                            | The environment prefix                                                       |
| az_location                               | The Azure location                                                           |
| project_name                              | DFE Project name                                                             |
| environment                               | Azure environment (e.g. "dev")                                               |
| azure_location                            | Azure location (e.g. "westeurope")                                           |
| az_tag_environment                        | Environment tag for Azure resources (e.g. "Dev")                             |
| az_tag_product                            | Product tag for Azure resources                                              |
| cdn_frontdoor_origin_host_header_override | Override header for the frontdoor CDN                                        |
| az_sql_admin_password                     | Password for SQL server admin                                                |
| az_sql_admin_userid_postfix               | User for the SQL server  (this will be prepended by the resource group name) |
| az_app_kestrel_endpoint                   | Expected endpoint for the Kestrel server (e.g. "https://127.0.0.1:8080)      |

Run the following command to execute the Plan command: 

`terraform plan -var-file="terraform.tfvars"`

**NB: Make sure that the image_tag field is set to the most recent container address - usually of the form `main...`. The current container can be found through Azure Portal -> Container Registries -> Services -> Repositories -> plan-tech-app**

For the moment, if you get a 403 then give IP-specific access to the `plantechcosting` storage account for the relevant environment to the person deploying the change. (Permissions may be needed.)

TODO: this is not acceptable, require a proper fix, for now use the follwoing workaround.  
* Go to plantechcosting | Networking > Enable `Public network access from all Network`
* Disable after run Terraform

### Terraform Apply

Use the apply command to apply the changes to the Azure environment.

`terraform apply -var-file="terraform.tfvars"`

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

## Troubleshooting

### KeyVault Firewall Error

When running locally you see an error similar to the following:

> Making Read request on Azure KeyVault Secret.  Client address is not authorized and caller is not a trusted service. Client address: 123.456.789.123.

To resolve this, you need to update the KeyVault Firewall to grant your IP access. 

### Logging

To help troubleshoot Terraform, you can turn on [Terraform logging](https://developer.hashicorp.com/terraform/tutorials/configuration-language/troubleshooting-workflow#enable-terraform-logging) which can be directed to a file.

To turn on logging create the following environment variable with either DEBUG or TRACE

`export TF_LOG=DEBUG`

To direct the output to a file, set the following environment variable. Terraform appends to the log file so consider deleting it before running terraform. 

`export TF_LOG_PATH=terraform.log`

### Output variables

Another useful tip for debugging Terraform is to utilise output variables, refer to the [Output Data from Terraform](https://developer.hashicorp.com/terraform/tutorials/configuration-language/outputs) article.  

The values of output variables are included within the log file so they can be reviewed.  

⚠️ Avoid committing any updates to the Terraform configuration files that include output variables for debugging purposes ⚠️