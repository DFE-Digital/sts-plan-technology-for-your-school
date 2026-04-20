#we only create it if using DR, otherwise the shared module will look for it
#using the standard resource prefix name anyway and create if doesn't exist.

resource "azurerm_resource_group" "app_rg" {
  count = local.is_dr ? 1 : 0

  name     = "${local.resource_prefix}"
  location = local.azure_location
  tags     = local.tags
}
