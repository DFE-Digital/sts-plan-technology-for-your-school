resource "azurerm_virtual_network" "default" {
  count = local.create_virtual_network ? 1 : 0

  name                = "${local.resource_prefix}default"
  address_space       = [local.virtual_network_address_space]
  location            = local.resource_group.location
  resource_group_name = local.resource_group.name
  tags                = local.tags
}

resource "azurerm_route_table" "default" {
  count = local.create_virtual_network ? 1 : 0

  name                = "${local.resource_prefix}default"
  location            = local.resource_group.location
  resource_group_name = local.resource_group.name
  tags                = local.tags
}

resource "azurerm_virtual_network_peering" "source_to_origin" {
  for_each = local.virtual_network_peering_targets

  name                      = "Vnet-${azurerm_virtual_network.default[0].name}-To-Vnet-${data.azurerm_virtual_network.vnet[each.key].name}"
  resource_group_name       = local.resource_group.name
  virtual_network_name      = azurerm_virtual_network.default[0].name
  remote_virtual_network_id = data.azurerm_virtual_network.vnet[each.key].id
}
