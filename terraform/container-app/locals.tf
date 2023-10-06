locals {
  ###########
  # General #
  ###########
  current_user_id     = coalesce(var.msi_id, data.azurerm_client_config.current.object_id)
  project_name        = var.project_name
  environment         = var.environment
  azure_location      = var.azure_location
  resource_prefix     = "${local.environment}${local.project_name}"
  resource_group_name = module.main_hosting.azurerm_resource_group_default.name

  tags = {
    "Environment"      = var.az_tag_environment,
    "Service Offering" = var.az_tag_product,
    "Product"          = var.az_tag_product
  }

  #################
  # Container App #
  #################
  container_app_image_name = "plan-tech-app"
  container_app_name       = "${local.resource_prefix}-${local.container_app_image_name}"
  kestrel_endpoint         = var.az_app_kestrel_endpoint
  container_port           = var.az_container_port

  ####################
  # Managed Identity #
  ####################
  user_identity_name = "${local.resource_prefix}-mi"

  #############
  # Azure SQL #
  #############
  az_sql_connection_string      = "Server=tcp:${local.resource_prefix}.database.windows.net,1433;Initial Catalog=${local.resource_prefix}-sqldb;Authentication=Active Directory Default; Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  az_sql_azuread_admin_username = var.az_sql_azuread_admin_username
  az_sql_azuread_admin_objectid = var.az_sql_azuread_admin_objectid
  az_use_azure_ad_auth_only     = var.az_tag_environment != "Dev"

  ##################
  # Azure KeyVault #
  ##################
  kv_name = "${local.environment}${local.project_name}-kv"

  ##################
  # CDN/Front Door #
  ##################
  cdn_create_custom_domain = var.cdn_create_custom_domain
}
