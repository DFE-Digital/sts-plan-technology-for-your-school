resource "null_resource" "keyvault-add-vnet-restriction" {
  depends_on = [module.main_hosting]

  provisioner "local-exec" {
    interpreter = ["/bin/bash", "-c"]
    command     = local.keyvault-add-vnet-restriction_command
  }
}