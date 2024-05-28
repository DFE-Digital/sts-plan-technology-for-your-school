resource "azurerm_storage_account" "function_app_storage" {
  name                     = replace("${local.resource_prefix}funcstr", "-", "")
  resource_group_name      = local.resource_group_name
  location                 = local.azure_location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  min_tls_version          = "TLS1_2"

  tags = local.tags

  public_network_access_enabled   = true
  shared_access_key_enabled       = false
  allow_nested_items_to_be_public = false
}

resource "azurerm_storage_container" "function_app_str_webjob_host" {
  name                 = "azure-webjobs-hosts"
  storage_account_name = azurerm_storage_account.function_app_storage.name
}

resource "azurerm_storage_container" "function_app_storage_container-" {
  name                 = "function-releases"
  storage_account_name = azurerm_storage_account.function_app_storage.name
}
resource "azurerm_storage_container" "function_app_str_webjob_secrets" {
  name                 = "azure-webjobs-secrets"
  storage_account_name = azurerm_storage_account.function_app_storage.name
}
resource "azurerm_service_plan" "function_app_plan" {
  location            = "northeurope"
  name                = "${local.resource_prefix}appserviceplan"
  os_type             = "Linux"
  resource_group_name = local.resource_group_name
  sku_name            = "Y1"

  tags = local.tags

  lifecycle {
    ignore_changes = [
      sku_name
    ]
  }
}
resource "azurerm_linux_function_app" "function_app" {
  builtin_logging_enabled     = false
  client_certificate_mode     = "Required"
  functions_extension_version = "~1"
  location                    = "northeurope"
  name                        = "${local.resource_prefix}contentfulfunction"

  resource_group_name  = local.resource_group_name
  service_plan_id      = azurerm_service_plan.function_app_plan.id
  storage_account_name = azurerm_storage_account.function_app_storage.name
  tags                 = local.tags

  key_vault_reference_identity_id = azurerm_user_assigned_identity.user_assigned_identity.id

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

  site_config {
    ftps_state               = "FtpsOnly"
    http2_enabled            = true
    application_insights_key = azurerm_application_insights.functional_insights.instrumentation_key
    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }
  }

  depends_on = [
    azurerm_service_plan.function_app_plan,
  ]

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITE_RUN_FROM_PACKAGE"],
      app_settings["AzureWebJobsStorage__clientId"],
      app_settings["AzureWebJobsStorage__credential"],
      app_settings["WEBSITE_RUN_FROM_PACKAGE_BLOB_MI_RESOURCE_ID"],
    ]
  }

}

resource "azurerm_app_service_custom_hostname_binding" "function_app_host" {
  app_service_name    = azurerm_service_plan.name
  hostname            = "${azurerm_linux_function_app.function_app.name}.azurewebsites.net"
  resource_group_name = local.resource_group_name

  depends_on = [
    azurerm_linux_function_app.function_app,
  ]
}

data "azurerm_subscription" "subscription" {
}


data "azurerm_function_app_host_keys" "default" {
  name                = azurerm_linux_function_app.function_app.name
  resource_group_name = loca.resource_group_name

  depends_on = [
    azurerm_linux_function_app.function_app
  ]
}

resource "azurerm_application_insights" "functional_insights" {
  name                = "${local.resource_prefix}-function-insights"
  location            = local.environment
  resource_group_name = local.resource_group_name
  application_type    = "web"
  retention_in_days   = 30
  tags                = local.tags
}

resource "azurerm_app_service_connection" "azurekeyvaultconnector" {
  name               = "azurekeyvaultconnection"
  app_service_id     = azurerm_linux_function_app.function_app.id
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
  app_service_id     = azurerm_linux_function_app.function_app.id
  target_resource_id = "/subscriptions/${data.azurerm_subscription.subscription.subscription_id}/resourceGroups/${local.resource_prefix}/providers/Microsoft.Sql/servers/${local.resource_prefix}/databases/${local.resource_prefix}-sqldb"
  client_type        = "dotnet"

  authentication {
    type            = "userAssignedIdentity"
    client_id       = azurerm_user_assigned_identity.user_assigned_identity.client_id
    subscription_id = data.azurerm_subscription.subscription.subscription_id
  }
}
