resource "azurerm_user_assigned_identity" "user_assigned_identity" {
  depends_on          = [module.main_hosting]
  
  name                = local.user_identity_name
  location            = local.azure_location
  resource_group_name = local.resource_group_name
}
