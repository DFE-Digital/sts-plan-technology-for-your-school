# Terraform Module Configuration

All of our infrastructure is managed as IaC via Terraform.

We use two external modules to create the majority of the resources required:
- [terraform-azurerm-container-apps-hosting](https://github.com/DFE-Digital/terraform-azurerm-container-apps-hosting)
- [terraform-azurerm-front-door-app-gateway-waf](https://github.com/dfe-digital/terraform-azurerm-front-door-app-gateway-waf)

## Detailed Overview

| File                             | Information                                                                                                                                                     |
| -------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| backend.tf                       |                                                                                                                                                                 |
| client_config.tf                 | Retrieves data for currently executing user                                                                                                                     |
| container_app-assign-identity.tf | Runs [Bash script](../terraform/scripts/assign-user-identity-to-app.sh) that assigns the created identity from user-assigned-identity.tf to the Container App   |
| key-vault.tf                     | Creates an Azure KeyVault, any necessary keys, and dummy values for all of our secrets                                                                          |
| keyvault-add-vnet-restriction.tf | Runs [Bash script](../terraform/scripts/add-keyvault-service-endpoint-to-app.sh) Adds service endpoint to the subnet, and allows access through the same subnet |
| locals.tf                        | Terraform locals                                                                                                                                                |
| main-hosting.tf                  | Main script, using terraform-azurerm-container-apps-hosting, that creates the majority of our infrastructure                                                    |
| providers.tf                     | Manages the provider for our own code                                                                                                                           |
| user-assigned-identity.tf        | Creates an Azure Identity to be assigned to the Container App                                                                                                   |
| variables.tf                     | Terraform inputs                                                                                                                                                |
| versions.tf                      | Minimum versions of Terraform modules etc.                                                                                                                      |
| waf.tf                           | Uses terraform-azurerm-front-door-app-gateway-waf to setup Azure Front Door CDN + WAF policies                                                                  |

<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 1.5.0 |
| <a name="requirement_azapi"></a> [azapi](#requirement\_azapi) | = 1.15.0 |
| <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) | = 4.4.0 |
| <a name="requirement_null"></a> [null](#requirement\_null) | = 3.2.3 |
| <a name="requirement_random"></a> [random](#requirement\_random) | = 3.6.3 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_azurerm"></a> [azurerm](#provider\_azurerm) | 4.4.0 |
| <a name="provider_null"></a> [null](#provider\_null) | 3.2.3 |
| <a name="provider_random"></a> [random](#provider\_random) | 3.6.3 |

## Modules

| Name | Source | Version |
|------|--------|---------|
| <a name="module_main_hosting"></a> [main\_hosting](#module\_main\_hosting) | github.com/DFE-Digital/terraform-azurerm-container-apps-hosting | 93096d1 |
| <a name="module_waf"></a> [waf](#module\_waf) | github.com/dfe-digital/terraform-azurerm-front-door-app-gateway-waf | f0ca7eb |

## Resources

| Name | Type |
|------|------|
| [azurerm_key_vault.vault](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/key_vault) | resource |
| [azurerm_key_vault_access_policy.vault_access_policy_mi](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/key_vault_access_policy) | resource |
| [azurerm_key_vault_access_policy.vault_access_policy_tf](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/key_vault_access_policy) | resource |
| [azurerm_key_vault_key.data_protection_key](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/key_vault_key) | resource |
| [azurerm_key_vault_secret.api_key](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/key_vault_secret) | resource |
| [azurerm_key_vault_secret.csp_connect_src](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/key_vault_secret) | resource |
| [azurerm_key_vault_secret.csp_default_src](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/key_vault_secret) | resource |
| [azurerm_key_vault_secret.csp_frame_src](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/key_vault_secret) | resource |
| [azurerm_key_vault_secret.csp_img_src](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/key_vault_secret) | resource |
| [azurerm_key_vault_secret.vault_secret_contentful_deliveryapikey](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/key_vault_secret) | resource |
| [azurerm_key_vault_secret.vault_secret_contentful_environment](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/key_vault_secret) | resource |
| [azurerm_key_vault_secret.vault_secret_contentful_previewapikey](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/key_vault_secret) | resource |
| [azurerm_key_vault_secret.vault_secret_contentful_spaceid](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/key_vault_secret) | resource |
| [azurerm_key_vault_secret.vault_secret_database_connectionstring](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/key_vault_secret) | resource |
| [azurerm_private_dns_zone.database](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/private_dns_zone) | resource |
| [azurerm_private_dns_zone.keyvault](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/private_dns_zone) | resource |
| [azurerm_private_dns_zone.redis](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/private_dns_zone) | resource |
| [azurerm_private_dns_zone_virtual_network_link.database_default](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/private_dns_zone_virtual_network_link) | resource |
| [azurerm_private_dns_zone_virtual_network_link.keyvault_to_defaultvnet](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/private_dns_zone_virtual_network_link) | resource |
| [azurerm_private_dns_zone_virtual_network_link.redis_default](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/private_dns_zone_virtual_network_link) | resource |
| [azurerm_private_endpoint.database](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/private_endpoint) | resource |
| [azurerm_private_endpoint.keyvault](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/private_endpoint) | resource |
| [azurerm_private_endpoint.redis](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/private_endpoint) | resource |
| [azurerm_redis_cache.redis](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/redis_cache) | resource |
| [azurerm_servicebus_namespace.service_bus](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/servicebus_namespace) | resource |
| [azurerm_servicebus_queue.contentful_queue](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/servicebus_queue) | resource |
| [azurerm_servicebus_queue_authorization_rule.azurefunction](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/servicebus_queue_authorization_rule) | resource |
| [azurerm_storage_account.costing_storage](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/storage_account) | resource |
| [azurerm_storage_container.blobforcost](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/storage_container) | resource |
| [azurerm_subnet.keyvault](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/subnet) | resource |
| [azurerm_subnet_route_table_association.keyvault](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/subnet_route_table_association) | resource |
| [azurerm_user_assigned_identity.user_assigned_identity](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/resources/user_assigned_identity) | resource |
| [null_resource.upsert_contentful_webhook](https://registry.terraform.io/providers/hashicorp/null/3.2.3/docs/resources/resource) | resource |
| [random_password.api_key_value](https://registry.terraform.io/providers/hashicorp/random/3.6.3/docs/resources/password) | resource |
| [azurerm_cdn_frontdoor_endpoint.app](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/data-sources/cdn_frontdoor_endpoint) | data source |
| [azurerm_client_config.current](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/data-sources/client_config) | data source |
| [azurerm_mssql_server.database](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/data-sources/mssql_server) | data source |
| [azurerm_route_table.default](https://registry.terraform.io/providers/hashicorp/azurerm/4.4.0/docs/data-sources/route_table) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_az_app_kestrel_endpoint"></a> [az\_app\_kestrel\_endpoint](#input\_az\_app\_kestrel\_endpoint) | Endpoint for Kestrel setup | `string` | n/a | yes |
| <a name="input_az_container_port"></a> [az\_container\_port](#input\_az\_container\_port) | What port the container app is bound to | `number` | `8080` | no |
| <a name="input_az_sql_admin_password"></a> [az\_sql\_admin\_password](#input\_az\_sql\_admin\_password) | Password for the admin listed in the 'az\_sql\_azuread\_admin\_username' variable | `string` | n/a | yes |
| <a name="input_az_sql_azuread_admin_objectid"></a> [az\_sql\_azuread\_admin\_objectid](#input\_az\_sql\_azuread\_admin\_objectid) | Object ID for the admin listed in the 'az\_sql\_azuread\_admin\_username' variable | `string` | n/a | yes |
| <a name="input_az_sql_azuread_admin_username"></a> [az\_sql\_azuread\_admin\_username](#input\_az\_sql\_azuread\_admin\_username) | Username/email/service principal name/etc of the Azure AD account to use as admin for the SQL Server | `string` | n/a | yes |
| <a name="input_az_sql_max_pool_size"></a> [az\_sql\_max\_pool\_size](#input\_az\_sql\_max\_pool\_size) | Maximum number of possible connections per SQL connection string usage | `number` | `100` | no |
| <a name="input_az_sql_sku"></a> [az\_sql\_sku](#input\_az\_sql\_sku) | What SKU/plan to use for the SQL DB | `string` | `"Basic"` | no |
| <a name="input_az_tag_environment"></a> [az\_tag\_environment](#input\_az\_tag\_environment) | Environment tag to be applied to all resources | `string` | n/a | yes |
| <a name="input_az_tag_product"></a> [az\_tag\_product](#input\_az\_tag\_product) | Product tag to be applied to all resources | `string` | n/a | yes |
| <a name="input_azure_location"></a> [azure\_location](#input\_azure\_location) | Recourse location | `string` | n/a | yes |
| <a name="input_cdn_create_custom_domain"></a> [cdn\_create\_custom\_domain](#input\_cdn\_create\_custom\_domain) | A flag to create the A and TXT records for the container app as part of setting up the cdn | `bool` | `false` | no |
| <a name="input_cdn_frontdoor_host_add_response_headers"></a> [cdn\_frontdoor\_host\_add\_response\_headers](#input\_cdn\_frontdoor\_host\_add\_response\_headers) | List of response headers to add at the CDN Front Door `[{ "Name" = "Strict-Transport-Security", "value" = "max-age=31536000" }]` | `list(map(string))` | `[]` | no |
| <a name="input_cdn_frontdoor_url_path_redirects"></a> [cdn\_frontdoor\_url\_path\_redirects](#input\_cdn\_frontdoor\_url\_path\_redirects) | List of url path redirects to add at the CDN Front Door `[{ "redirect_type": "PermanentRedirect", "destination_path": "/example", "destination_hostname": "www.example.uk", "operator": "Equals", "match_values": ["/example"] }]` | <pre>list(object({<br>    redirect_type        = string<br>    redirect_protocol    = optional(string)<br>    destination_path     = optional(string)<br>    destination_hostname = optional(string)<br>    destination_fragment = optional(string)<br>    query_string         = optional(string)<br>    operator             = string<br>    match_values         = optional(list(string))<br>    transforms           = optional(list(string))<br>  }))</pre> | `[]` | no |
| <a name="input_container_app_blob_storage_public_access_enabled"></a> [container\_app\_blob\_storage\_public\_access\_enabled](#input\_container\_app\_blob\_storage\_public\_access\_enabled) | Enable app blob storage public access | `bool` | `false` | no |
| <a name="input_container_app_http_concurrency"></a> [container\_app\_http\_concurrency](#input\_container\_app\_http\_concurrency) | Scale up at this number of HTTP requests | `number` | `10` | no |
| <a name="input_container_app_max_replicas"></a> [container\_app\_max\_replicas](#input\_container\_app\_max\_replicas) | Maximum replicas for the container app | `number` | `2` | no |
| <a name="input_container_app_min_replicas"></a> [container\_app\_min\_replicas](#input\_container\_app\_min\_replicas) | Minimum replicas for the container app | `number` | `1` | no |
| <a name="input_container_app_storage_account_shared_access_key_enabled"></a> [container\_app\_storage\_account\_shared\_access\_key\_enabled](#input\_container\_app\_storage\_account\_shared\_access\_key\_enabled) | Enable shared access key | `bool` | `false` | no |
| <a name="input_container_environment"></a> [container\_environment](#input\_container\_environment) | Container app environment - set to ASPNETCORE\_ENVIRONMENT in the app environment variables | `string` | `""` | no |
| <a name="input_container_environment_variables"></a> [container\_environment\_variables](#input\_container\_environment\_variables) | Additional environment variables to set on the Azure Container App | `map(string)` | `{}` | no |
| <a name="input_contentful_management_token"></a> [contentful\_management\_token](#input\_contentful\_management\_token) | Contentful management token | `string` | `null` | no |
| <a name="input_contentful_upsert_webhook"></a> [contentful\_upsert\_webhook](#input\_contentful\_upsert\_webhook) | Whether to create/update the webhook or not | `bool` | `false` | no |
| <a name="input_contentful_webhook_endpoint"></a> [contentful\_webhook\_endpoint](#input\_contentful\_webhook\_endpoint) | Endpoint for Contentful webhook | `string` | `"/api/cms/webhook"` | no |
| <a name="input_contentful_webhook_name"></a> [contentful\_webhook\_name](#input\_contentful\_webhook\_name) | Prefix for the Contentful webhook name | `string` | `"Plan Tech Webhook"` | no |
| <a name="input_environment"></a> [environment](#input\_environment) | Environment name, used along with `project_name` as a prefix for all resources | `string` | n/a | yes |
| <a name="input_image_tag"></a> [image\_tag](#input\_image\_tag) | Image tag | `string` | n/a | yes |
| <a name="input_key_ops"></a> [key\_ops](#input\_key\_ops) | The permitted JSON web key operations of the key to be created. | `list(string)` | <pre>[<br>  "decrypt",<br>  "encrypt",<br>  "sign",<br>  "unwrapKey",<br>  "verify",<br>  "wrapKey"<br>]</pre> | no |
| <a name="input_key_size"></a> [key\_size](#input\_key\_size) | The size in bits of the key to be created. | `number` | `2048` | no |
| <a name="input_key_type"></a> [key\_type](#input\_key\_type) | The JsonWebKeyType of the key to be created. | `string` | `"RSA"` | no |
| <a name="input_msi_id"></a> [msi\_id](#input\_msi\_id) | The Managed Service Identity ID. If this value isn't null (the default), 'data.azurerm\_client\_config.current.object\_id' will be set to this value. | `string` | `null` | no |
| <a name="input_project_name"></a> [project\_name](#input\_project\_name) | project name, used along with `environment` as a prefix for all resources | `string` | n/a | yes |
| <a name="input_redis_capacity"></a> [redis\_capacity](#input\_redis\_capacity) | Redis cache capacity (0-6 for C and 1-5 for P) | `number` | `0` | no |
| <a name="input_redis_family"></a> [redis\_family](#input\_redis\_family) | Redis cache family (C for basic/standard, P for premium) | `string` | `"C"` | no |
| <a name="input_redis_sku_name"></a> [redis\_sku\_name](#input\_redis\_sku\_name) | SKU for Redis cache (Basic, Standard or Premium) | `string` | `"Standard"` | no |
| <a name="input_redis_tls_version"></a> [redis\_tls\_version](#input\_redis\_tls\_version) | Minimum TLS version for Redis | `string` | `"1.2"` | no |
| <a name="input_registry_password"></a> [registry\_password](#input\_registry\_password) | Container registry password | `string` | n/a | yes |
| <a name="input_registry_server"></a> [registry\_server](#input\_registry\_server) | Container registry server | `string` | n/a | yes |
| <a name="input_registry_username"></a> [registry\_username](#input\_registry\_username) | Container registry username | `string` | n/a | yes |
| <a name="input_storage_account_expiration_period"></a> [storage\_account\_expiration\_period](#input\_storage\_account\_expiration\_period) | The SAS expiration period in format of DD.HH:MM:SS | `string` | `"00.01:00:00"` | no |
| <a name="input_storage_account_public_access_enabled"></a> [storage\_account\_public\_access\_enabled](#input\_storage\_account\_public\_access\_enabled) | Enable public network access | `bool` | `false` | no |

## Outputs

No outputs.
<!-- END_TF_DOCS -->