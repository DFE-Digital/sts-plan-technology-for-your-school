data "azurerm_sql_server" "database" {
  name                = local.resource_prefix
  resource_group_name = local.resource_group_name
}
