locals {
  resource_prefix = "${var.environment}${var.project_name}"

  backup_resource_group_name  = "${local.resource_prefix}-sql-backups"
  backup_storage_account_name = "${replace(local.resource_prefix, "-", "")}sqlbackups"
  backup_container_name       = "bacpacs"

  tags = {
    "Environment"      = var.az_tag_environment
    "Service Offering" = var.az_tag_product
    "Product"          = var.az_tag_product
    "Purpose"          = "sql-backups"
    "ManagedBy"        = "terraform"
  }
}
