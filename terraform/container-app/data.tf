#created in the shared container app module, to use in shared waf for AFD private link. output was suggested in PR but
#not merged, so use data here instead.

data "azurerm_container_app_environment" "env" {
  name                = "${local.resource_prefix}containerapp"
  resource_group_name = local.resource_prefix
}
