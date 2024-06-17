module "waf" {
  source = "github.com/dfe-digital/terraform-azurerm-front-door-app-gateway-waf?ref=v0.3.6"

  depends_on = [module.main_hosting]

  environment             = local.environment
  project_name            = local.project_name
  azure_location          = local.azure_location
  existing_resource_group = local.resource_group_name

  enable_waf       = true
  enable_waf_alert = false
  waf_mode         = "Prevention"
  waf_application  = "CDN"

  waf_targets = {
    "container-app-url" = {
      health_probe_request_type = "GET"
      domain                    = module.main_hosting.container_fqdn
      create_custom_domain      = local.cdn_create_custom_domain
      custom_fqdn               = module.main_hosting.container_fqdn
    }
  }

  cdn_sku = "Premium_AzureFrontDoor"

  cdn_waf_enable_rate_limiting              = true
  cdn_waf_rate_limiting_duration_in_minutes = 5
  cdn_waf_rate_limiting_threshold           = 1000
  cdn_waf_rate_limiting_action              = "Block"

  cdn_waf_managed_rulesets = {
    "Microsoft_DefaultRuleSet" = {
      version = "2.1",
      action  = "Block",
      overrides = {
        "SQLI" = {
          "942200" = {
            action  = "Log"
            enabled = false
          },
          "942340" = {
            action  = "Log"
            enabled = false
          },
          "942450" = {
            action  = "Log"
            enabled = false
          }
          "942370" = {
            action  = "Log"
            enabled = false
          }
        },
        "RFI" = {
          "931130" = {
            action  = "Log"
            enabled = false
          }
        }
      }
    },
    "Microsoft_BotManagerRuleSet" = {
      version = "1.0",
      action  = "Block"
    }
  }

  cdn_waf_custom_block_response_status_code = 405
  cdn_waf_custom_block_response_body        = base64encode("<h1>Service unavailable</h1>")

  cdn_add_response_headers = local.cdn_frontdoor_host_add_response_headers

  tags = local.tags
}
