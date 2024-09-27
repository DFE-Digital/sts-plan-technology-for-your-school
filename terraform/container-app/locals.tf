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

  container_environment_variables_default = {
    "Kestrel__Endpoints__Http__Url"       = local.kestrel_endpoint,
    "ASPNETCORE_FORWARDEDHEADERS_ENABLED" = "true",
    "ASPNETCORE_ENVIRONMENT"              = var.container_environment
  }

  container_environment_variables = merge(local.container_environment_variables_default, var.container_environment_variables)

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
  az_sql_max_size_gb            = local.az_sql_sku == "Basic" ? null : 512

  az_sql_vnet = {
    dns_zone_name = "privatelink.database.windows.net"
    nic_name      = "${local.resource_prefix}-db-nic"
    endpoint_name = "${local.resource_prefix}-db"
  }

  ##################
  # Azure KeyVault #
  ##################
  kv_name = "${local.environment}${local.project_name}-kv"

  kv_secrets_csp_connectsrc = "${local.csp_google_tag_manager_domain} region1.google-analytics.com ${local.csp_clarity_domains}"
  kv_secrets_csp_defaultsrc = local.csp_clarity_domains
  kv_secrets_csp_framesrc   = "${local.csp_google_tag_manager_domain} ${local.csp_clarity_domains}"
  kv_secrets_csp_imgsrc     = "${local.csp_google_tag_manager_domain} ${local.csp_clarity_domains}"

  kv_networking = {
    subnet = {
      name             = "${local.resource_prefix}-keyvault-endpoint"
      address_prefixes = ["172.16.7.0/24"]
    }
    private_dns_zone = {
      name = "privatelink.vaultcore.azure.net"
    }
    endpoint = {
      nic_name = "${local.resource_prefix}-keyvault-nic"
      name     = "${local.resource_prefix}-keyvault"
    }
  }

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
  storage_account_expiration_period                       = var.storage_account_expiration_period

  ###########################
  # Content Security Policy #
  ###########################

  csp_clarity_domains           = "https://www.clarity.ms https://c.bing.com https://a.clarity.ms https://b.clarity.ms https://c.clarity.ms https://d.clarity.ms https://e.clarity.ms https://f.clarity.ms https://g.clarity.ms https://h.clarity.ms https://i.clarity.ms https://j.clarity.ms https://k.clarity.ms https://l.clarity.ms https://m.clarity.ms https://n.clarity.ms https://o.clarity.ms https://p.clarity.ms https://q.clarity.ms https://r.clarity.ms https://s.clarity.ms https://t.clarity.ms https://u.clarity.ms https://v.clarity.ms https://w.clarity.ms https://x.clarity.ms https://y.clarity.ms https://z.clarity.ms"
  csp_google_tag_manager_domain = "www.googletagmanager.com"

  ################
  # Function App #
  ################

  function = {
    name     = "${local.resource_prefix}contentfulfunction",
    location = "northeurope",

    runtime = var.function_runtime,
    scaling = var.function_scaling,

    vnet = {
      name          = "${local.resource_prefix}-function-vn"
      address_space = "10.0.0.0/14"

      subnets = {
        infra = {
          name             = "${local.resource_prefix}-functioninfra"
          address_prefixes = ["10.0.0.0/24"]
        }

        storage = {
          name             = "${local.resource_prefix}-function-storage"
          address_prefixes = ["10.0.1.0/24"]
        }
      }

      dns = {
        blob = {
          name = "privatelink.blob.core.windows.net"
        }

        files = {
          name = "privatelink.file.core.windows.net"
        }
      }

      endpoints = {
        blob = {
          nic_name = "${local.resource_prefix}-blob-storage-nic"
          name     = "${local.resource_prefix}-blob-storage"
        }

        files = {
          nic_name = "${local.resource_prefix}-files-storage-nic"
          name     = "${local.resource_prefix}-files-storage"
        }
      }
    }
  }
}
