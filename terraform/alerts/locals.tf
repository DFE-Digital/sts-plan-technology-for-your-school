locals {
    scopes = [
      "/subscriptions/${var.subscription_id}/resourceGroups/${var.resource_group_name}"
    ]
  tags = {
    "Environment"      = var.az_tag_environment,
    "Service Offering" = var.az_tag_product,
    "Product"          = var.az_tag_product
  }
}
