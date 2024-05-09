resource "azurerm_user_assigned_identity" "user_assigned_identity" {
  name                = local.user_identity_name
  location            = local.azure_location
  resource_group_name = module.main_hosting.azurerm_resource_group_default.name
}

resource "azurerm_role_assignment" "uami_storage_data_owner_role" {
  scope                = azurerm_storage_account.function_storage.id
  role_definition_name = "Storage Blob Data Owner"
  principal_id         = azurerm_user_assigned_identity.user_assigned_identity.principal_id
}