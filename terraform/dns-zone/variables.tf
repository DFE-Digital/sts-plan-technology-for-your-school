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

variable "az_tag_environment" {
  description = "Environment tag to be applied to all resources"
  type        = string
}

variable "az_tag_product" {
  description = "Product tag to be applied to all resources"
  type        = string
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

variable "is_dr" {
  description = "used to build tf filenames. Note: most resources will have it built into project name, and not need it again."
  type        = bool
  default     = false
}
