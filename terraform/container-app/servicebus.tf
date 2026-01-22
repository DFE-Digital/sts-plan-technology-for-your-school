resource "azurerm_servicebus_namespace" "service_bus" {
  name                = "${local.resource_prefix}servicebus"
  location            = local.azure_location
  resource_group_name = local.resource_group_name
  sku                 = "Basic"

  local_auth_enabled            = true
  public_network_access_enabled = true

  tags = local.tags

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.user_assigned_identity.id]
  }
}

resource "azurerm_servicebus_queue" "contentful_queue" {
  name         = "contentful"
  namespace_id = azurerm_servicebus_namespace.service_bus.id
}


resource "azurerm_servicebus_queue_authorization_rule" "azurefunction" {
  name     = "azurefunction"
  queue_id = azurerm_servicebus_queue.contentful_queue.id
  listen   = true
  send     = true
  manage   = false
}
