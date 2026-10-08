resource "azurerm_storage_account" "backups" {
  name                     = var.backup_storage_account_name
  resource_group_name      = azurerm_resource_group.sql_backups.name
  location                 = azurerm_resource_group.sql_backups.location
  account_tier             = "Standard"
  account_replication_type = "GRS"

  blob_properties {
    versioning_enabled = true

    delete_retention_policy {
      days = var.blob_delete_retention_days
    }

    container_delete_retention_policy {
      days = var.container_delete_retention_days
    }
  }

  tags = local.tags

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_storage_container" "bacpacs" {
  name                  = var.backup_container_name_sql
  storage_account_id    = azurerm_storage_account.backups.id
  container_access_type = "private"

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_storage_container_immutability_policy" "bacpacs" {
  count = var.immutability_enabled ? 1 : 0

  storage_container_resource_manager_id = azurerm_storage_container.bacpacs.id
  immutability_period_in_days           = var.immutability_period_days
  protected_append_writes_all_enabled   = false
}

resource "azurerm_storage_container" "contentful" {
  name                  = var.backup_container_name_contentful
  storage_account_id    = azurerm_storage_account.backups.id
  container_access_type = "private"

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_storage_container_immutability_policy" "contentful" {
  count = var.immutability_enabled ? 1 : 0

  storage_container_resource_manager_id = azurerm_storage_container.contentful.id
  immutability_period_in_days           = var.immutability_period_days
  protected_append_writes_all_enabled   = false
}

resource "azurerm_storage_container" "secrets" {
  name                  = var.backup_container_name_secrets
  storage_account_id    = azurerm_storage_account.backups.id
  container_access_type = "private"

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_storage_container_immutability_policy" "secrets" {
  count = var.immutability_enabled ? 1 : 0

  storage_container_resource_manager_id = azurerm_storage_container.secrets.id
  immutability_period_in_days           = var.immutability_period_days
  protected_append_writes_all_enabled   = false
}
