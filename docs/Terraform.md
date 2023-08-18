# Terraform

All of our infrastructure is managed as IaC via Terraform.

We use two external modules to create the majority of the resources required:
- [terraform-azurerm-container-apps-hosting](https://github.com/DFE-Digital/terraform-azurerm-container-apps-hosting)
- [terraform-azurerm-front-door-app-gateway-waf](github.com/dfe-digital/terraform-azurerm-front-door-app-gateway-waf)

## Detailed Overview

| File                             | Information                                                                                                    |
| -------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| backend.tf                       |                                                                                                                |
| client_config.tf                 | Retrieves data for currently executing user                                                                    |
| container_app-assign-identity.tf | Runs [Bash script]() that assigns the created identity from [HERE]() to the container app                      |
| key-vault.tf                     | Creates an Azure KeyVault, any necessary keys, and dummy values for all of our secrets                         |
| keyvault-add-vnet-restriction.tf | Runs [Bash script]() that allows the Container App to access the KV, and adds a service endpoint to the subnet |
| locals.tf                        | Terraform locals                                                                                               |
| main-hosting.tf                  | Main script, using terraform-azurerm-container-apps-hosting, that creates the majority of our infrastructure   |
| providers.tf                     | Manages the provider for our own code                                                                          |
| user-assigned-identity.tf        | Creates an Azure Identity to be assigned to the Container App                                                  |
| variables.tf                     | Terraform inputs                                                                                               |
| versions.tf                      | Minimum versions of Terraform modules etc.                                                                     |
| waf.tf                           | Uses terraform-azurerm-front-door-app-gateway-waf to setup Azure Front Door CDN + WAF policies                 |
