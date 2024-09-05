module "main_hosting" {
  source = "github.com/DFE-Digital/terraform-azurerm-container-apps-hosting?ref=v1.10.3"

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
  container_port            = local.container_port
  container_secret_environment_variables = {
    "AZURE_CLIENT_ID" = azurerm_user_assigned_identity.user_assigned_identity.client_id,
    "KeyVaultName"    = local.kv_name
  }

  container_environment_variables = {
    "Kestrel__Endpoints__Http__Url"       = local.kestrel_endpoint,
    "ASPNETCORE_FORWARDEDHEADERS_ENABLED" = "true"
  }

  container_app_identities = {
    type         = "UserAssigned",
    identity_ids = [azurerm_user_assigned_identity.user_assigned_identity.id]
  }

  container_max_replicas           = local.container_app_max_replicas
  container_min_replicas           = local.container_app_min_replicas
  container_scale_http_concurrency = local.container_app_http_concurrency

  #############
  # Azure SQL #
  #############
  enable_mssql_database              = true
  mssql_database_name                = "${local.resource_prefix}-sqldb"
  mssql_server_public_access_enabled = true
  mssql_server_admin_password        = local.az_sql_admin_password
  mssql_azuread_admin_username       = local.az_sql_azuread_admin_username
  mssql_azuread_admin_object_id      = local.az_sql_azuread_admin_objectid
  mssql_azuread_auth_only            = local.az_use_azure_ad_auth_only
  mssql_managed_identity_assign_role = false
  mssql_sku_name                     = local.az_sql_sku
  mssql_max_size_gb                  = local.az_sql_max_size_gb

  ##############
  # Networking #
  ##############
  container_apps_infra_subnet_service_endpoints = ["Microsoft.KeyVault", "Microsoft.Storage"]

  #############################
  # Github Container Registry #
  #############################
  registry_server   = local.registry_server
  registry_username = local.registry_username
  registry_password = local.registry_password
  image_tag         = local.image_tag

  ###########
  # Storage #
  ###########
  storage_account_sas_expiration_period = local.storage_account_expiration_period
}
