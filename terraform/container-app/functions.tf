resource "azurerm_storage_account" "function_storage" {
  name                     = replace("${local.resource_prefix}funcstr", "-", "")
  resource_group_name      = local.resource_group_name
  location                 = local.azure_location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  public_network_access_enabled = false
  shared_access_key_enabled     = false

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.user_assigned_identity.id]
  }
}

resource "azurerm_service_plan" "function_plan" {
  name                = "${local.resource_prefix}appserviceplan"
  resource_group_name = local.resource_group_name
  location            = local.azure_location
  os_type             = "Linux"
  sku_name            = "Y1"
}

resource "azurerm_linux_function_app" "contentful_function" {
  name                = "${local.resource_prefix}contentfulfunction"
  resource_group_name = local.resource_group_name
  location            = local.azure_location

  storage_account_name          = azurerm_storage_account.function_storage.name
  storage_uses_managed_identity = true
  service_plan_id               = azurerm_service_plan.function_plan.id

  key_vault_reference_identity_id = azurerm_user_assigned_identity.user_assigned_identity.id

  site_config {}

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.user_assigned_identity.id]
  }

  auth_settings {
    enabled = true
    active_directory {
      client_id                  = data.azurerm_client_config.current.client_id
      client_secret_setting_name = "AZURE_AD_AUTH_CLIENT_SECRET" # We use an app setting to store a key vault reference.
    }
  }

  app_settings = {
    AZURE_AD_AUTH_CLIENT_SECRET = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.vault.name};SecretName=${azurerm_key_vault_secret.client_secret.name})"
  }
}
