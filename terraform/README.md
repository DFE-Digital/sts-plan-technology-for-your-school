# Terraform

The following article discusses developer tools and how to run the **Terraform** and **Teraform-Docs** tools. [Terraform Configuration](./terraform-configuration.md) page contains a detailed view of the Terraform configuration.

## Contents

- [Terraform](#terraform)
  - [Contents](#contents)
  - [Developer tools](#developer-tools)
  - [Running Terraform locally](#running-terraform-locally)
    - [Setting environment variables](#setting-environment-variables)
    - [Terraform Init](#terraform-init)
    - [Terraform Plan](#terraform-plan)
  - [Terraform Documentation](#terraform-documentation)

## Developer tools

The following tools are recommended/required to work with Terraform locally

* Visual Studio Code
  * VSCode Terraform Extension
* Install Terraform
  * MacOS: 
    * Xcode: `xcode-select --install` 
    * Terraform: `brew tap hashicorp/tap`  
* Install Terraform-Docs
  * MacOS:
    * `brew install terraform-docs`
* Optionally Install Azure CLI 
  * MacOS
    * `brew update && brew install azure-cli`

## Running Terraform locally

This section discusses how to set-up and run Terraform locally on a development machine.

⚠️ do not update the example Terraform configuration files with sensitive information and commit to repo ⚠️ 

### Setting environment variables
Within a terminal window window run the following commands to set some environment variables that Terraform requires to connect to Azure.

MacOS:
```
export ARM_TENANT_ID = <tenant_id> 
export ARM_SUBSCRIPTION_ID = <subscription_id> 
export ARM_CLIENT_ID = <client_id>
export ARM_CLIENT_SECRET = <client_secret>
```

### Terraform Init 
Terraform needs to be initialised on your local machine before you can use it. To do this rename the `init.tfvars.example` file to `init.tfvars` and complete the configuration so that Terraform connects to the correct Azure Storage instance.  

Run the following command to initialise Terraform.

`terraform init -backend-config=backend.tfvars`

⚠️ tfvars files are ignored by git, but do ensure they do not get committed to the repo ⚠️ 

### Terraform Plan
To run the plan command, first rename the `terraform.tfvars.example` file to `terraform.tfvars` and complete the following configuration.  

| Title          | description            |
| -------------- | ---------------------- |
| az_environment | The environment prefix |
| az_location    | The Azure location     |

Run the following command to execute the Plan commande: 

`terraform plan -var-file="terraform.tfvars"`

## Terraform Documentation

When the Terraform configuration is updated, the module documentation will need to be re-generated, to do this run the following command within a terminal window:

`terraform-docs markdown .` 

This will generate the `terraform-documentation.md` markdown file.