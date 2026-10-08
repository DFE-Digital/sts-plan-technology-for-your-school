# if running in non-prod, DNS goes into the app RG so we need to look it up here.

data "azurerm_resource_group" "existing_resource_group" {
  count = local.existing_resource_group == "" ? 0 : 1

  name = local.existing_resource_group
}
