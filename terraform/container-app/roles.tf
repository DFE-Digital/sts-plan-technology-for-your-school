
resource "azurerm_role_assignment" "github_actions_kv_secretsadmin" {
  scope                = azurerm_key_vault.vault.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = var.github_actions_principal_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_role_assignment" "mi-crypto" {
  scope                = azurerm_key_vault.vault.id
  role_definition_name = "Key Vault Crypto Service Encryption User"
  principal_id         = azurerm_user_assigned_identity.user_assigned_identity.id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_role_assignment" "mi-secretsuser" {
  scope                = azurerm_key_vault.vault.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.user_assigned_identity.id
  principal_type       = "ServicePrincipal"
}
