#we only create it if using DR, otherwise the shared module will look for it
#using the standard resource prefix name anyway and create if doesn't exist.

resource "azurerm_resource_group" "app_rg" {
  count = local.is_dr ? 1 : 0

  name     = "${local.resource_prefix}-dr"
  location = local.azure_location
  tags     = local.tags
}

resource "azurerm_management_lock" "app_rg" {
  count = local.enable_resource_group_lock && local.is_dr ? 1 : 0

  name       = "${local.resource_prefix}-drlock"
  scope      = azurerm_resource_group.app_rg[0].id
  lock_level = "CanNotDelete"
  notes      = "Resources in this Resource Group cannot be deleted. Please remove the lock first."
}
