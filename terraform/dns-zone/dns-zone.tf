locals {
  tags = {
    "Environment"      = var.environment
    "Service Offering" = var.project_name
    "Product"          = var.project_name
  }
}

resource "azurerm_resource_group" "dns-zone" {
  name     = "${var.project_name}-dns-zone"
  location = var.azure_region
}

resource "azurerm_dns_zone" "primary" {
  name                = var.primary_fqdn
  resource_group_name = azurerm_resource_group.dns-zone.name
  tags                = local.tags
}

resource "azurerm_dns_zone" "subdomains" {
  for_each            = var.subdomains
  name                = each.value
  resource_group_name = azurerm_resource_group.dns-zone.name
  tags                = locals.tags
}

resource "azurerm_dns_ns_record" "subdomains" {
  for_each            = azurerm_dns_zone.subdomains
  zone_name           = azurerm_dns_zone.primary.name
  resource_group_name = azurerm_resource_group.dns-zone.name
  ttl                 = 60
  records             = each.value.name_servers
}
