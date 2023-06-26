resource "null_resource" "keyvault-add-vnet-restriction" {
  depends_on = [azurerm_key_vault_access_policy.vault_access_policy_tf]

  provisioner "local-exec" {
    interpreter = ["/bin/bash", "-c"]
    command     = local.keyvault-add-vnet-restriction_command
  }
}