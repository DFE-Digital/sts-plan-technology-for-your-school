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
  registry_server     = var.registry_server
  registry_username   = var.registry_username
  registry_password   = var.registry_password

  tags = {
    "Environment"      = var.az_tag_environment,
    "Service Offering" = var.az_tag_product,
    "Product"          = var.az_tag_product
  }

  #################
  # Container App #
  #################
  container_app_image_name       = "plan-tech-app"
  kestrel_endpoint               = var.az_app_kestrel_endpoint
  container_port                 = var.az_container_port
  image_tag                      = var.image_tag
  container_app_min_replicas     = var.container_app_min_replicas
  container_app_max_replicas     = var.container_app_max_replicas
  container_app_http_concurrency = var.container_app_http_concurrency

  ####################
  # Managed Identity #
  ####################
  user_identity_name = "${local.resource_prefix}-mi"

  #############
  # Azure SQL #
  #############
  az_sql_connection_string      = "Server=tcp:${local.resource_prefix}.database.windows.net,1433;Initial Catalog=${local.resource_prefix}-sqldb;Authentication=Active Directory Default; Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Max Pool Size=${local.az_sql_max_pool_size};"
  az_sql_azuread_admin_username = var.az_sql_azuread_admin_username
  az_sql_admin_password         = var.az_sql_admin_password
  az_sql_azuread_admin_objectid = var.az_sql_azuread_admin_objectid
  az_use_azure_ad_auth_only     = var.az_tag_environment != "Dev"
  az_sql_sku                    = var.az_sql_sku
  az_sql_max_pool_size          = var.az_sql_max_pool_size

  ##################
  # Azure KeyVault #
  ##################
  kv_name = "${local.environment}${local.project_name}-kv"

  ##################
  # CDN/Front Door #
  ##################
  cdn_create_custom_domain = var.cdn_create_custom_domain
  cdn_frontdoor_host_add_response_headers = length(var.cdn_frontdoor_host_add_response_headers) > 0 ? var.cdn_frontdoor_host_add_response_headers : [{
    "name"  = "Strict-Transport-Security",
    "value" = "max-age=31536000",
    },
    {
      "name"  = "X-Content-Type-Options",
      "value" = "nosniff",
    },
    {
      "name"  = "X-XSS-Protection",
      "value" = "1",
  }]


  ####################x
  # Storage Accounts #
  ####################

  storage_account_public_access_enabled                   = var.storage_account_public_access_enabled
  container_app_storage_account_shared_access_key_enabled = var.container_app_storage_account_shared_access_key_enabled
  container_app_blob_storage_public_access_enabled        = var.container_app_blob_storage_public_access_enabled
}
