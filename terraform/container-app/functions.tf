resource "azurerm_storage_account" "function_storage" {
  name                     = replace("${local.resource_prefix}funcstr", "-", "")
  resource_group_name      = local.resource_group_name
  location                 = local.azure_location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  tags                     = local.tags

  public_network_access_enabled = false
  shared_access_key_enabled     = true

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
  tags                = local.tags
}

resource "azurerm_linux_function_app" "contentful_function" {
  name                = "${local.resource_prefix}contentfulfunction"
  resource_group_name = local.resource_group_name
  location            = local.azure_location
  tags                = local.tags

  storage_account_name          = azurerm_storage_account.function_storage.name
  storage_uses_managed_identity = true
  service_plan_id               = azurerm_service_plan.function_plan.id

  key_vault_reference_identity_id = azurerm_user_assigned_identity.user_assigned_identity.id

  site_config {
    application_insights_key = azurerm_application_insights.functional_insights.instrumentation_key
  }

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
    AZURE_SQL_CONNECTIONSTRING  = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.vault.name};SecretName=${azurerm_key_vault_secret.vault_secret_database_connectionstring.name})"
    AzureWebJobsServiceBus      = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.vault.name};SecretName=${azurerm_key_vault_secret.vault_secret_servicebus_connectionstring.name})"
  }
}

data "azurerm_function_app_host_keys" "default" {
  name                = azurerm_linux_function_app.contentful_function.name
  resource_group_name = local.resource_group_name
}

resource "azurerm_application_insights" "functional_insights" {
  name                = "${local.resource_prefix}-function-insights"
  location            = local.azure_location
  resource_group_name = local.resource_group_name
  application_type    = "web"
  retention_in_days   = 30
  tags                = local.tags
}
