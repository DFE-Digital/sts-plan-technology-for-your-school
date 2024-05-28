resource "azurerm_key_vault" "vault" {
  name                       = local.kv_name
  location                   = local.azure_location
  resource_group_name        = module.main_hosting.azurerm_resource_group_default.name
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

resource "azurerm_key_vault_access_policy" "vault_access_policy_tf" {
  key_vault_id = azurerm_key_vault.vault.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = local.current_user_id

  secret_permissions = ["List", "Get", "Set"]
  key_permissions    = ["List", "Get", "Create", "GetRotationPolicy", "SetRotationPolicy", "Delete", "Purge", "UnwrapKey", "WrapKey"]
}

resource "azurerm_key_vault_access_policy" "vault_access_policy_mi" {
  key_vault_id = azurerm_key_vault.vault.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_user_assigned_identity.user_assigned_identity.principal_id

  secret_permissions = ["List", "Get"]
  key_permissions    = ["List", "Get", "WrapKey", "UnwrapKey"]
}

resource "azurerm_key_vault_secret" "vault_secret_contentful_deliveryapikey" {
  key_vault_id = azurerm_key_vault.vault.id
  name         = "contentful--deliveryapikey"
  value        = "temp value"

  lifecycle {
    ignore_changes = [
      value,
      expiration_date
    ]
  }
}

resource "azurerm_key_vault_secret" "vault_secret_contentful_previewapikey" {
  key_vault_id = azurerm_key_vault.vault.id
  name         = "contentful--previewapikey"
  value        = "temp value"

  lifecycle {
    ignore_changes = [
      value,
      expiration_date
    ]
  }
}

resource "azurerm_key_vault_secret" "vault_secret_contentful_spaceid" {
  key_vault_id = azurerm_key_vault.vault.id
  name         = "contentful--spaceid"
  value        = "temp value"

  lifecycle {
    ignore_changes = [
      value,
      expiration_date
    ]
  }
}

resource "azurerm_key_vault_secret" "vault_secret_contentful_environment" {
  key_vault_id = azurerm_key_vault.vault.id
  name         = "contentful--environment"
  value        = "temp value"

  lifecycle {
    ignore_changes = [
      value,
      expiration_date
    ]
  }
}

resource "azurerm_key_vault_secret" "vault_secret_database_connectionstring" {
  key_vault_id = azurerm_key_vault.vault.id
  name         = "connectionstrings--database"
  value        = local.az_sql_connection_string

  lifecycle {
    ignore_changes = [
      value,
      expiration_date
    ]
  }
}

resource "azurerm_key_vault_secret" "functionapp_possibleoutboundipaddresses" {
  key_vault_id = azurerm_key_vault.vault.id
  name         = "functionapp--possibleoutboundipaddresses"
  value        = join(",", azurerm_linux_function_app.function_app.possible_outbound_ip_address_list)


  lifecycle {
    ignore_changes = [
      value,
      expiration_date
    ]
  }
}

resource "azurerm_key_vault_secret" "functionapp_default_key" {
  key_vault_id = azurerm_key_vault.vault.id
  name         = "functionapp--accesskey"
  value        = data.azurerm_function_app_host_keys.default.default_function_key

  lifecycle {
    ignore_changes = [
      value,
      expiration_date
    ]
  }
}


resource "azurerm_key_vault_key" "data_protection_key" {
  name         = "dataprotection"
  key_vault_id = azurerm_key_vault.vault.id

  key_type = var.key_type
  key_size = var.key_size
  key_opts = var.key_ops

  tags = local.tags

  lifecycle {
    ignore_changes = all
  }
}
