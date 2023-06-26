resource "null_resource" "keyvault-add-vnet-restriction" {
  depends_on = [module.main_hosting]

  triggers = {
    ip_address = local.github_runner_ip
  }

  provisioner "local-exec" {
    interpreter = ["/bin/bash", "-c"]
    command     = local.keyvault-add-vnet-restriction_command
  }
}