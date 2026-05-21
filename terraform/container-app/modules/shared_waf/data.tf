data "azurerm_client_config" "current" {}

data "azurerm_resource_group" "existing_resource_group" {
  count = local.existing_resource_group == "" ? 0 : 1

  name = local.existing_resource_group
}

data "azurerm_virtual_network" "existing_virtual_network" {
  count = local.existing_virtual_network == "" ? 0 : 1

  name                = local.existing_virtual_network
  resource_group_name = local.existing_resource_group
}

data "azurerm_virtual_network" "vnet" {
  for_each = local.virtual_network_peering_targets

  name                = each.value.name
  resource_group_name = each.value.resource_group_name
}

#created in the shared container app module, to use in shared waf for AFD private link. output was suggested in PR but
#not merged, so use data here instead.

data "azurerm_container_app_environment" "env" {
  name                = "${local.resource_prefix}containerapp"
  resource_group_name = local.resource_prefix
}
