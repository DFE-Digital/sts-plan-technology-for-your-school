resource "azurerm_virtual_network" "function_vnet" {
  name                = local.function.vnet.name
  address_space       = [local.function.vnet.address_space]
  location            = local.function.location
  resource_group_name = local.resource_group_name
  tags                = local.tags
}

resource "azurerm_subnet" "function_infra_subnet" {
  name                 = local.function.vnet.subnets.infra.name
  virtual_network_name = local.function.vnet.name
  resource_group_name  = local.resource_group_name
  address_prefixes     = local.function.vnet.subnets.infra.address_prefixes

  service_endpoints = ["Microsoft.KeyVault", "Microsoft.Storage"]

  delegation {
    name = "AFADelegationService"

    service_delegation {
      actions = [
        "Microsoft.Network/virtualNetworks/subnets/join/action"
      ]
      name = "Microsoft.App/environments"
    }
  }
}

resource "azurerm_subnet" "function_storage" {
  name                 = local.function.vnet.subnets.storage.name
  virtual_network_name = azurerm_virtual_network.function_vnet.name
  resource_group_name  = local.resource_group_name
  address_prefixes     = local.function.vnet.subnets.storage.address_prefixes

}

resource "azurerm_private_dns_zone" "blob_storage" {
  name                = local.function.vnet.dns.blob.name
  resource_group_name = local.resource_group_name
  tags                = local.tags
}

resource "azurerm_private_dns_zone" "files_storage" {
  name                = local.function.vnet.dns.files.name
  resource_group_name = local.resource_group_name
  tags                = local.tags
}

resource "azurerm_private_dns_zone" "keyvault" {
  name                = local.function.vnet.dns.keyvault.name
  resource_group_name = local.resource_group_name
  tags                = local.tags
}

resource "azurerm_private_endpoint" "blob_storage" {
  custom_network_interface_name = local.function.vnet.endpoints.blob.nic_name
  location                      = local.function.location
  name                          = local.function.vnet.endpoints.blob.name
  resource_group_name           = local.resource_group_name
  subnet_id                     = azurerm_subnet.function_storage.id
  tags                          = local.tags

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [azurerm_private_dns_zone.blob_storage.id]
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = local.function.vnet.endpoints.blob.name
    private_connection_resource_id = azurerm_storage_account.function_storage.id
    subresource_names              = ["blob"]
  }
}

resource "azurerm_private_dns_zone_virtual_network_link" "blob_storage" {
  name                  = "function_vn"
  resource_group_name   = local.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.blob_storage.name
  virtual_network_id    = azurerm_virtual_network.function_vnet.id
}

resource "azurerm_private_endpoint" "files_storage" {
  custom_network_interface_name = local.function.vnet.endpoints.files.nic_name
  location                      = local.function.location
  name                          = local.function.vnet.endpoints.files.name
  resource_group_name           = local.resource_group_name
  subnet_id                     = azurerm_subnet.function_storage.id
  tags                          = local.tags

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [azurerm_private_dns_zone.files_storage.id]
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = local.function.vnet.endpoints.files.name
    private_connection_resource_id = azurerm_storage_account.function_storage.id
    subresource_names              = ["file"]
  }
}

resource "azurerm_private_dns_zone_virtual_network_link" "files_storage" {
  name                  = "function_vn"
  resource_group_name   = local.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.files_storage.name
  virtual_network_id    = azurerm_virtual_network.function_vnet.id
}

resource "azurerm_private_endpoint" "keyvault" {
  custom_network_interface_name = local.function.vnet.endpoints.keyvault.nic_name
  location                      = local.function.location
  name                          = local.function.vnet.endpoints.keyvault.name
  resource_group_name           = local.resource_group_name
  subnet_id                     = azurerm_subnet.function_storage.id
  tags                          = local.tags

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [azurerm_private_dns_zone.keyvault.id]
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = local.function.vnet.endpoints.vault.name
    private_connection_resource_id = azurerm_key_vault.vault.id
    subresource_names              = ["vault"]
  }
}

resource "azurerm_private_dns_zone_virtual_network_link" "keyvault_to_functionvnet" {
  name                  = "function_vn"
  resource_group_name   = local.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.keyvault.name
  virtual_network_id    = azurerm_virtual_network.function_vnet.id
}

resource "azurerm_private_dns_zone_virtual_network_link" "keyvault_to_defaultvnet" {
  name                  = "default_vn"
  resource_group_name   = local.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.keyvault.name
  virtual_network_id    = module.main_hosting.networking.vnet_id
}

resource "azurerm_virtual_network_peering" "function_to_main" {
  name                      = "${local.resource_prefix}-defaultvn-peer"
  resource_group_name       = local.resource_group_name
  virtual_network_name      = azurerm_virtual_network.function_vnet.name
  remote_virtual_network_id = module.main_hosting.networking.vnet_id

  allow_forwarded_traffic = true
}

resource "azurerm_virtual_network_peering" "main_to_function" {
  name                      = "${local.resource_prefix}-functionvn-peer"
  resource_group_name       = local.resource_group_name
  virtual_network_name      = "${local.resource_prefix}default"
  remote_virtual_network_id = azurerm_virtual_network.function_vnet.id

  allow_forwarded_traffic = true
}
