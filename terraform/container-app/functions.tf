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
  location            = "northeurope"
  os_type             = "Linux"
  sku_name            = "FC1"
  tags                = local.tags
}

data "azurerm_resource_group" "resource_group" {
  name = local.resource_group_name
}

resource "azapi_resource" "contentful_function" {
  type                      = "Microsoft.Web/sites@2023-12-01"
  schema_validation_enabled = false
  location                  = "northeurope"
  name                      = "${local.resource_prefix}contentfulfunction"
  parent_id                 = data.azurerm_resource_group.resource_group.id

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.user_assigned_identity.id]
  }

  body = jsonencode({
    kind = "functionapp,linux",

    properties = {
      serverFarmId = azurerm_service_plan.function_plan.id,

      functionAppConfig = {
        deployment = {
          storage = {
            type  = "blobContainer",
            value = "${azurerm_storage_account.function_storage.primary_blob_endpoint}function-releases"
            authentication = {
              type = "SystemAssignedIdentity"
            }
          }
        },
        /* Make these variables */
        scaleAndConcurrency = {
          maximumInstanceCount = 40,
          instanceMemoryMB     = 2048
        },
        runtime = {
          name    = "dotnet-isolated",
          version = "8.0"
        }
      },

      siteConfig = {
        appSettings = [
          {
            name  = "AzureWebJobsStorage__accountName",
            value = azurerm_storage_account.function_storage.name
          },
          {
            name  = "APPLICATIONINSIGHTS_CONNECTION_STRING",
            value = azurerm_application_insights.functional_insights.connection_string
          },
          {
            name  = "AZURE_SQL_CONNECTIONSTRING",
            value = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.vault.name};SecretName=${azurerm_key_vault_secret.vault_secret_database_connectionstring.name})"
          },
          {
            name  = "AzureWebJobsServiceBus",
            value = azurerm_servicebus_namespace.service_bus.default_primary_connection_string
          },
          {
            name  = "WEBSITE_ENABLE_SYNC_UPDATE_SITE",
            value = true
          },
          {
            name  = "WEBSITE_MOUNT_ENABLED",
            value = 1
          },
          {
            name  = "AZURE_CLIENT_ID",
            value = azurerm_user_assigned_identity.user_assigned_identity.client_id
          },
          {
            name  = "AZURE_KEYVAULT_CLIENTID",
            value = azurerm_user_assigned_identity.user_assigned_identity.client_id
          },
          {
            name  = "AZURE_KEYVAULT_RESOURCEENDPOINT",
            value = azurerm_key_vault.vault.vault_uri
          },
          {
            name  = "AZURE_KEYVAULT_SCOPE",
            value = "https://vault.azure.net/.default"
          },
          {
            name  = "KeyVaultReferenceIdentity",
            value = azurerm_user_assigned_identity.user_assigned_identity.id
          },
          {
            name  = "WEBSITE_RUN_FROM_PACKAGE",
            value = ""
          }
        ]
      }
    }
  })
  depends_on = [azurerm_service_plan.function_plan, azurerm_user_assigned_identity.user_assigned_identity, azurerm_servicebus_namespace.service_bus, azurerm_storage_account.function_storage]
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
  app_service_id     = azapi_resource.contentful_function.id
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
  app_service_id     = azapi_resource.contentful_function.id
  target_resource_id = "/subscriptions/${data.azurerm_subscription.subscription.subscription_id}/resourceGroups/${local.resource_prefix}/providers/Microsoft.Sql/servers/${local.resource_prefix}/databases/${local.resource_prefix}-sqldb"
  client_type        = "dotnet"
  authentication {
    type            = "userAssignedIdentity"
    client_id       = azurerm_user_assigned_identity.user_assigned_identity.client_id
    subscription_id = data.azurerm_subscription.subscription.subscription_id
  }
}
