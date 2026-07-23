resource "azurerm_resource_group" "dns-zone" {
  count    = var.existing_resource_group == "" ? 1 : 0
  name     = local.dns_rg_name
  location = var.azure_location
  tags     = local.tags
}

resource "azurerm_dns_zone" "primary" {
  name                = var.primary_fqdn
  resource_group_name = local.rg_name
  tags                = local.tags
}

resource "azurerm_dns_a_record" "fd_domain" {
  name                = "@"
  zone_name           = azurerm_dns_zone.primary.name
  resource_group_name = local.rg_name
  ttl                 = 3600

  target_resource_id = var.frontdoor_endpoint_id
}

resource "azurerm_dns_txt_record" "frontdoor_validation" {
  name                = "_dnsauth"
  zone_name           = azurerm_dns_zone.primary.name
  resource_group_name = local.rg_name
  ttl                 = 3600

  record {
    value = azurerm_cdn_frontdoor_custom_domain.custom_domain.validation_token
  }
}

# we don't have a cname currently.
#resource "azurerm_dns_cname_record" "primary" {
#  name                = "www"
#  zone_name           = azurerm_dns_zone.primary.name
#  resource_group_name = local.rg_name
#  ttl                 = 3600
#  record              = var.primary_fqdn
#}


#usually, a subdomain would be another a record, but prod is set up to have staging as a separate dns zone
#and a full other domain name pointing at the 02 front door. so don't use this for that.
resource "azurerm_dns_a_record" "subdomain" {
  for_each            = var.subdomains
  name                = each.value
  zone_name           = azurerm_dns_zone.primary.name
  resource_group_name = local.rg_name
  tags                = local.tags
  ttl                 = 3600
}
