resource "null_resource" "keyvault-add-vnet-restriction" {
  depends_on = [module.main_hosting]

  triggers = {
    managed_identity_id = azurerm_user_assigned_identity.user_assigned_identity.id
  }

  provisioner "local-exec" {
    interpreter = ["/bin/bash", "-c"]
    command     = local.keyvault-add-vnet-restriction_command
  }
}