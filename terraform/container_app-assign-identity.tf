resource "null_resource" "app-assign-identity" {
  depends_on = [azurerm_user_assigned_identity.user_assigned_identity]

  triggers = {
    always_run = timestamp()
  }

  provisioner "local-exec" {
    interpreter = ["/bin/bash", "-c"]
    command     = local.container_app-assign-identity
  }
}
