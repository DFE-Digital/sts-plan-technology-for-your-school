resource "null_resource" "avnet-add-kv-service-endpoint" {
  depends_on = [module.main_hosting]

  provisioner "local-exec" {
    interpreter = ["/bin/bash", "-c"]
    command     = local.vnet-add_kv_service_endpoint
  }
}