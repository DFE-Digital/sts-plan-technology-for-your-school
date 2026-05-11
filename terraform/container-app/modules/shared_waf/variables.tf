variable "environment" {
  description = "Environment name. Will be used along with `project_name` as a prefix for all resources."
  type        = string
}

variable "project_name" {
  description = "Project name. Will be used along with `environment` as a prefix for all resources."
  type        = string
}

variable "azure_location" {
  description = "Azure location in which to launch resources."
  type        = string
}

variable "tags" {
  description = "Tags to be applied to all resources"
  type        = map(string)
  default     = {}
}

variable "existing_resource_group" {
  description = "Conditionally launch resources into an existing resource group. Specifying this will NOT create a resource group."
  type        = string
  default     = ""
}

variable "existing_virtual_network" {
  description = "Conditionally use an existing virtual network. The `virtual_network_address_space` must match an existing address space in the VNet. This also requires the resource group name."
  type        = string
  default     = ""
}

variable "virtual_network_address_space" {
  description = "Virtual Network address space CIDR"
  type        = string
  default     = "172.16.0.0/12"

  validation {
    # Ensures the VNet prefix is not smaller than a /28, leaving room for subnets.
    condition     = tonumber(split("/", var.virtual_network_address_space)[1]) <= 28
    error_message = "The VNet address space must be larger than a /28 to allow for subnetting."
  }
}

variable "waf_application" {
  description = "Which product to apply the WAF to. Must be either CDN or AppGatewayV2"
  type        = string
  default     = "CDN"
  validation {
    condition     = contains(["CDN", "AppGatewayV2"], var.waf_application)
    error_message = "waf_application must be either CDN or AppGatewayV2"
  }
}

variable "app_gateway_v2_enable_http2" {
  description = "App Gateway V2 enable HTTP2"
  type        = bool
  default     = true
}

variable "app_gateway_v2_capacity_units" {
  description = "App Gateway V2 capacity units"
  type        = number
  default     = 1
}

variable "app_gateway_v2_frontend_port" {
  description = "App Gateway V2 frontend port"
  type        = number
  default     = 80
}

variable "app_gateway_v2_enable_private_link" {
  description = "Enable Private Link service for App Gateway V2"
  type        = bool
  default     = false
}

variable "app_gateway_v2_cookie_based_affinity" {
  description = "App Gateway V2 Cookie Based Affinity. Sets an affinity cookie in the response with a hash value which contains the session details, so that the subsequent requests carrying the affinity cookie will be routed to the same backend server for maintaining stickiness."
  type        = string
  default     = "Disabled"
}

variable "restrict_app_gateway_v2_to_front_door_inbound_only" {
  description = "Restricts access to the App Gateway V2 by creating a network security group that only allows 'AzureFrontDoor.Backend' inbound, and attaches it to the subnet of the application gateway."
  type        = bool
  default     = false
}

variable "restrict_app_gateway_v2_to_front_door_inbound_only_destination_prefixes" {
  description = "If app gateway v2 has access restricted to front door only (by enabling `restrict_app_gateway_v2_to_front_door_inbound_only`), use this to set the destination prefixes for the security group rule."
  type        = list(string)
  default     = []
}

variable "restrict_app_gateway_v2_to_front_door_inbound_only_destination_prefix" {
  description = "If app gateway v2 has access restricted to front door only (by enabling `restrict_app_gateway_v2_to_front_door_inbound_only`), use this to set the destination prefix for the security group rule."
  type        = string
  default     = "*"
}

variable "app_gateway_v2_identity_ids" {
  description = "App Gateway V2 User Assigned identity ids. If empty, one will be created."
  type        = list(any)
  default     = []
}

variable "app_gateway_v2_custom_error_configuration" {
  description = "A map of Status Codes to HTML URLs"
  type        = map(string)
  default     = {}
}

variable "enable_key_vault_app_gateway_certificates" {
  description = "Deploy a Key Vault to hold TLS Certificates for use by App Gateway"
  type        = bool
  default     = true
}

variable "key_vault_app_gateway_certificates_access_users" {
  description = "List of users that require access to the App Gateway Certificates Key Vault. This should be a list of User Principle Names (Found in Active Directory) that need to run terraform"
  type        = list(string)
  default     = []
}

variable "key_vault_app_gateway_certificates_access_ipv4" {
  description = "List of IPv4 Addresses that are permitted to access the App Gateway Certificates Key Vault"
  type        = list(string)
  default     = []
}

variable "key_vault_app_gateway_enable_rbac" {
  description = "Use RBAC authorisation on the App Gateway Certificates Key Vault. Has no effect if key_vault_app_gateway_certificates_access_users is defined."
  type        = bool
  default     = false
}

variable "key_vault_app_gateway_certificates_access_subnet_ids" {
  description = "List of Azure Subnet IDs that are permitted to access the App Gateway Certificates Key Vault"
  type        = list(string)
  default     = []
}

variable "cdn_sku" {
  description = "Azure CDN Front Door SKU"
  type        = string
  default     = "Standard_AzureFrontDoor"
}

variable "response_request_timeout" {
  description = "Azure CDN Front Door response timeout, or app gateway v2 request timeout in seconds"
  type        = number
  default     = 120
}

variable "waf_targets" {
  description = "Target endpoints to configure the WAF to point towards"
  type = map(
    object({
      domain : string,
      cdn_create_custom_domain : optional(bool, false),
      custom_fqdn : optional(string, "")
      app_gateway_v2_ssl_certificate_key_vault_id : optional(string, "")
      app_gateway_v2_use_private_listener : optional(string, false)
      app_gateway_v2_use_backend_hostname : optional(bool, false)
      vnet_peering_target : optional(object({
        name : string,
        resource_group_name : string
      }))
      enable_health_probe : optional(bool, true),
      health_probe_interval : optional(number, 60),
      health_probe_request_type : optional(string, "HEAD"),
      health_probe_path : optional(string, "/"),
      cdn_add_response_headers : optional(list(object({
        name : string,
        value : string
        })
      ), [])
      cdn_add_request_headers : optional(list(object({
        name : string,
        value : string
        })
      ), [])
      cdn_remove_response_headers : optional(list(string), [])
      cdn_remove_request_headers : optional(list(string), [])
      custom_errors : optional(object({
        error_page_directory : string,
        error_pages : map(string)
      }), null)
      # Private Link additions
      create_private : optional(bool, false)
      private_link_target_id : optional(string)
      private_link_location  : optional(string)
      private_link_target_type : optional(string, "managedEnvironments")
    })
  )
  default = {}

  #private link validation
  validation {
    condition = alltrue([
      for k, v in var.waf_targets :
      (
        !try(v.create_private, false) ||
        (
          try(v.private_link_target_id, null) != null &&
          try(v.private_link_location, null) != null
        )
      )
    ])

    error_message = "When create_private is true, private_link_target_id and private_link_location must be provided."
  }
}

variable "cdn_host_redirects" {
  description = "CDN FrontDoor host redirects `[{ \"from\" = \"example.com\", \"to\" = \"www.example.com\" }]`"
  type        = list(map(string))
  default     = []
}

variable "cdn_url_path_redirects" {
  description = "CDN FrontDoor url path redirects `[{ \"redirect_type\": \"PermanentRedirect\", \"destination_path\": \"/example\", \"destination_hostname\": \"www.example.uk\", \"operator\": \"Equals\", \"match_values\": [\"/example\"] }]`"
  type = list(object({
    redirect_type        = string
    redirect_protocol    = optional(string, null)
    destination_path     = optional(string, null)
    destination_hostname = optional(string, null)
    destination_fragment = optional(string, null)
    query_string         = optional(string, null)
    operator             = string
    match_values         = optional(list(string), [])
    transforms           = optional(list(string), [])
  }))
  default = []
}

variable "cdn_add_response_headers" {
  description = "List of response headers to add at the CDN Front Door for all endpoints `[{ \"Name\" = \"Strict-Transport-Security\", \"value\" = \"max-age=31536000\" }]`"
  type        = list(map(string))
  default     = []
}

variable "cdn_remove_response_headers" {
  description = "List of response headers to remove at the CDN Front Door for all endpoints"
  type        = list(string)
  default     = []
}

variable "existing_monitor_action_group_id" {
  description = "ID of an existing monitor action group"
  type        = string
  default     = ""
}

variable "enable_latency_monitor" {
  description = "Enable CDN latency monitor"
  type        = bool
  default     = false
}

variable "latency_monitor_threshold" {
  description = "CDN latency monitor threshold in milliseconds"
  type        = number
  default     = 5000
}

variable "enable_waf" {
  description = "Enable WAF"
  type        = bool
  default     = false
}

variable "waf_mode" {
  description = "WAF mode"
  type        = string
  default     = "Prevention"
}

variable "cdn_waf_enable_rate_limiting" {
  description = "Deploy a Rate Limiting Policy on the Front Door WAF"
  type        = bool
  default     = false
}

variable "cdn_waf_rate_limiting_duration_in_minutes" {
  description = "Number of minutes to BLOCK requests that hit the Rate Limit threshold"
  type        = number
  default     = 1
}

variable "cdn_waf_rate_limiting_threshold" {
  description = "Maximum number of concurrent requests before Rate Limiting policy is applied"
  type        = number
  default     = 300
}

variable "cdn_waf_rate_limiting_bypass_ip_list" {
  description = "List if IP CIDRs to bypass the Rate Limit Policy"
  type        = list(string)
  default     = []
}

variable "cdn_waf_rate_limiting_action" {
  description = "Action to take when rate limiting (Block/Log)"
  type        = string
  default     = "Block"
  validation {
    condition     = contains(["Allow", "Block", "Log"], var.cdn_waf_rate_limiting_action)
    error_message = "waf_rate_limiting_action must be one of 'Allow', 'Block', or 'Log'"
  }
}

variable "cdn_waf_managed_rulesets" {
  description = "Map of all Managed rules you want to apply to the CDN WAF, including any overrides, or exclusions"
  type = map(object({
    version : string,
    action : optional(string, "Block"),
    exclusions : optional(map(object({
      match_variable : string,
      operator : string,
      selector : string
    })), {})
    overrides : optional(map(map(object({
      action : string,
      enabled : optional(bool, true),
      exclusions : optional(map(object({
        match_variable : string,
        operator : string,
        selector : string
      })), {})
    }))), {})
  }))
  default = {
    "BotProtection" = {
      version = "preview-0.1"
    },
    "DefaultRuleSet" = {
      version = "1.0"
    }
  }
}

variable "app_gateway_v2_waf_managed_rulesets" {
  description = "Map of all Managed rules you want to apply to the App Gateway WAF, including any overrides"
  type = map(object({
    version : string,
    overrides : optional(map(object({
      rules : map(object({
        enabled : bool,
        action : optional(string, "Block")
      }))
    })), {})
  }))
  default = {
    "OWASP" = {
      version = "3.2"
    },
    "Microsoft_BotManagerRuleSet" = {
      version = "1.0"
    }
  }
}

variable "app_gateway_v2_waf_managed_rulesets_exclusions" {
  description = "Map of all exlusions and the assoicated Managed rules to apply to the App Gateway WAF"
  type = map(object({
    match_variable : string,
    selector : string,
    selector_match_operator : string,
    excluded_rule_set : map(object({
      version : string,
      rule_group_name : string,
      excluded_rules : list(string)
    }))
  }))
  default = {}
}

variable "app_gateway_v2_availability_zones" {
  description = "List of Availability Zones in which the App Gateway should be located"
  type        = list(string)
  default     = []
}

variable "waf_custom_rules" {
  description = "Map of all Custom rules you want to apply to the WAF"
  type = map(object({
    priority : number,
    action : string
    match_conditions : map(object({
      match_variable : string,
      match_values : optional(list(string), []),
      operator : optional(string, "Any"),
      selector : optional(string, null),
      negation_condition : optional(bool, false),
    }))
  }))
  default = {}
}

variable "cdn_waf_custom_block_response_status_code" {
  description = "Custom response status code when the WAF blocks a request."
  type        = number
  default     = 0
}

variable "cdn_waf_custom_block_response_body" {
  description = "Base64 encoded custom response body when the WAF blocks a request"
  type        = string
  default     = ""
}

variable "monitor_email_receivers" {
  description = "A list of email addresses that should be notified by monitoring alerts"
  type        = list(string)
  default     = []
}

variable "existing_logic_app_workflow" {
  description = "Name, Resource Group and HTTP Trigger URL of an existing Logic App Workflow"
  type = object({
    name : string
    resource_group_name : string
  })
  default = {
    name                = ""
    resource_group_name = ""
  }
}

variable "enable_waf_alert" {
  description = "Toggle to enable or disable the WAF logs alert"
  type        = bool
  default     = true
}

variable "app_gateway_v2_waf_file_upload_limit_in_mb" {
  description = "Maximum file size permitted in MB"
  type        = number
  default     = 100
}

variable "app_gateway_v2_waf_max_request_body_size_in_kb" {
  description = "Maximum request size for a single request in KB. Has no effect if 'app_gateway_v2_waf_request_body_enforcement' is set to 'false'"
  type        = number
  default     = 128
}

variable "app_gateway_v2_waf_request_body_enforcement" {
  description = "Should the firewall block a request with a body size greater than 'app_gateway_v2_waf_max_request_body_size_in_kb'"
  type        = bool
  default     = true
}
