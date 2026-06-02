resource "azurerm_private_dns_zone" "database" {
  count = local.create_db_network ? 1 : 0
  name                = local.az_sql_vnet.dns_zone_name
  resource_group_name = local.resource_group_name
  tags                = local.tags
}

resource "azurerm_private_endpoint" "database" {
  count = local.create_db_network ? 1 : 0
  custom_network_interface_name = local.az_sql_vnet.dns_zone_name
  location                      = local.azure_location
  name                          = local.az_sql_vnet.endpoint_name
  resource_group_name           = local.resource_group_name
  subnet_id                     = module.main_hosting.networking.subnet_id
  tags                          = local.tags

  private_dns_zone_group {
    count = local.create_db_network ? 1 : 0
    name                 = "default"
    private_dns_zone_ids = [azurerm_private_dns_zone.database[0].id]
  }

  private_service_connection {
    count = local.create_db_network ? 1 : 0
    is_manual_connection           = false
    name                           = local.az_sql_vnet.endpoint_name
    private_connection_resource_id = data.azurerm_mssql_server.database.id
    subresource_names              = ["sqlServer"]
  }
}

resource "azurerm_private_dns_zone_virtual_network_link" "database_default" {
  count = local.create_db_network ? 1 : 0
  name                  = "default_vnet"
  resource_group_name   = local.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.database[0].name
  virtual_network_id    = module.main_hosting.networking.vnet_id

  tags = local.tags
}
