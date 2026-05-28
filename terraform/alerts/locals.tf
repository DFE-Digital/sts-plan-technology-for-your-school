locals {
  resource_group_scope = "/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/${var.resource_group_name}"
  scope_insights = "/providers/Microsoft.Insights/components/${var.resource_group_name}-insights"
  tags = {
    "Environment"      = var.az_tag_environment,
    "Service Offering" = var.az_tag_product,
    "Product"          = var.az_tag_product
  }
  action_group_name_by_environment = {
    productionunprotected = "${var.resource_group_name}-action-alerts"
    prod = "${var.resource_group_name}-action-alerts"
    dev  = "${var.resource_group_name}-actiongroup"
  }

  action_group_name = local.action_group_name_by_environment[lower(var.environment_github)]
}
