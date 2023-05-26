module "main_hosting" {
  source = "github.com/DFE-Digital/terraform-azurerm-container-apps-hosting"

  environment    = local.environment
  project_name   = local.project_name
  azure_location = local.azure_location
  tags           = local.tags

  enable_container_registry = false
  image_name                = "my-app"
}