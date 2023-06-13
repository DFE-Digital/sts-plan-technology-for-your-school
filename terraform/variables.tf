variable "project_name" {
  description = ""
  type        = string
}

variable "environment" {
  description = "Environment name. Will be used along with `project_name` as a prefix for all resources."
  type        = string
}

variable "azure_location" {
  description = ""
  type        = string
}

variable "az_tag_environment" {
  description = ""
  type        = string
}

variable "az_tag_product" {
  description = ""
  type        = string
}

variable "cdn_frontdoor_origin_host_header_override" {
  description = "Override the frontdoor origin host header"
  type        = string
}

variable "msi_id" {
  type        = string
  description = "The Managed Service Identity ID. If this value isn't null (the default), 'data.azurerm_client_config.current.object_id' will be set to this value."
  default     = null
}