locals {
  project_name   = var.project_name
  environment    = var.environment
  azure_location = var.azure_location
  tags = {
    "Environment"      = var.az_tag_environment,
    "Service Offering" = var.az_tag_product,
    "Product"          = var.az_tag_product
  }
  cdn_frontdoor_origin_host_header_override = var.cdn_frontdoor_origin_host_header_override

  container_app_image_name = "plan-tech-app"
  resource_prefix          = "${local.environment}${local.project_name}"
  resource_group_name      = "${local.environment}${local.project_name}"
  container_app_name       = "${local.resource_prefix}-${local.container_app_image_name}"
  user_identity_name       = "${local.resource_prefix}-mi"

  keyvault-assign-identity = "timeout 15m ${path.module}/scripts/assign-user-identity-to-key-vault.sh -n \"${local.container_app_name}\" -g \"${local.resource_group_name}\" -u \"${local.user_identity_name}\""
}