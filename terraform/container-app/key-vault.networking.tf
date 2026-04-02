data "azurerm_route_table" "default" {
  depends_on = [module.main_hosting]
  count = local.has_route_table ? 1 : 0
  name                = "${local.resource_prefix}default"
  resource_group_name = local.resource_group_name
}

resource "azurerm_subnet" "keyvault" {
  name                 = local.kv_networking.subnet.name
  virtual_network_name = "${local.resource_prefix}default"
  resource_group_name  = local.resource_group_name
  address_prefixes     = local.kv_networking.subnet.address_prefixes
}

resource "azurerm_subnet_route_table_association" "keyvault" {
  count = local.has_route_table ? 1 : 0
  subnet_id      = azurerm_subnet.keyvault.id
  route_table_id = data.azurerm_route_table.default[0].id
}

resource "azurerm_private_dns_zone" "keyvault" {
  name                = local.kv_networking.private_dns_zone.name
  resource_group_name = local.resource_group_name
  tags                = local.tags
}

resource "azurerm_private_endpoint" "keyvault" {
  custom_network_interface_name = local.kv_networking.endpoint.nic_name
  location                      = local.azure_location
  name                          = local.kv_networking.endpoint.name
  resource_group_name           = local.resource_group_name
  subnet_id                     = azurerm_subnet.keyvault.id
  tags                          = local.tags

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [azurerm_private_dns_zone.keyvault.id]
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = local.kv_networking.endpoint.name
    private_connection_resource_id = azurerm_key_vault.vault.id
    subresource_names              = ["vault"]
  }
}

resource "azurerm_private_dns_zone_virtual_network_link" "keyvault_to_defaultvnet" {
  name                  = "default_vnet"
  resource_group_name   = local.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.keyvault.name
  virtual_network_id    = module.main_hosting.networking.vnet_id
  tags                  = local.tags
}
