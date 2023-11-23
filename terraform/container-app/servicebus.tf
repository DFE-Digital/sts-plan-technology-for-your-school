resource "azurerm_servicebus_namespace" "service_bus" {
  name                = "${local.resource_prefix}servicebus"
  location            = local.azure_location
  resource_group_name = local.resource_group_name
  sku                 = "Basic"

  local_auth_enabled            = false
  public_network_access_enabled = false

  tags = local.tags

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.user_assigned_identity.id]
  }
}
