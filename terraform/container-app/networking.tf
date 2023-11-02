resource "azurerm_subnet" "service_bus_subnet" {
  name                 = "${local.resource_prefix}servicebusinfra"
  virtual_network_name = "${local.resource_prefix}default"
  resource_group_name  = local.resource_group_name
  address_prefixes     = [cidrsubnet("172.16.0.0/12", 23 - element(split("/", "172.16.0.0/12"), 1), 6)]

  service_endpoints = ["Microsoft.ServiceBus"]
}

resource "azurerm_subnet" "function_subnet" {
  name                 = "${local.resource_prefix}functioninfra"
  virtual_network_name = "${local.resource_prefix}default"
  resource_group_name  = local.resource_group_name
  address_prefixes     = [cidrsubnet("172.16.0.0/12", 23 - element(split("/", "172.16.0.0/12"), 1), 7)]

  service_endpoints = ["Microsoft.Storage"]
}
