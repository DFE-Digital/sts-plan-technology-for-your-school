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
  az_sql_max_size_gb            = local.az_sql_sku == "Basic" ? null : 10
  
  mssql_firewall_ipv4_allow_list = lookup(var.mssql_firewall_ipv4_allow_list, var.environment, {})

  az_sql_vnet = {
    dns_zone_name = "privatelink.database.windows.net"
    nic_name      = "${local.resource_prefix}-db-nic"
    endpoint_name = "${local.resource_prefix}-db"
  }

  #####################
  # Azure Redis Cache #
  #####################
  redis_capacity    = var.redis_capacity
  redis_family      = var.redis_family
  redis_sku_name    = var.redis_sku_name
  redis_tls_version = var.redis_tls_version

  redis_vnet = {
    dns_zone_name = "privatelink.redis.cache.windows.net"
    nic_name      = "${local.resource_prefix}-redis-cache-nic"
    endpoint_name = "${local.resource_prefix}-redis-cache"
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
  cdn_frontdoor_url_path_redirects = var.cdn_frontdoor_url_path_redirects

  waf_custom_rules = {
    //WAF policy to allow payloads to the CMS webhook route
    ContentfulWebhookBypassPolicy = {
      priority = 2
      action   = "Allow"

      match_conditions = {
        has_contentful_topic_header = {
          match_variable = "RequestHeader"
          selector       = "X-Contentful-Topic"
          operator       = "Any"
          match_values   = []
        }
        has_contentful_webhookname_header = {
          match_variable = "RequestHeader"
          selector       = "X-Contentful-Webhook-Name"
          operator       = "Any"
          match_values   = []
        }
        has_contentful_eventdatetime_header = {
          match_variable = "RequestHeader"
          selector       = "X-Contentful-Event-Datetime"
          operator       = "Any"
          match_values   = []
        }
        has_contentful_crn_header = {
          match_variable = "RequestHeader"
          selector       = "X-Contentful-CRN"
          operator       = "Any"
          match_values   = []
        }
        is_json_payload = {
          match_variable = "RequestHeader"
          selector       = "Content-Type"
          operator       = "Equal"
          match_values   = ["application/json"]
        }
        has_auth_header = {
          match_variable = "RequestHeader"
          selector       = "Authorization"
          operator       = "Any"
          match_values   = []
        }
        is_webhook_uri = {
          match_variable = "RequestUri"
          operator       = "Contains"
          match_values   = ["/api/cms/webhook"]
        }
      }
    }
  }

  ####################x
  # Storage Accounts #
  ####################

  container_app_storage_account_shared_access_key_enabled = var.container_app_storage_account_shared_access_key_enabled
  container_app_blob_storage_public_access_enabled        = var.container_app_blob_storage_public_access_enabled
  storage_account_expiration_period                       = var.storage_account_expiration_period

  ###########################
  # Content Security Policy #
  ###########################

  csp_clarity_domains           = "https://www.clarity.ms https://c.bing.com https://a.clarity.ms https://b.clarity.ms https://c.clarity.ms https://d.clarity.ms https://e.clarity.ms https://f.clarity.ms https://g.clarity.ms https://h.clarity.ms https://i.clarity.ms https://j.clarity.ms https://k.clarity.ms https://l.clarity.ms https://m.clarity.ms https://n.clarity.ms https://o.clarity.ms https://p.clarity.ms https://q.clarity.ms https://r.clarity.ms https://s.clarity.ms https://t.clarity.ms https://u.clarity.ms https://v.clarity.ms https://w.clarity.ms https://x.clarity.ms https://y.clarity.ms https://z.clarity.ms"
  csp_google_tag_manager_domain = "www.googletagmanager.com"

  ######################
  # Contentful Webhook #
  ######################

  contentful_webhook_name                    = var.contentful_webhook_name
  contentful_webhook_url                     = "https://${data.azurerm_cdn_frontdoor_endpoint.app.host_name}${var.contentful_webhook_endpoint}"
  contentful_webhook_shell_command           = var.contentful_management_token != null && var.contentful_upsert_webhook == true ? "bash ./scripts/create-contentful-webhook.sh --env-id ${azurerm_key_vault_secret.vault_secret_contentful_environment.value} --env-name \"${var.container_environment}\" --management-token \"${var.contentful_management_token}\" --space-id ${azurerm_key_vault_secret.vault_secret_contentful_spaceid.value} --webhook-api-key \"${random_password.api_key_value.result}\" --webhook-name \"${local.contentful_webhook_name}\" --webhook-url ${local.contentful_webhook_url}" : "echo Not updating webhook"
  contentful_webhook_secret_timetolive_hours = 365 * 24
}
