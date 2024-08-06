# Create Azure Storage Account required for Cost Analysis Export storage
resource "azurerm_storage_account" "costing_storage" {
  name                     = replace("${local.resource_prefix}costing", "-", "")
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

resource azurerm_storage_container "blobforcost" {
  name                  = "blobforcost"
  storage_account_name  = azurerm_storage_account.costing_storage.name
  container_access_type = "private"
}
