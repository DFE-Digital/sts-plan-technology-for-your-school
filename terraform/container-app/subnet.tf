resource "azurerm_subnet" "private_endpoints" {
  name                 = "${local.resource_prefix}privateendpoints"
  resource_group_name  = local.resource_group_name
  virtual_network_name = local.virtual_network_name
  address_prefixes     = [local.private_endpoints_subnet_cidr]
}
