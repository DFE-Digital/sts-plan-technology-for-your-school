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

resource "azurerm_monitor_scheduled_query_rules_alert_v2" "key_vault_expiry" {
  count = var.enabled ? 1 : 0

  name                = "Key Vault expiry approaching"
  description         = "Alert when Key Vault secrets or keys are due to expire within ${var.key_vault_expiry_alert_days} days"
  resource_group_name = var.resource_group_name
  location            = var.azure_location
  tags                = local.tags

  evaluation_frequency = "PT1H"
  window_duration      = "PT1H"
  scopes               = [local.key_vault_log_analytics_workspace_id]
  severity             = 2

  criteria {
    query = <<-QUERY
      AzureDiagnostics
      | where ResourceProvider == "MICROSOFT.KEYVAULT"
      | where Category == "AuditEvent"
      | where operationName_s in~ ("SecretSet", "SecretUpdate", "KeyCreate", "KeyUpdate")
      | extend props = parse_json(properties_s)
      | extend expirationDate = todatetime(tostring(props.expirationDate))
      | where isnotempty(expirationDate)
      | where expirationDate between (now() .. now() + ${var.key_vault_expiry_alert_days}d)
      | summarize AlertCount = count()
    QUERY

    time_aggregation_method = "Count"
    threshold               = 0
    operator                = "GreaterThan"

    failing_periods {
      minimum_failing_periods_to_trigger_alert = 1
      number_of_evaluation_periods             = 1
    }
  }

  action {
    action_groups = [data.azurerm_monitor_action_group.existing.id]
  }
}
