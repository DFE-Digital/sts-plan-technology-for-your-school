locals {
  tags = {
    "Environment"      = var.environment
    "Service Offering" = var.project_name
    "Product"          = var.project_name
  }
}

resource "azurerm_resource_group" "dns-zone" {
  name     = "${var.environment_prefix}dns"
  location = var.azure_location
  tags     = local.tags
}

resource "azurerm_dns_zone" "primary" {
  name                = var.primary_fqdn
  resource_group_name = azurerm_resource_group.dns-zone.name
  tags                = local.tags
}

resource "azurerm_dns_zone" "subdomains" {
  for_each            = var.subdomains
  name                = "${each.value}.${var.primary_fqdn}"
  resource_group_name = azurerm_resource_group.dns-zone.name
  tags                = local.tags
}

resource "azurerm_dns_ns_record" "subdomains" {
  for_each            = var.subdomains
  name                = each.value
  zone_name           = azurerm_dns_zone.primary.name
  resource_group_name = azurerm_resource_group.dns-zone.name
  ttl                 = 3600
  records             = azurerm_dns_zone.subdomains[each.value].name_servers
}

resource "azurerm_dns_cname_record" "primary" {
  name                = "www"
  zone_name           = azurerm_dns_zone.primary.name
  resource_group_name = azurerm_resource_group.dns-zone.name
  ttl                 = 3600
  record              = var.frontdoor_url
}

output "resource_group_name" {
  value = azurerm_resource_group.dns-zone.name
}
output "primary-zone" {
  value = {
    "domain"       = azurerm_dns_zone.primary.name
    "name_servers" = azurerm_dns_zone.primary.name_servers
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
