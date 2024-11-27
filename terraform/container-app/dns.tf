resource "azurerm_resource_group" "dns-zone" {
  count = local.dns.enable_dns ? 1 : 0

  name     = "${local.environment}dns"
  location = var.azure_location
  tags     = local.tags
}

resource "azurerm_dns_zone" "primary" {
  depends_on = [module.waf]

  count = local.dns.enable_dns ? 1 : 0

  name                = local.dns.primary_fqdn
  resource_group_name = azurerm_resource_group.dns-zone[0].name
  tags                = local.tags
}

resource "azurerm_dns_zone" "subdomains" {
  for_each            = local.dns.enable_dns == true ? local.dns.subdomains : []
  name                = "${each.value}.${var.primary_fqdn}"
  resource_group_name = azurerm_resource_group.dns-zone[0].name
  tags                = local.tags
}

resource "azurerm_dns_ns_record" "subdomains" {
  for_each            = local.dns.enable_dns == true ? local.dns.subdomains : []
  name                = each.value
  zone_name           = azurerm_dns_zone.primary[0].name
  resource_group_name = azurerm_resource_group.dns-zone[0].name
  ttl                 = 3600
  records             = azurerm_dns_zone.subdomains[each.value].name_servers
}

resource "azurerm_dns_cname_record" "primary" {
  name                = "www"
  zone_name           = azurerm_dns_zone.primary[0].name
  resource_group_name = azurerm_resource_group.dns-zone[0].name
  ttl                 = 3600
  record              = module.main_hosting.container_fqdn
}

output "resource_group_name" {
  value = azurerm_resource_group.dns-zone[0].name
}

output "primary-zone" {
  value = {
    "domain"       = azurerm_dns_zone.primary[0].name
    "name_servers" = azurerm_dns_zone.primary[0].name_servers
  }
}

output "subdomain-zones" {
  value = [
    for subdomain in azurerm_dns_zone.subdomains : {
      "domain"       = subdomain.name
      "name_servers" = subdomain.name_servers
    }
  ]
}
