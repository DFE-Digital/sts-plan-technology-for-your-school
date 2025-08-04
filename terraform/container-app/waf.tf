module "waf" {
  source = "github.com/dfe-digital/terraform-azurerm-front-door-app-gateway-waf?ref=f0ca7eb"

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

  waf_custom_rules = local.waf_custom_rules

  cdn_sku = "Premium_AzureFrontDoor"

  cdn_waf_enable_rate_limiting              = true
  cdn_waf_rate_limiting_duration_in_minutes = 1
  cdn_waf_rate_limiting_threshold           = 100
  cdn_waf_rate_limiting_action              = "Block"

  cdn_waf_managed_rulesets = {
    "Microsoft_DefaultRuleSet" = {
      version = "2.1",
      action  = "Block",
      overrides = {
        "SQLI" = {
          "942110" = {
            action  = "Log"
            enabled = false
          },
          "942150" = {
            action  = "Log"
            enabled = false
          },
          "942200" = {
            action  = "Log"
            enabled = false
          },
          "942260" = {
            action  = "Log"
            enabled = false
          },
          "942330" = {
            action  = "Log"
            enabled = false
          },
          "942340" = {
            action  = "Log"
            enabled = false
          },
          "942370" = {
            action  = "Log"
            enabled = false
          },
          "942430" = {
            action  = "Log"
            enabled = false
          },
          "942440" = {
            action  = "Log"
            enabled = false
          },
          "942450" = {
            action  = "Log"
            enabled = false
          }
        },
        "RFI" = {
          "931130" = {
            action  = "Log"
            enabled = false
          }
        },
        "MS-ThreatIntel-WebShells" = {
          "99005006" = {
            action  = "AnomalyScoring"
            enabled = false
          }
        },
        "MS-ThreatIntel-CVEs" = {
          "99001015" = {
            action  = "AnomalyScoring"
            enabled = false
          },
          "99001016" = {
            action  = "AnomalyScoring"
            enabled = false
          },
          "99001014" = {
            action  = "AnomalyScoring"
            enabled = false
          },
          "99001017" = {
            action  = "AnomalyScoring"
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
  cdn_url_path_redirects   = local.cdn_frontdoor_url_path_redirects

  tags = local.tags
}
