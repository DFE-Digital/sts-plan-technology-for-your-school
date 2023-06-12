module "main_hosting" {
  source = "github.com/DFE-Digital/terraform-azurerm-container-apps-hosting?ref=v0.17.5"

  environment    = local.environment
  project_name   = local.project_name
  azure_location = local.azure_location
  tags           = local.tags

  enable_container_registry = true
  image_name                = local.container_app_image_name
  container_secret_environment_variables = {
    "managedidentity__clientid" = azurerm_user_assigned_identity.managed_identity.client_id,
    "contentful__deliveryapikey" = "rShyhJ2FI1oo5HpWFbXOO0zyr6X-uCkUrLvF_qgwidY",
    "contentful__previewapikey" = "So9qXGp7CerLUYNJGkeHO42G7fBMIzig92Hb4xyOrGQ",
    "contentful__spaceid" = "py5afvqdlxgo",
    "contentful__environment" = "dev",
  }  

  enable_cdn_frontdoor = true

  cdn_frontdoor_origin_host_header_override = local.cdn_frontdoor_origin_host_header_override
}