###########
# General #
###########
variable "subscription_id" {
  description = "The subscription ID for the environment against which Terraform is running"
  type        = string
}

variable "project_name" {
  description = "Project name, used as a prefix for all resources"
  type        = string
}

variable "github_actions_principal_id" {
  description = "Principal ID of the GitHub Actions managed identity that writes backups"
  type        = string
  sensitive   = true
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

variable "backup_resource_group_name" {
  description = "Resource group name outside of the main service environments in which to store backups"
  type        = string
  sensitive   = false
}

variable "backup_storage_account_name" {
  description = "Backups storage account name"
  type        = string
  sensitive   = false
}

variable "backup_container_name" {
  description = "Backup container name"
  type        = string
  sensitive   = false
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

variable "tags" {
  description = "Product and service offering tags"
  type = object({
    Product         = string
    ServiceOffering = string
  })
  default = {
    Product         = "Plan Technology for your School"
    ServiceOffering = "Plan Technology for your School"
  }
}
