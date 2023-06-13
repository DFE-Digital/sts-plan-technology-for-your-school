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

variable "cdn_frontdoor_origin_host_header_override" {
  description = "Override the frontdoor origin host header"
  type        = string
}

variable "az_sql_admin_password" {
  description = "Azure SQL admin password"
  type        = string
}