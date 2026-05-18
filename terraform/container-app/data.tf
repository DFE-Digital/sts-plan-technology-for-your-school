# only needed if not using container app shared module as that already contains it. use to pull user id and subscription id.
#data "azurerm_client_config" "current" {}


#created in the shared container app module, to use in shared waf for AFD private link. output was suggested in PR but
#not merged, so use data here instead.

data "azurerm_container_app_environment" "env" {
  name                = "${local.resource_prefix}containerapp"
  resource_group_name = local.resource_prefix
}
