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

variable "restrict_container_apps_to_cdn_inbound_only" {
  description = "Restricts access to the Container Apps by creating a network security group that only allows 'AzureFrontDoor.Backend' inbound, and attaches it to the subnet of the container app environment."
  type        = bool
  default     = true
}