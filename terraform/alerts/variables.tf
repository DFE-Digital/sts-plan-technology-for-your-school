variable "resource_group_name" {
  type = string
}

variable "environment_github" {
  type = string
  default = "Dev"
}
variable "subscription_id" {
  type = string
}

variable "enabled" {
  type    = bool
  default = true
}

variable "azure_location" {
  description = "Resource location"
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

variable "use_common_alert_schema" {
  type    = bool
  default = false
}

variable "activity_log_alerts" {
  type = map(object({
    name           = string
    description    = string
    category       = string
    operation_name = string
    statuses       = optional(list(string), [])
  }))
}
