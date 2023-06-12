data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "vault" {
  name                       = "${local.environment}${local.project_name}-kv"
  location                   = local.azure_location
  resource_group_name        = "${local.environment}${local.project_name}"
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = "standard"
  soft_delete_retention_days = 90
  enable_rbac_authorization  = true
}