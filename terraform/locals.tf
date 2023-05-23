locals {
  dfe_project_name   = "plantech"
  az_environment     = "s190d01-"
  az_resource_prefix = "${local.az_environment}${local.dfe_project_name}"
  az_location  = "westeurope"
  az_tags = {
    "Environment"      = "Dev",
    "Service Offering" = "Plan Technology for your School",
    "Product"          = "Plan Technology for your School"
  }
}