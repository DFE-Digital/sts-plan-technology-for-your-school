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
  location            = local.function.location
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
  location                  = local.function.location
  name                      = local.function.name
  parent_id                 = data.azurerm_resource_group.resource_group.id

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.user_assigned_identity.id]
  }

  tags = local.tags

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
              type                               = "StorageAccountConnectionString"
              storageAccountConnectionStringName = "AzureWebJobsStorage"
              userAssignedIdentityResourceId     = azurerm_user_assigned_identity.user_assigned_identity.id
            }
          }
        },
        /* Make these variables */
        scaleAndConcurrency = {
          maximumInstanceCount = local.function.scaling.max_instance_count,
          instanceMemoryMB     = local.function.scaling.memory
        },
        runtime = {
          name    = local.function.runtime.name,
          version = local.function.runtime.version
        }
      },

      siteConfig = {
        appSettings = [
          /* Connections */
          {
            name  = "keyVaultReferenceIdentity",
            value = azurerm_user_assigned_identity.user_assigned_identity.id
          },
          {
            name  = "APPLICATIONINSIGHTS_CONNECTION_STRING",
            value = azurerm_application_insights.functional_insights.connection_string
          },
          {
            name  = "AZURE_SQL_CONNECTIONSTRING",
            value = local.function.app_settings.sql_connection_string
          },
          {
            name  = "AzureWebJobsServiceBus",
            value = azurerm_servicebus_namespace.service_bus.default_primary_connection_string
          },
          {
            name  = "AzureWebJobsStorage"
            value = azurerm_storage_account.function_storage.primary_blob_connection_string
          },
          {
            name  = "AzureWebJobsStorage__accountName",
            value = azurerm_storage_account.function_storage.name
          },
          /* Cache clearing */
          {
            name  = "WEBSITE_CACHE_CLEAR_APIKEY_NAME"
            value = local.function.app_settings.cacheclear.apikey_name
          },
          {
            name  = "WEBSITE_CACHE_CLEAR_APIKEY_VALUE"
            value = local.function.app_settings.cacheclear.apikey_value
          },
          {
            name  = "WEBSITE_CACHE_CLEAR_ENDPOINT"
            value = local.function.app_settings.cacheclear.endpoint
          }
        ]
      }
    }
  })
  depends_on = [azurerm_service_plan.function_plan, azurerm_user_assigned_identity.user_assigned_identity, azurerm_servicebus_namespace.service_bus, azurerm_storage_account.function_storage]

  lifecycle {
    ignore_changes = [
      tags
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
