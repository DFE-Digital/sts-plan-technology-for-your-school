module "main_hosting" {
  source = "github.com/DFE-Digital/terraform-azurerm-container-apps-hosting?ref=v2.6.3"
  depends_on = [azurerm_resource_group.app_rg, azurerm_container_registry.acr_notshared]
  ###########
  # General #
  ###########
  environment    = local.environment
  project_name   = local.project_name
  azure_location = local.azure_location
  tags           = local.tags
  existing_resource_group = local.existing_resource_group

  ############################################
  # Networking configuration
  ############################################
  #having both true prevents the public IP issue by forcing creation of internal load balancer
  # Deploy container apps inside a VNet
  launch_in_vnet = local.launch_in_vnet
  container_app_environment_internal_load_balancer_enabled = local.container_app_environment_internal_load_balancer_enabled

  #################
  # Container App #
  #################
  enable_container_registry = local.enable_container_registry
  image_name                = local.container_app_image_name
  container_port            = local.container_port
  container_secret_environment_variables = {
    "KeyVaultName"    = local.kv_name,
    "AZURE_CLIENT_ID" = azurerm_user_assigned_identity.user_assigned_identity.client_id,
  }

  container_environment_variables = local.container_environment_variables

  container_app_identities           = [azurerm_user_assigned_identity.user_assigned_identity.id]
  container_app_use_managed_identity = false

  container_max_replicas           = local.container_app_max_replicas
  container_min_replicas           = local.container_app_min_replicas
  container_scale_http_concurrency = local.container_app_http_concurrency
  container_app_environment_workload_profile_type = var.container_app_environment_workload_profile_type

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
  mssql_firewall_ipv4_allow_list     = local.az_mssql_ipv4_allow_list

  ##############
  # Networking #
  ##############
  container_apps_infra_subnet_service_endpoints = ["Microsoft.KeyVault", "Microsoft.Storage"]

  #############################
  # Azure Container Registry #
  #############################
  registry_server = coalesce(
    var.registry_server,
    azurerm_container_registry.acr_notshared[0].login_server
  )

  registry_username = coalesce(
    var.registry_username,
    azurerm_container_registry.acr_notshared[0].admin_username
  )

  registry_password = coalesce(
    var.registry_password,
    azurerm_container_registry.acr_notshared[0].admin_password
  )
  image_tag         = local.image_tag

  ###########
  # Storage #
  ###########
  storage_account_sas_expiration_period = local.storage_account_expiration_period
}
