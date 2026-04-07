resource "azurerm_role_assignment" "github_actions_storage_blob_data_contributor" {
  scope                = azurerm_storage_account.sql_backups.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.github_actions_principal_id
}
