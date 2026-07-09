###########
# General #
###########
variable "project_name" {
  description = "Project name, used along with `environment` as a prefix for all resources"
  type        = string
}

variable "environment" {
  description = "Environment name, used along with `project_name` as a prefix for all resources. NOT the github env"
  type        = string
}

variable "azure_location" {
  description = "Resource location"
  type        = string
}

variable "az_tag_environment" {
  description = "Environment tag to be applied to all resources"
  type        = string
}

variable "az_tag_product" {
  description = "Product tag to be applied to all resources"
  type        = string
}

variable "is_dr" {
  description = "Append dr to certain resources to build disaster recovery in new resource group. Note: most resources will have it built into project name, and not need it again."
  type        = bool
  default     = false
}

##################
# Resource Group #
##################

variable "create_rg_separately" {
  description = "Flag to create the RG before the shared module"
  type        = bool
  default     = true
}

############
# Identity #
############
variable "msi_id" {
  type        = string
  description = "The Managed Service Identity ID. If this value isn't null (the default), 'data.azurerm_client_config.current.object_id' will be set to this value."
  default     = null
  sensitive   = true
}

#############
# Azure SQL #
#############
variable "az_sql_azuread_admin_username" {
  description = "Username/email/service principal name/etc of the Azure AD account to use as admin for the SQL Server"
  type        = string
}

variable "az_sql_azuread_admin_objectid" {
  description = "Object ID for the admin listed in the 'az_sql_azuread_admin_username' variable"
  type        = string
  sensitive   = true
}

variable "az_sql_admin_password" {
  description = "Password for the admin listed in the 'az_sql_azuread_admin_username' variable"
  type        = string
  sensitive   = true
}

variable "az_sql_sku" {
  description = "What SKU/plan to use for the SQL DB"
  type        = string
  default     = "Basic"
}

variable "az_sql_max_pool_size" {
  description = "Maximum number of possible connections per SQL connection string usage"
  type        = number
  default     = 100
}

variable "az_mssql_ipv4_allow_list" {
  description = "IPv4 allow list for SQL DB"
  type = map(object({
    start_ip_range : string,
    end_ip_range : optional(string, "")
  }))
  default = {}
}

#####################
# Azure Redis Cache #
#####################
variable "redis_capacity" {
  description = "Redis cache capacity (0-6 for C and 1-5 for P)"
  type        = number
  default     = 0
}

variable "redis_family" {
  description = "Redis cache family (C for basic/standard, P for premium)"
  type        = string
  default     = "C"
}

variable "redis_sku_name" {
  description = "SKU for Redis cache (Basic, Standard or Premium)"
  type        = string
  default     = "Standard"
}

variable "redis_tls_version" {
  description = "Minimum TLS version for Redis"
  type        = string
  default     = "1.2"
}

variable "redis_public_access_enabled" {
  description = "Enable public access on the redis cache"
  type        = bool
  default     = false
}

############
# KeyVault #
############
variable "kv_use_rbac" {
  description = "create kv with rbac instead of access."
  type        = bool
  default     = false
}

variable "key_type" {
  description = "The JsonWebKeyType of the key to be created."
  default     = "RSA"
  type        = string
  validation {
    condition     = contains(["EC", "EC-HSM", "RSA", "RSA-HSM"], var.key_type)
    error_message = "The key_type must be one of the following: EC, EC-HSM, RSA, RSA-HSM."
  }
}

variable "key_ops" {
  type        = list(string)
  description = "The permitted JSON web key operations of the key to be created."
  default     = ["decrypt", "encrypt", "sign", "unwrapKey", "verify", "wrapKey"]
}

variable "key_size" {
  type        = number
  description = "The size in bits of the key to be created."
  default     = 2048
}

variable "key_vault_cidr_rules" {
  description = "A set of CIDR rules representing allowed IP addresses for network access to the keyvault s190x0x-plantech-kv"
  type        = set(string)
  default     = []
}

variable "add_secrets" {
  description = "Add secrets using TF - leave false if restoring from backup in pipeline"
  type        = bool
  default     = false
}

variable "include_kv_subnet_acl" {
  description = "Whether to add the link to the virtual network"
  type    = bool
  default = true
}

#######################
# Azure App Container #
#######################
variable "az_app_kestrel_endpoint" {
  description = "Endpoint for Kestrel setup"
  type        = string
}

variable "az_container_port" {
  description = "What port the container app is bound to"
  type        = number
  default     = 8080
}

variable "container_app_min_replicas" {
  description = "Minimum replicas for the container app"
  type        = number
  default     = 1
}

variable "container_app_max_replicas" {
  description = "Maximum replicas for the container app"
  type        = number
  default     = 2
}

variable "container_app_http_concurrency" {
  description = "Scale up at this number of HTTP requests"
  type        = number
  default     = 10
}

variable "container_environment" {
  description = "Container app environment - set to ASPNETCORE_ENVIRONMENT in the app environment variables"
  type        = string
  default     = ""

  validation {
    condition     = contains(["Dev", "Test", "Staging", "Prod"], var.container_environment)
    error_message = "container_environment must be one of ${jsonencode(["Dev", "Test", "Staging", "Prod"])}"
  }
}

variable "container_environment_variables" {
  description = "Additional environment variables to set on the Azure Container App"
  type        = map(string)
  default     = {}
}

variable "container_app_environment_workload_profile_type"  {
  description = "Container app environment type"
  type        = string
  default     = "Consumption"
}

variable "launch_in_vnet" {
  description = "Needed to force container app to be made with internal load balancer to avoid Public IP within shared module"
  type        = bool
  default     = true
}

variable "container_app_environment_internal_load_balancer_enabled" {
  description = "Force container app to be made with internal load balancer to avoid Public IP within shared module"
  type        = bool
  default     = true
}

##############################################################
# Azure Container Registry creation - settings to match shared module #
##############################################################
variable "enable_container_registry" {
  description = "Create registry within shared module"
  type        = bool
  default     = true
}

variable "registry_sku" {
  description = "Container registry sku level"
  type        = string
  default = "Standard"
}

variable "registry_admin_enabled"  {
  description = "Create admin logon on ACR"
  type        = bool
  default     = true
}

variable "registry_public_access_enabled"  {
  description = "Create public access on ACR"
  type        = bool
  default     = true
}

variable "enable_registry_retention_policy"  {
  description = "Create public access on ACR"
  type        = bool
  default     = false
}

variable "registry_retention_days"  {
  description = "registry retention days"
  type        = number
  default     = null
}

variable "registry_ipv4_allow_list" {
  description = "IPv4 allow list for ACR"
  type = map(object({
    start_ip_range : string,
    end_ip_range : optional(string, "")
  }))
  default = {}
}
#######
# DNS #
#######
variable "manage_dns_in_app_state" {
  description = "A flag to have this terraform create the DNS zone using local module & app tfstate. currently prod has dns in its own tf state file. so we will only create resources in container-app terraforming for other envs"
  type        = bool
  default     = true 
}

variable "primary_fqdn" {
  description = "The host name to use in the custom domain in front door"
  type        = string
  default     = null
}

variable "subdomains" {
  description = "A list of subdomain names to use in the dns a records"
  type        = list(string)
  default     = []
}

########################
# CDN/Front Door & DNS #
########################
#has shared container app set up OR  WAF module can add custom domain to Front Door. Set up DNS first to associate.
variable "cdn_create_custom_domain_container" {
  description = "A flag to have the custom front door domain created IN SHARED CONTAINER MODULE"
  type        = bool
  default     = false 
}

variable "cdn_create_custom_domain_waf" {
  description = "A flag to have the custom front door domain created IN SHARED WAF MODULE"
  type        = bool
  default     = true 
}

variable "cdn_frontdoor_host_add_response_headers" {
  description = "List of response headers to add at the CDN Front Door `[{ \"Name\" = \"Strict-Transport-Security\", \"value\" = \"max-age=31536000\" }]`"
  type        = list(map(string))
  default     = []
}

variable "cdn_frontdoor_url_path_redirects" {
  description = "List of url path redirects to add at the CDN Front Door `[{ \"redirect_type\": \"PermanentRedirect\", \"destination_path\": \"/example\", \"destination_hostname\": \"www.example.uk\", \"operator\": \"Equals\", \"match_values\": [\"/example\"] }]`"
  type = list(object({
    redirect_type        = string
    redirect_protocol    = optional(string)
    destination_path     = optional(string)
    destination_hostname = optional(string)
    destination_fragment = optional(string)
    query_string         = optional(string)
    operator             = string
    match_values         = optional(list(string))
    transforms           = optional(list(string))
  }))
  default = []
}

###################
# Azure Registry - existing #
###################

variable "registry_server" {
  description = "Container registry server"
  type        = string
  default     = null
}

variable "registry_username" {
  description = "Container registry username"
  type        = string
  sensitive   = true
  default     = null
}

variable "registry_password" {
  description = "Container registry password"
  type        = string
  sensitive   = true
  default     = null
}

variable "image_tag" {
  description = "Image tag"
  type        = string
  default     = null
}

####################
# Storage Accounts #
####################

variable "container_app_storage_account_shared_access_key_enabled" {
  description = "Enable shared access key"
  type        = bool
  default     = false
}

variable "container_app_blob_storage_public_access_enabled" {
  description = "Enable app blob storage public access"
  type        = bool
  default     = false
}

variable "storage_account_expiration_period" {
  description = "The SAS expiration period in format of DD.HH:MM:SS"
  type        = string
  default     = "00.01:00:00"
}

##############
# Contentful #
##############

variable "contentful_management_token" {
  description = "Contentful management token"
  type        = string
  sensitive   = true
  default     = null
}

variable "contentful_webhook_endpoint" {
  description = "Endpoint for Contentful webhook"
  type        = string
  sensitive   = false
  default     = "/api/cms/webhook"
}

variable "contentful_upsert_webhook" {
  description = "Whether to create/update the webhook or not"
  type        = bool
  default     = false
}

variable "contentful_webhook_name" {
  description = "Prefix for the Contentful webhook name"
  type        = string
  default     = "Plan Tech Webhook"
}

#########
# Misc #
#########
#the current pipeline ip to add to the key vault on creation to whitelist it
variable "workflow_runner_ip" {
  type    = string
  default = null
}
