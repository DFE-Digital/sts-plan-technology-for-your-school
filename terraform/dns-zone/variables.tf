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
  default     = "UK South"
}

variable "subdomains" {
  description = "A list of subdomains that can be associated with the primary domain"
  type        = list(string)
  default     = null
}
variable "primary_fqdn" {
  description = "The fully qualified domain name for the primary dns zone"
  type        = string
}