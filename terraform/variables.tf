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