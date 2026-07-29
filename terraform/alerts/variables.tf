variable "az_tag_environment" {
  description = "Environment tag to be applied to all resources"
  type        = string
}

variable "az_tag_product" {
  description = "Product tag to be applied to all resources"
  type        = string
}

variable "azure_location" {
  description = "Resource location"
  type        = string
}

variable "enabled" {
  type    = bool
  default = true
}

variable "environment_github" {
  type    = string
  default = "Dev"
}

variable "key_vault_expiry_alert_days" {
  description = "How many days ahead to raise an alert for Key Vault items that are due to expire"
  type        = number
  default     = 30
}

variable "resource_group_name" {
  type = string
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
    level          = string
  }))
}

variable "metric_alerts" {
  type = map(object({
    name        = string
    description = optional(string)
    severity    = optional(number, 3)

    criteria = optional(object({
      metric_namespace       = optional(string)
      metric_name            = string
      aggregation            = string
      operator               = string
      threshold              = number
      skip_metric_validation = optional(bool, false)

      dimension = optional(list(object({
        name     = string
        operator = string
        values   = list(string)
      })), [])
    }))

    dynamic_criteria = optional(object({
      metric_namespace         = string
      metric_name              = string
      aggregation              = string
      operator                 = string
      alert_sensitivity        = string
      evaluation_total_count   = optional(number, 4)
      evaluation_failure_count = optional(number, 4)
      ignore_data_before       = optional(string)
      skip_metric_validation   = optional(bool, false)

      dimension = optional(list(object({
        name     = string
        operator = string
        values   = list(string)
      })), [])
    }))
  }))
}
