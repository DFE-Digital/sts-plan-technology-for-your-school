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
  description = "The region in which all azure resources should be created"
  type        = string
  default     = "West Europe"
}

variable "tags" {
  description = "Tags to be applied to all resources"
  type        = map(string)
  default     = {}
}

variable "existing_resource_group" {
  description = "Specifying this will NOT create a resource group. Use if running on non-prod and putting DNS in app RG"
  type        = string
  default     = ""
}

variable "subdomains" {
  description = "A list of subdomains that can be associated with the primary domain"
  type        = set(string)
  default     = []
}

variable "primary_fqdn" {
  description = "The fully qualified domain name for the primary dns zone"
  type        = string
  default     = ""
}

variable "frontdoor_endpoint_host_name" {
  description = "The created fd endpoint to the app: hostname"
  type        = string
  default     = ""
}
variable "frontdoor_profile_id" {
  description = "The created fd"
  type        = string
  default     = ""
}
variable "frontdoor_route_id" {
  description = "The created fd route to the app: id"
  type        = string
  default     = ""
}
