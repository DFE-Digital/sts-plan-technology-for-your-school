resource "azurerm_application_insights" "functional_insights" {
  name                = "${local.resource_prefix}-function-insights"
  location            = local.azure_location
  resource_group_name = local.resource_group_name
  application_type    = "web"
  retention_in_days   = 30
  tags                = local.tags
}