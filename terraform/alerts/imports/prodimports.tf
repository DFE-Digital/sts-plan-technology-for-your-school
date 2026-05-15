import {
  to = module.activity_alerts.azurerm_monitor_activity_log_alert.this["container_app_environment_change"]
  id = "/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/${var.resource_group_name}/providers/Microsoft.Insights/activityLogAlerts/Container App Environment Change"
}

import {
  to = module.activity_alerts.azurerm_monitor_activity_log_alert.this["container_app_image_pushed"]
  id = "/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/${var.resource_group_name}/providers/Microsoft.Insights/activityLogAlerts/Container App Image Pushed"
}

import {
  to = module.activity_alerts.azurerm_monitor_activity_log_alert.this["container_app_revision_active"]
  id = "/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/${var.resource_group_name}/providers/Microsoft.Insights/activityLogAlerts/Container App Revision Active"
}

import {
  to = module.activity_alerts.azurerm_monitor_activity_log_alert.this["container_app_revision_deactivation_succeeded"]
  id = "/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/${var.resource_group_name}/providers/Microsoft.Insights/activityLogAlerts/Container App Revision Deactivation Succeeded"
}

import {
  to = module.activity_alerts.azurerm_monitor_activity_log_alert.this["container_app_update_succeeded"]
  id = "/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/${var.resource_group_name}/providers/Microsoft.Insights/activityLogAlerts/Container App Update Succeeded"
}
