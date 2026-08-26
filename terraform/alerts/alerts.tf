data "azurerm_client_config" "current" {}

resource "azurerm_monitor_activity_log_alert" "this" {
  for_each = var.activity_log_alerts

  name                = each.value.name
  description         = each.value.description
  resource_group_name = var.resource_group_name
  location            = var.azure_location
  scopes              = [local.resource_group_scope]
  enabled             = var.enabled
  tags                = local.tags


  action {
    action_group_id = data.azurerm_monitor_action_group.existing.id
  }

  criteria {
    category       = each.value.category
    operation_name = each.value.operation_name
    statuses       = each.value.statuses
    level          = each.value.level
  }
}

resource "azurerm_monitor_metric_alert" "this" {
  for_each = var.metric_alerts

  name                     = each.value.name
  description              = each.value.description
  resource_group_name      = var.resource_group_name
  target_resource_location = var.azure_location
  enabled                  = var.enabled
  tags                     = local.tags
  severity                 = each.value.severity
  scopes                   = ["${local.resource_group_scope}${local.scope_insights}"]
  window_size              = each.value.dynamic_criteria == null ? "PT15M" : "PT5M"

  action {
    action_group_id = data.azurerm_monitor_action_group.existing.id
  }

  dynamic "criteria" {
    for_each = each.value.criteria == null ? [] : [each.value.criteria]

    content {
      metric_namespace       = criteria.value.metric_namespace
      metric_name            = criteria.value.metric_name
      aggregation            = criteria.value.aggregation
      operator               = criteria.value.operator
      threshold              = criteria.value.threshold
      skip_metric_validation = criteria.value.skip_metric_validation

      dynamic "dimension" {
        for_each = criteria.value.dimension
        content {
          name     = dimension.value.name
          operator = dimension.value.operator
          values   = dimension.value.values
        }
      }
    }
  }

  dynamic "dynamic_criteria" {
    for_each = each.value.dynamic_criteria == null ? [] : [each.value.dynamic_criteria]

    content {
      metric_namespace         = dynamic_criteria.value.metric_namespace
      metric_name              = dynamic_criteria.value.metric_name
      aggregation              = dynamic_criteria.value.aggregation
      operator                 = dynamic_criteria.value.operator
      alert_sensitivity        = dynamic_criteria.value.alert_sensitivity
      evaluation_total_count   = dynamic_criteria.value.evaluation_total_count
      evaluation_failure_count = dynamic_criteria.value.evaluation_failure_count
      ignore_data_before       = dynamic_criteria.value.ignore_data_before != "" ? dynamic_criteria.value.ignore_data_before : null
      skip_metric_validation   = dynamic_criteria.value.skip_metric_validation
    }
  }
}

resource "azurerm_monitor_scheduled_query_rules_alert_v2" "this" {
  for_each = var.enabled ? var.scheduled_query_alerts : {}

  name                = each.value.name
  description         = each.value.description
  resource_group_name = var.resource_group_name
  location            = var.azure_location
  tags                = local.tags

  evaluation_frequency = each.value.evaluation_frequency
  window_duration      = each.value.window_duration
  scopes               = length(each.value.scopes) > 0 ? each.value.scopes : [local.key_vault_log_analytics_workspace_id]
  severity             = each.value.severity

  criteria {
    query                   = replace(each.value.query, "__EXPIRY_DAYS__", tostring(var.key_vault_expiry_alert_days))
    time_aggregation_method = each.value.time_aggregation_method
    threshold               = each.value.threshold
    operator                = each.value.operator

    failing_periods {
      minimum_failing_periods_to_trigger_alert = each.value.minimum_failing_periods_to_trigger_alert
      number_of_evaluation_periods             = each.value.number_of_evaluation_periods
    }
  }

  action {
    action_groups = [data.azurerm_monitor_action_group.existing.id]
  }
}
