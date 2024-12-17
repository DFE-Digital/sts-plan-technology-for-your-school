resource "azurerm_resource_group" "dns_zone" {
  count = local.dns.enable_dns ? 1 : 0

  name     = local.dns.resource_group_name
  location = local.azure_location
  tags     = local.tags
}

resource "azurerm_dns_zone" "primary" {
  depends_on = [module.waf]

  count = local.dns.enable_dns ? 1 : 0

  name                = local.dns.primary_fqdn
  resource_group_name = azurerm_resource_group.dns_zone[0].name
  tags                = local.tags
}

resource "azurerm_dns_zone" "subdomains" {
  for_each = local.dns.enable_dns == true ? local.dns.subdomains : []

  name                = "${each.value}.${local.dns.primary_fqdn}"
  resource_group_name = local.dns.enable_dns ? azurerm_resource_group.dns_zone[0].name : ""
  tags                = local.tags
}

resource "azurerm_dns_ns_record" "subdomains" {
  for_each = local.dns.enable_dns == true ? local.dns.subdomains : []

  name                = each.value
  zone_name           = local.dns.enable_dns ? azurerm_dns_zone.primary[0].name : ""
  resource_group_name = local.dns.enable_dns ? azurerm_resource_group.dns_zone[0].name : local.dns.resource_group_name
  ttl                 = 3600
  records             = azurerm_dns_zone.subdomains[each.value].name_servers
}

resource "azurerm_dns_cname_record" "primary" {
  count = local.dns.enable_dns == true ? 1 : 0

  name                = "www"
  zone_name           = local.dns.enable_dns ? azurerm_dns_zone.primary[0].name : ""
  resource_group_name = local.dns.enable_dns ? azurerm_resource_group.dns_zone[0].name : local.dns.resource_group_name
  ttl                 = 3600
  record              = module.main_hosting.container_fqdn
}
