locals {
  project_name   = var.project_name
  environment    = var.environment
  azure_location = var.azure_location
  tags = {
    "Environment"      = var.az_tag_environment,
    "Service Offering" = var.az_tag_product,
    "Product"          = var.az_tag_product
  }

  restrict_container_apps_to_cdn_inbound_only = var.restrict_container_apps_to_cdn_inbound_only
}