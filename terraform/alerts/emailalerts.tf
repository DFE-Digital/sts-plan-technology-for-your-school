data "azurerm_client_config" "current" {}

resource "azurerm_monitor_action_group" "this" {
  name                = var.action_group_name
  resource_group_name = var.resource_group_name
  short_name          = var.action_group_short_name
  location            = "global"
  enabled             = var.enabled
  tags                = var.tags

  arm_role_receiver {
    name                    = var.arm_role_receiver_name
    role_id                 = var.arm_role_receiver_role_id
    use_common_alert_schema = var.use_common_alert_schema
  }
}

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
    action_group_id    = azurerm_monitor_action_group.this.id
    webhook_properties = {}
  }

  criteria {
    category       = each.value.category
    operation_name = each.value.operation_name
    statuses       = each.value.statuses
  }
}
