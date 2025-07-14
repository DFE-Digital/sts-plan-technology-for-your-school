resource "azurerm_redis_cache" "redis" {
  name                          = local.resource_prefix
  location                      = local.azure_location
  resource_group_name           = local.resource_group_name
  capacity                      = local.redis_capacity
  family                        = local.redis_family
  sku_name                      = local.redis_sku_name
  minimum_tls_version           = local.redis_tls_version
  tags                          = local.tags
  public_network_access_enabled = false
}
