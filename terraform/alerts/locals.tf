locals {
    scopes = [
      "/subscriptions/${var.subscription_id}/resourceGroups/${var.resource_group_name}"
    ]
  tags = {
    "Environment"      = var.az_tag_environment,
    "Service Offering" = var.az_tag_product,
    "Product"          = var.az_tag_product
  }
  action_group_name_by_environment = {
    productionunprotected = "${var.project_name}-action-alerts"
    prod = "${var.project_name}-action-alerts"
    dev  = "${var.project_name}-actiongroup"
  }

  action_group_name = local.action_group_name_by_environment[lower(var.environment)]
}
