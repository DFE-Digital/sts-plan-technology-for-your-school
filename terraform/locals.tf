locals {
  dfe_project_name   = var.dfe_project_name
  az_environment     = var.az_environment
  az_resource_prefix = "${local.az_environment}${local.dfe_project_name}"
  az_location  = var.az_location
  az_tags = {
    "Environment"      = var.az_tag_environment,
    "Service Offering" = var.az_tag_product,
    "Product"          = var.az_tag_product
  }
}