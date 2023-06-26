resource "null_resource" "keyvault-remove-ip-whitelist" {
  depends_on = [azurerm_key_vault_secret.vault_secret_database_connectionstring]

  provisioner "local-exec" {
    interpreter = ["/bin/bash", "-c"]
    command     = local.keyvault-remove-ip-whitelist_command
  }
}