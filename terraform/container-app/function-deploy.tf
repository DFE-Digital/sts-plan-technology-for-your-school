resource "null_resource" "function-deployment" {
  depends_on = [azurerm_linux_function_app.contentful_function]

  triggers = {
    always_run = timestamp()
  }

  provisioner "local-exec" {
    interpreter = ["/bin/bash", "-c"]
    command     = "${path.module}/scripts/function-deploy.sh ${azurerm_linux_function_app.contentful_function.name}"
  }
}