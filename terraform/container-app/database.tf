data "azurerm_mssql_server" "database" {
  depends_on = [module.main_hosting]
  name                = local.resource_prefix
  resource_group_name = local.resource_group_name
}
