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
}

##############
# Front Door #
##############
variable "cdn_frontdoor_origin_host_header_override" {
  description = "Override the frontdoor origin host header"
  type        = string
}

#############
# Azure SQL #
#############
variable "az_sql_admin_password" {
  description = "Azure SQL admin password"
  type        = string
}

variable "az_sql_admin_userid_postfix" {
  description = "Azure SQL admin userid postfix, used with `project_name` and `environment` to build userid"
  type        = string
}

#######################
# Azure App Container #
#######################
variable "az_app_kestrel_endpoint" {
  description = "Endpoint for Kestrel setup"
  type        = string
}
