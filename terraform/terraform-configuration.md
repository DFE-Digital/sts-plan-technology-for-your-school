# Terraform Module Configuration

<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 1.4.5 |
| <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) | >= 3.52.0 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_azurerm"></a> [azurerm](#provider\_azurerm) | 3.56.0 |

## Modules

| Name | Source | Version |
|------|--------|---------|
| <a name="module_main_hosting"></a> [main\_hosting](#module\_main\_hosting) | github.com/DFE-Digital/terraform-azurerm-container-apps-hosting | v0.17.5 |

## Resources

| Name | Type |
|------|------|
| [azurerm_key_vault.vault](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/key_vault) | resource |
| [azurerm_client_config.current](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/client_config) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_az_tag_environment"></a> [az\_tag\_environment](#input\_az\_tag\_environment) | n/a | `string` | n/a | yes |
| <a name="input_az_tag_product"></a> [az\_tag\_product](#input\_az\_tag\_product) | n/a | `string` | n/a | yes |
| <a name="input_azure_location"></a> [azure\_location](#input\_azure\_location) | n/a | `string` | n/a | yes |
| <a name="input_cdn_frontdoor_origin_host_header_override"></a> [cdn\_frontdoor\_origin\_host\_header\_override](#input\_cdn\_frontdoor\_origin\_host\_header\_override) | Override the frontdoor origin host header | `string` | n/a | yes |
| <a name="input_environment"></a> [environment](#input\_environment) | Environment name. Will be used along with `project_name` as a prefix for all resources. | `string` | n/a | yes |
| <a name="input_msi_id"></a> [msi\_id](#input\_msi\_id) | The Managed Service Identity ID. If this value isn't null (the default), 'data.azurerm\_client\_config.current.object\_id' will be set to this value. | `string` | `null` | no |
| <a name="input_project_name"></a> [project\_name](#input\_project\_name) | n/a | `string` | n/a | yes |

## Outputs

No outputs.
<!-- END_TF_DOCS -->