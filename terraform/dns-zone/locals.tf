locals {
  ###########
  # General #
  ###########
  is_dr               = var.is_dr
  current_user_id     = coalesce(var.msi_id, data.azurerm_client_config.current.object_id)
  project_name        = var.project_name
  environment         = var.environment
  azure_location      = var.azure_location
  resource_prefix     = "${local.environment}${local.project_name}"
  rg_name = 

  ##for looking up front door in tf state. project and storage name needed shortening for dr
  tf_rg = var.is_dr ? "${var.environment}pt-dr-tf" : "${local.resource_prefix}-tf"
  tf_sa = var.is_dr ? "${replace(local.tf_rg, "-", "")}" : "${replace(local.resource_prefix, "-", "")}tfstate"
  tf_container        = "tfstate"
  tf_key              = "terraform.tfstate"

  tags = {
    "Environment"      = var.az_tag_environment,
    "Service Offering" = var.az_tag_product,
    "Product"          = var.az_tag_product
  }
}
