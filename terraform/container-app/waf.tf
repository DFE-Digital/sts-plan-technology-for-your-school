module "waf" {
  source = "github.com/dfe-digital/terraform-azurerm-front-door-app-gateway-waf?ref=v0.3.2"

  depends_on = [module.main_hosting]

  environment             = local.environment
  project_name            = local.project_name
  azure_location          = local.azure_location
  existing_resource_group = local.resource_group_name

  enable_waf      = true
  waf_mode        = "Prevention"
  waf_application = "CDN"

  waf_targets = {
    "container-app-url" = {
      domain               = local.cdn_frontdoor_origin_host_header_override
      create_custom_domain = local.cdn_create_custom_domain
      custom_fqdn          = local.cdn_frontdoor_origin_host_header_override
    }
  }

  cdn_sku = "Premium_AzureFrontDoor"

  cdn_waf_enable_rate_limiting              = true
  cdn_waf_rate_limiting_duration_in_minutes = 5
  cdn_waf_rate_limiting_threshold           = 1000
  cdn_waf_rate_limiting_action              = "Block" # one of "Allow", "Block", "Log"

  cdn_waf_managed_rulesets = {
    "Microsoft_DefaultRuleSet" = {
      version = "2.1",
      action  = "Block"
    },
    "Microsoft_BotManagerRuleSet" = {
      version = "1.0",
      action  = "Block"
    }
  }

  cdn_waf_custom_block_response_status_code = 405
  cdn_waf_custom_block_response_body        = base64encode("<h1>Service unavailable</h1>")

  tags = local.tags
}
