variable "dfe_project_name" {
  description = ""
  type        = string
}

variable "az_environment" {
  description = "Environment name. Will be used along with `project_name` as a prefix for all resources."
  type        = string
}

variable "az_location" {
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

variable "tf_state_container_name" {
  description = ""
  type        = string
}

variable "tf_state_container_name" {
  description = ""
  type        = string
}

variable "tf_state_key" {
  description = ""
  type        = string
}

variable "tf_state_storage_account_name" {
  description = ""
  type        = string
}
