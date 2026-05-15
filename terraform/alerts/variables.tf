variable "resource_group_name" {
  type = string
}

variable "subscription_id" {
  type = string
}

variable "enabled" {
  type    = bool
  default = true
}

variable "environment" {
  description = "Environment name, used along with `project_name` as a prefix for all resources"
  type        = string
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

variable "action_group_name" {
  type = string
}

variable "action_group_short_name" {
  type = string
}

variable "arm_role_receiver_name" {
  type    = string
  default = "Contributor"
}

variable "arm_role_receiver_role_id" {
  type    = string
  default = "b24988ac-6180-42a0-ab88-20f7382dd24c"
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
