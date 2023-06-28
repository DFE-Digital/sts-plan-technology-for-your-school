resource "null_resource" "keyvault-assign-identity" {
  depends_on = [module.main_hosting]

  provisioner "local-exec" {
    interpreter = ["/bin/bash", "-c"]
    command     = local.keyvault-assign-identity_command
  }
}
