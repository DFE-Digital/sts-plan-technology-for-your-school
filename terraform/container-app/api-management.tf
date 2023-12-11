resource "azurerm_api_management" "api_management" {
  name                = "${local.resource_prefix}-apim"
  location            = local.azure_location
  resource_group_name = local.resource_group_name
  publisher_name      = "DFE Digital"
  publisher_email     = "dfe-digital@education.gov.uk"

  tags     = local.tags
  sku_name = "Consumption_0"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.user_assigned_identity.id]
  }
}


resource "azurerm_api_management_api" "contentful_api" {
  name                = "contentful"
  resource_group_name = local.resource_group_name
  api_management_name = azurerm_api_management.api_management.name

  revision     = "1"
  display_name = "Contentful Functions"
  path         = "contentful"
  protocols    = ["https"]
}
