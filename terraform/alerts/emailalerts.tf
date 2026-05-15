data "azurerm_client_config" "current" {}

resource "azurerm_monitor_activity_log_alert" "this" {
  for_each = var.activity_log_alerts

  name                = each.value.name
  description         = each.value.description
  resource_group_name = var.resource_group_name
  location            = "global"
  scopes              = local.scopes
  enabled             = var.enabled
  tags                = var.tags

  action {
    action_group_id = data.azurerm_monitor_action_group.existing.id
  }

  criteria {
    category       = each.value.category
    operation_name = each.value.operation_name
    statuses       = each.value.statuses
  }
}
