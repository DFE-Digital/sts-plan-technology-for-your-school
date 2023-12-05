resource "azurerm_storage_account" "function_storage" {
  name                     = replace("${local.resource_prefix}funcstr", "-", "")
  resource_group_name      = local.resource_group_name
  location                 = local.azure_location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  tags                     = local.tags

  public_network_access_enabled = true
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

  service_plan_id = azurerm_service_plan.function_plan.id

  storage_account_access_key = azurerm_storage_account.function_storage.primary_access_key
  storage_account_name       = azurerm_storage_account.function_storage.name

  site_config {
    api_management_api_id = azurerm_api_management_api.contentful_api.id
    application_stack {
      dotnet_version              = "7.0"
      use_dotnet_isolated_runtime = true
    }
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.user_assigned_identity.id]
  }

  app_settings = {
    AZURE_SQL_CONNECTIONSTRING  = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.vault.name};SecretName=${azurerm_key_vault_secret.vault_secret_database_connectionstring.name})"
    AzureWebJobsServiceBus      = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.vault.name};SecretName=${azurerm_key_vault_secret.vault_secret_servicebus_connectionstring.name})"
    WEBSITE_ENABLE_SYNC_UPDATE_SITE = true
    WEBSITE_MOUNT_ENABLED = 1
    AZURE_CLIENT_ID = azurerm_user_assigned_identity.user_assigned_identity.client_id
    AZURE_KEYVAULT_CLIENTID = azurerm_user_assigned_identity.user_assigned_identity.client_id
    AZURE_KEYVAULT_RESOURCEENDPOINT = "https://${local.resource_prefix}-kv.vault.azure.net/"
    AZURE_KEYVAULT_SCOPE = "https://vault.azure.net/.default"
    KeyVaultReferenceIdentity = azurerm_user_assigned_identity.user_assigned_identity.principal_id
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
