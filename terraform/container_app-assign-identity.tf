resource "null_resource" "app-assign-identity" {
  depends_on = [module.main_hosting]

  triggers = {
    always_run = "${timestamp()}"
  }
  
  provisioner "local-exec" {
    interpreter = ["/bin/bash", "-c"]
    command     = local.container_app-assign-identity
  }
}
