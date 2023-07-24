module "main_hosting" {
  source = "github.com/DFE-Digital/terraform-azurerm-container-apps-hosting?ref=v0.17.5"

  ###########
  # General #
  ###########
  environment    = local.environment
  project_name   = local.project_name
  azure_location = local.azure_location
  tags           = local.tags

  #################
  # Container App #
  #################
  enable_container_registry = true
  image_name                = local.container_app_image_name
  container_secret_environment_variables = {
    "AZURE_CLIENT_ID" = azurerm_user_assigned_identity.user_assigned_identity.client_id,
    "KeyVaultName"    = local.kv_name
  }

  container_environment_variables = {
    "Kestrel__Endpoints__Https__Url" = local.kestrel_endpoint,
    "ASPNETCORE_FORWARDEDHEADERS_ENABLED" = "true"
  }

  ##############
  # Front Door #
  ##############
  enable_cdn_frontdoor                      = true
  cdn_frontdoor_origin_host_header_override = local.cdn_frontdoor_origin_host_header_override

  #############
  # Azure SQL #
  #############
  enable_mssql_database       = true
  mssql_database_name         = "${local.resource_prefix}-sqldb"
  mssql_server_admin_password = local.az_sql_admin_password
}