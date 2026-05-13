locals {
  tags = {
    "Environment"      = var.az_tag_environment
    "Service Offering" = var.az_tag_product
    "Product"          = var.az_tag_product
    "Purpose"          = "sql-backups"
    "ManagedBy"        = "terraform"
  }
  keyvaultname = "${var.backup_resource_group_name}-kv"
}
