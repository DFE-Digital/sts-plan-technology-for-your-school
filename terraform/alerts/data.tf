data "azurerm_monitor_action_group" "existing" {
  name                = local.action_group_name
  resource_group_name = var.resource_group_name
}
