resource "azurerm_resource_group" "sql_backups" {
  name     = var.backup_resource_group_name
  location = var.azure_location

  tags = local.tags

  lifecycle {
    prevent_destroy = true
  }
}
