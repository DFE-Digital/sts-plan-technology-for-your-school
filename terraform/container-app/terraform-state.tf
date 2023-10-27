resource "azurerm_resource_group" "tfgroup" {
    name = local.tf_rg_name
    location = locals.azure_location
    tags = local.tags
}

resource "azurerm_storage_account" "tfstatesa" {
    name                        = local.tf_storage_account_name
    resource_group_name         = local.tf_rg_name
    location                    = local.azure_location
    account_tier                = "Standard"
    account_replication_type    = "LRS"
    service                     = "blob"
    tags                        = local.tags
}

resource "azurerm_storage_container" "tfstatecontainer" {
    name                        = "tfstate"
    storage_account_name        = local.tf_storage_account_name
}

resource "azurerm_key_vault" "tfvault" {
  name                       = local.tf_kv_name
  location                   = local.azure_location
  resource_group_name        = local.tf_rg_name
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = "standard"
  soft_delete_retention_days = 90
  enable_rbac_authorization  = false
  tags                       = local.tags
  purge_protection_enabled   = true

  network_acls {
    bypass                     = "None"
    default_action             = "Deny"
    virtual_network_subnet_ids = [module.main_hosting.networking.subnet_id]
  }

  lifecycle {
    ignore_changes = [network_acls[0].ip_rules]
  }
}

resource "azurerm_key_vault_secret" "storage_account_key" {
    name            = "storage-account-key"
    value           = data.azurerm_storage_account.tfstatesa.primary_access_key
    key_vault_id    = azurerm_key_vault.tfvault.id
}