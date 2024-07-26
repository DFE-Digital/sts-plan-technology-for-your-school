resource "azurerm_storage_account" "function_storage" {
  name                     = replace("${local.resource_prefix}funcstr", "-", "")
  resource_group_name      = local.resource_group_name
  location                 = local.azure_location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  tags                     = local.tags

  public_network_access_enabled   = local.storage_account_public_access_enabled
  shared_access_key_enabled       = local.container_app_storage_account_shared_access_key_enabled
  allow_nested_items_to_be_public = local.container_app_blob_storage_public_access_enabled

  blob_properties {
    container_delete_retention_policy {
      days = 7
    }

    delete_retention_policy {
      days = 7
    }
  }

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
  sku_name            = "FC1"
  tags                = local.tags
}

resource "azurerm_linux_function_app" "contentful_function" {
  name                = "${local.resource_prefix}contentfulfunction"
  resource_group_name = local.resource_group_name
  location            = local.azure_location
  tags                = local.tags

  service_plan_id = azurerm_service_plan.function_plan.id

  storage_account_name = azurerm_storage_account.function_storage.name

  storage_account_access_key    = local.container_app_storage_account_shared_access_key_enabled ? azurerm_storage_account.function_storage.primary_access_key : null
  storage_uses_managed_identity = local.container_app_storage_account_shared_access_key_enabled ? null : true

  key_vault_reference_identity_id = azurerm_user_assigned_identity.user_assigned_identity.id

  site_config {
    application_insights_key = azurerm_application_insights.functional_insights.instrumentation_key
    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.user_assigned_identity.id]
  }

  app_settings = {
    AZURE_SQL_CONNECTIONSTRING      = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.vault.name};SecretName=${azurerm_key_vault_secret.vault_secret_database_connectionstring.name})"
    AzureWebJobsServiceBus          = azurerm_servicebus_namespace.service_bus.default_primary_connection_string
    WEBSITE_ENABLE_SYNC_UPDATE_SITE = true
    WEBSITE_MOUNT_ENABLED           = 1
    AZURE_CLIENT_ID                 = azurerm_user_assigned_identity.user_assigned_identity.client_id
    AZURE_KEYVAULT_CLIENTID         = azurerm_user_assigned_identity.user_assigned_identity.client_id
    AZURE_KEYVAULT_RESOURCEENDPOINT = azurerm_key_vault.vault.vault_uri
    AZURE_KEYVAULT_SCOPE            = "https://vault.azure.net/.default"
    KeyVaultReferenceIdentity       = azurerm_user_assigned_identity.user_assigned_identity.id
    WEBSITE_RUN_FROM_PACKAGE        = ""
  }

  lifecycle {
    ignore_changes = [
      app_settings,
    ]
  }
}

resource "azurerm_application_insights" "functional_insights" {
  name                = "${local.resource_prefix}-function-insights"
  location            = local.azure_location
  resource_group_name = local.resource_group_name
  application_type    = "web"
  retention_in_days   = 30
  tags                = local.tags
}

data "azurerm_subscription" "subscription" {
}

resource "azurerm_app_service_connection" "azurekeyvaultconnector" {
  name               = "azurekeyvaultconnection"
  app_service_id     = azurerm_linux_function_app.contentful_function.id
  target_resource_id = azurerm_key_vault.vault.id
  client_type        = "dotnet"
  authentication {
    type            = "userAssignedIdentity"
    client_id       = azurerm_user_assigned_identity.user_assigned_identity.client_id
    subscription_id = data.azurerm_subscription.subscription.subscription_id
  }
}

resource "azurerm_app_service_connection" "azuresqlconnector" {
  name               = "azuresqlconnection"
  app_service_id     = azurerm_linux_function_app.contentful_function.id
  target_resource_id = "/subscriptions/${data.azurerm_subscription.subscription.subscription_id}/resourceGroups/${local.resource_prefix}/providers/Microsoft.Sql/servers/${local.resource_prefix}/databases/${local.resource_prefix}-sqldb"
  client_type        = "dotnet"
  authentication {
    type            = "userAssignedIdentity"
    client_id       = azurerm_user_assigned_identity.user_assigned_identity.client_id
    subscription_id = data.azurerm_subscription.subscription.subscription_id
  }
}
