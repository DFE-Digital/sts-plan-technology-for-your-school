resource "azurerm_private_dns_zone" "redis" {
  name                = local.redis_vnet.dns_zone_name
  resource_group_name = local.resource_group_name
  tags                = local.tags
}

resource "azurerm_private_endpoint" "redis" {
  custom_network_interface_name = local.redis_vnet.nic_name
  location                      = local.azure_location
  name                          = local.redis_vnet.endpoint_name
  resource_group_name           = local.resource_group_name
  subnet_id                     = module.main_hosting.networking.subnet_id
  tags                          = local.tags

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [azurerm_private_dns_zone.redis.id]
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = local.redis_vnet.endpoint_name
    private_connection_resource_id = azurerm_redis_cache.redis.id
    subresource_names              = ["redisCache"]
  }
}

resource "azurerm_private_dns_zone_virtual_network_link" "redis_default" {
  name                  = "default_vnet"
  resource_group_name   = local.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.redis.name
  virtual_network_id    = module.main_hosting.networking.vnet_id

  tags = local.tags
}
