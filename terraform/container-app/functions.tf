resource "azurerm_storage_account" "function_storage" {
  name                     = "${local.resource_prefix}appservicestorage"
  resource_group_name      = local.resource_group_name
  location                 = local.azure_location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  public_network_access_enabled = false
  shared_access_key_enabled     = false

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.user_assigned_identity.id]
  }
}

resource "azurerm_storage_account_network_rules" "app_service_storage_network_rules" {
  storage_account_id = azurerm_storage_account.function_storage.id

  default_action             = "Deny"
  virtual_network_subnet_ids = [module.main_hosting.networking.subnet_id]
  bypass                     = ["Logging", "Metrics"]
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

  storage_account_name          = azurerm_storage_account.function_storage.name
  storage_uses_managed_identity = true
  service_plan_id               = azurerm_service_plan.function_plan.id

  virtual_network_subnet_id = azurerm_subnet.function_subnet.name

  site_config {}

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.user_assigned_identity.id]
  }
}
