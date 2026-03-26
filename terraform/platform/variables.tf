variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "acr_name" {
  description = "Name of the Azure Container Registry"
  type        = string
}

variable "environment" {
  description = "Environment name (Dev, Tst, etc.)"
  type        = string
}

variable "product" {
  description = "Product tag"
  type        = string
}

variable "costings_name" {
  description = "Name of the Azure Storage Acc for costings"
  type        = string
}
