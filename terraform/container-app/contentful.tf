data "azurerm_cdn_frontdoor_endpoint" "app" {
  name                = "${local.resource_prefix}-container-app-url"
  profile_name        = "${local.resource_prefix}-cdnwaf"
  resource_group_name = local.resource_prefix
}

resource "null_resource" "upsert_contentful_webhook" {
  triggers = {
    api_key_change         = azurerm_key_vault_secret.api_key.value
    url_change             = data.azurerm_cdn_frontdoor_endpoint.app.host_name
    management_token       = var.contentful_management_token
    contentful_environment = azurerm_key_vault_secret.vault_secret_contentful_environment.value
    contentful_space       = azurerm_key_vault_secret.vault_secret_contentful_spaceid.value
    webhook_url            = var.contentful_webhook_endpoint
    should_upsert          = var.contentful_upsert_webhook == true ? timestamp() : var.contentful_upsert_webhook
  }

  provisioner "local-exec" {
    command = local.contentful_webhook_shell_command
  }
}

resource "azurerm_storage_account" "contentful_backup_storage" {
  name                     = replace("${local.resource_prefix}content", "-", "")
  resource_group_name      = local.resource_prefix
  location                 = local.azure_location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  tags                     = local.tags

  blob_properties {
    container_delete_retention_policy {
      days = 30
    }

    delete_retention_policy {
      days = 30
    }
  }
}

resource "azurerm_storage_container" "backups_container" {
  name                  = "backups-container"
  storage_account_id    = azurerm_storage_account.contentful_backup_storage.id
  container_access_type = "private"
}

resource "azurerm_storage_management_policy" "lifecycle_policy" {
  storage_account_id = azurerm_storage_account.contentful_backup_storage.id

  rule {
    name    = "delete_after_30_days"
    enabled = true

    filters {
      blob_types = ["blockBlob"]
    }

    actions {
      base_blob {
        delete_after_days_since_creation_greater_than = 30
      }
    }
  }
}
