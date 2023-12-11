resource "azurerm_api_management" "api_management" {
  name                = "${local.resource_prefix}-apim"
  location            = local.azure_location
  resource_group_name = local.resource_group_name
  publisher_name      = "DFE Digital"
  publisher_email     = "dfe-digital@education.gov.uk"

  sku_name = "Consumption_0"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.user_assigned_identity.id]
  }
}

resource "azurerm_api_management_backend" "apim_backend" {
  name                = "${local.resource_prefix}-contentful-content-updates"
  resource_group_name = local.resource_group_name
  api_management_name = azurerm_api_management.api_management.name
  protocol            = "http"
  url                 = "https://${azurerm_linux_function_app.contentful_function.name}.azurewebsites.net/api/"
}
