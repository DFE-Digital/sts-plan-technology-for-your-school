locals {
  environment              = var.environment
  project_name             = var.project_name
  resource_prefix          = "${local.environment}${local.project_name}"
  azure_location           = var.azure_location
  existing_resource_group  = var.existing_resource_group
  resource_group           = data.azurerm_resource_group.existing_resource_group[0]
  existing_virtual_network = var.existing_virtual_network
  create_virtual_network   = local.waf_application == "AppGatewayV2"
  virtual_network_name = local.existing_virtual_network == "" ? (
    local.create_virtual_network ? azurerm_virtual_network.default[0].name : ""
  ) : data.azurerm_virtual_network.existing_virtual_network[0].name
  virtual_network_address_space = var.virtual_network_address_space
  virtual_network_address_space_mask = element(split("/", local.virtual_network_address_space
  ), 1)
  virtual_network_peering_targets = {
    for key, target in var.waf_targets : key => target.vnet_peering_target if target.vnet_peering_target != null && local.create_virtual_network
  }

  cdn_sku     = var.cdn_sku
  waf_targets = var.waf_targets
  cdn_custom_domains = {
    for waf_target_name, waf_target_value in local.waf_targets : waf_target_name => waf_target_value.custom_fqdn if waf_target_value.cdn_create_custom_domain
  }
  response_request_timeout    = var.response_request_timeout
  cdn_host_redirects          = var.cdn_host_redirects
  cdn_url_path_redirects      = var.cdn_url_path_redirects
  cdn_add_response_headers    = var.cdn_add_response_headers
  cdn_remove_response_headers = var.cdn_remove_response_headers

  existing_monitor_action_group_id = var.existing_monitor_action_group_id
  enable_latency_monitor           = var.enable_latency_monitor
  latency_monitor_threshold        = var.latency_monitor_threshold

  enable_waf                                = var.enable_waf
  waf_application                           = var.waf_application
  enable_waf_alert                          = var.enable_waf_alert
  waf_custom_rules                          = var.waf_custom_rules
  waf_mode                                  = var.waf_mode
  cdn_waf_custom_block_response_status_code = var.cdn_waf_custom_block_response_status_code
  cdn_waf_custom_block_response_body        = var.cdn_waf_custom_block_response_body
  cdn_waf_managed_rulesets                  = var.cdn_waf_managed_rulesets

  cdn_waf_enable_rate_limiting              = var.cdn_waf_enable_rate_limiting
  cdn_waf_rate_limiting_duration_in_minutes = var.cdn_waf_rate_limiting_duration_in_minutes
  cdn_waf_rate_limiting_threshold           = var.cdn_waf_rate_limiting_threshold
  cdn_waf_rate_limiting_bypass_ip_list      = var.cdn_waf_rate_limiting_bypass_ip_list
  cdn_waf_rate_limiting_action              = var.cdn_waf_rate_limiting_action


  content_types = {
    css   = "text/css"
    html  = "text/html"
    js    = "application/javascript"
    png   = "image/png"
    svg   = "image/svg+xml"
    woff  = "font/woff"
    woff2 = "font/woff2"
  }

  tags = var.tags
}
