locals {
  project_name    = var.project_name
  environment     = var.environment
  resource_prefix = "${local.environment}${local.project_name}"
  azure_location  = var.azure_location
  tags = {
    "Environment"      = var.az_tag_environment,
    "Service Offering" = var.az_tag_product,
    "Product"          = var.az_tag_product
  }
}