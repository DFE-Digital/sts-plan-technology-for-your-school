resource "azurerm_key_vault" "backup" {
  tenant_id           = data.azurerm_client_config.current.tenant_id
  location            = var.azure_location
  name                = var.backup_keyvault_name
  resource_group_name = var.backup_resource_group_name
  sku_name            = "standard"

  enable_rbac_authorization  = true
  purge_protection_enabled   = true
  soft_delete_retention_days = 90

  tags = {
    Environment        = var.az_tag_environment
    Product            = var.az_tag_product
    "Service Offering" = var.az_tag_product
  }
}
###################
# Access Policies #
###################

resource "azurerm_key_vault_access_policy" "vault_access_policy_tf" {
  key_vault_id = azurerm_key_vault.backup.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  secret_permissions = ["List", "Get", "Set"]
  key_permissions    = ["List", "Get", "Create", "GetRotationPolicy", "SetRotationPolicy", "Delete", "Purge", "UnwrapKey", "WrapKey"]
}
