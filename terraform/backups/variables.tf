###########
# General #
###########
variable "project_name" {
  description = "Project name, used as a prefix for all resources"
  type        = string
}

variable "environment" {
  description = "Environment name, used as a prefix for all resources"
  type        = string
}

variable "azure_location" {
  description = "Azure region for backup resources"
  type        = string
  default     = "westeurope"
}

variable "az_tag_environment" {
  description = "Environment tag applied to all resources"
  type        = string
}

variable "az_tag_product" {
  description = "Product tag applied to all resources"
  type        = string
}

####################
# Backup-specific  #
####################
variable "github_actions_principal_id" {
  description = "Principal ID of the GitHub Actions managed identity that writes backups"
  type        = string
  sensitive   = true
}

variable "blob_delete_retention_days" {
  description = "Soft delete retention period for blobs (days)"
  type        = number
  default     = 14
}

variable "container_delete_retention_days" {
  description = "Soft delete retention period for containers (days)"
  type        = number
  default     = 14
}

variable "immutability_enabled" {
  description = "Whether to apply a container-level immutability policy"
  type        = bool
  default     = false
}

variable "immutability_period_days" {
  description = "Days to retain blobs under immutability policy"
  type        = number
  default     = 7
}
