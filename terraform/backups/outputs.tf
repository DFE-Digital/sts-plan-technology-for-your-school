output "backup_resource_group_name" {
  description = "Backup resource group name"
  value       = azurerm_resource_group.sql_backups.name
}

output "backup_storage_account_name" {
  description = "Backup storage account name"
  value       = azurerm_storage_account.sql_backups.name
}

output "backup_container_name" {
  description = "Backup blob container name"
  value       = azurerm_storage_container.bacpacs.name
}

output "backup_primary_blob_endpoint" {
  description = "Primary blob endpoint for the backup storage account"
  value       = azurerm_storage_account.sql_backups.primary_blob_endpoint
}

output "backup_container_url" {
  description = "Full URL of the backup blob container"
  value       = "${azurerm_storage_account.sql_backups.primary_blob_endpoint}${azurerm_storage_container.bacpacs.name}"
}
