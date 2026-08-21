resource "azurerm_role_assignment" "github_actions_storage_blob_data_contributor" {
  scope                = azurerm_storage_account.backups.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.github_actions_principal_id
  principal_type       = "ServicePrincipal"
}
resource "azurerm_role_assignment" "github_actions_kv_secretsofficer" {
  scope                = azurerm_key_vault.backup.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = var.github_actions_principal_id
  principal_type       = "ServicePrincipal"
}
