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

  sas_policy {
    expiration_period = local.storage_account_expiration_period
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.user_assigned_identity.id]
  }

  network_rules {
    bypass         = ["Logging", "Metrics"]
    default_action = "Deny"
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

      keyVaultReferenceIdentity = azurerm_user_assigned_identity.user_assigned_identity.id,

      virtualNetworkSubnetId  = azurerm_subnet.function_infra_subnet.id,
      vnetContentShareEnabled = true,
      vnetImagePullEnabled    = true,
      vnetRouteAllEnabled     = true,

      functionAppConfig = {
        deployment = {
          storage = {
            type  = "blobContainer",
            value = "${azurerm_storage_account.function_storage.primary_blob_endpoint}function-releases"
            authentication = {
              type                               = "StorageAccountConnectionString"
              storageAccountConnectionStringName = "AzureWebJobsStorage"
            }
          }
        },
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
            name  = "APPLICATIONINSIGHTS_CONNECTION_STRING",
            value = azurerm_application_insights.functional_insights.connection_string
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
          /* Key Vault */
          {
            name  = "Azure_KeyVault_Name",
            value = azurerm_key_vault.vault.name
          },
          {
            name  = "AZURE_CLIENT_ID",
            value = azurerm_user_assigned_identity.user_assigned_identity.client_id
          }
        ],
        http20enabled = true
        minTlsVersion = "1.3"
      }
    }
  })

  lifecycle {
    ignore_changes = [
      tags,
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
