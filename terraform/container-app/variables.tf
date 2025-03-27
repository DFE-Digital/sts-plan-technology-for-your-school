###########
# General #
###########
variable "project_name" {
  description = "project name, used along with `environment` as a prefix for all resources"
  type        = string
}

variable "environment" {
  description = "Environment name, used along with `project_name` as a prefix for all resources"
  type        = string
}

variable "azure_location" {
  description = "Recourse location"
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

variable "mssql_firewall_ipv4_allow_list" {
  description = "A list of IPv4 Addresses that require remote access to the MSSQL Server"
  type        = list(string)
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

############
# KeyVault #
############
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

##################
# CDN/Front Door #
##################
variable "cdn_create_custom_domain" {
  description = "A flag to create the A and TXT records for the container app as part of setting up the cdn"
  type        = bool
  default     = false
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
# Github Registry #
###################

variable "registry_server" {
  description = "Container registry server"
  type        = string
}

variable "registry_username" {
  description = "Container registry username"
  type        = string
  sensitive   = true
}

variable "registry_password" {
  description = "Container registry password"
  type        = string
  sensitive   = true
}

variable "image_tag" {
  description = "Image tag"
  type        = string
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
