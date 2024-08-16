resource "azurerm_private_dns_zone" "database" {
  name                = local.az_sql_vnet.dns_zone_name
  resource_group_name = local.resource_group_name
  tags                = local.tags
}

resource "azurerm_private_endpoint" "database" {
  custom_network_interface_name = local.az_sql_vnet.dns_zone_name
  location                      = local.azure_location
  name                          = local.az_sql_vnet.endpoint_name
  resource_group_name           = local.resource_group_name
  subnet_id                     = module.main_hosting.networking.subnet_id
  tags                          = local.tags

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [azurerm_private_dns_zone.database.id]
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = local.az_sql_vnet.endpoint_name
    private_connection_resource_id = data.azurerm_sql_server.database.id
    subresource_names              = ["sqlServer"]
  }
}

resource "azurerm_private_dns_zone_virtual_network_link" "database_default" {
  name                  = "default_vnet"
  resource_group_name   = local.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.database.name
  virtual_network_id    = module.main_hosting.networking.vnet_id
}

resource "azurerm_private_dns_zone_virtual_network_link" "database_function" {
  name                  = "function_vnet"
  resource_group_name   = local.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.database.name
  virtual_network_id    = azurerm_virtual_network.function_vnet.id
}
