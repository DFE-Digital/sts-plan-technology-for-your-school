resource "azurerm_key_vault" "backup" {
  tenant_id    = data.azurerm_client_config.current.tenant_id

  location                        = var.azure_location
  name                            = local.keyvaultname
  rbac_authorization_enabled      = true
  resource_group_name             = var.backup_resource_group_name
  sku_name                        = "standard"
  soft_delete_retention_days      = var.blob_delete_retention_days
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
  object_id    = local.current_user_id

  secret_permissions = ["List", "Get", "Set"]
  key_permissions    = ["List", "Get", "Create", "GetRotationPolicy", "SetRotationPolicy", "Delete", "Purge", "UnwrapKey", "WrapKey"]
}

resource "azurerm_key_vault_access_policy" "vault_access_policy_mi" {
  key_vault_id = azurerm_key_vault.backup.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_user_assigned_identity.user_assigned_identity.principal_id

  secret_permissions = ["List", "Get"]
  key_permissions    = ["List", "Get", "WrapKey", "UnwrapKey"]
}
