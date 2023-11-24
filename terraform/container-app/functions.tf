resource "azurerm_storage_account" "function_storage" {
  name                     = replace("${local.resource_prefix}funcstr", "-", "")
  resource_group_name      = local.resource_group_name
  location                 = local.azure_location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  public_network_access_enabled = true
  shared_access_key_enabled     = true

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.user_assigned_identity.id]
  }
}

resource "azurerm_service_plan" "function_plan" {
  name                = "${local.resource_prefix}appserviceplan"
  resource_group_name = local.resource_group_name
  location            = local.azure_location
  os_type             = "Linux"
  sku_name            = "Y1"
}

resource "azurerm_linux_function_app" "contentful_function" {
  name                = "${local.resource_prefix}contentfulfunction"
  resource_group_name = local.resource_group_name
  location            = local.azure_location

  service_plan_id = azurerm_service_plan.function_plan.id

  storage_account_access_key = azurerm_storage_account.function_storage.primary_access_key
  storage_account_name       = azurerm_storage_account.function_storage.name

  site_config {
    api_management_api_id = azurerm_api_management_api.contentful_api.id
    application_stack {
      dotnet_version              = "7.0"
      use_dotnet_isolated_runtime = true
    }
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.user_assigned_identity.id]
  }

}
