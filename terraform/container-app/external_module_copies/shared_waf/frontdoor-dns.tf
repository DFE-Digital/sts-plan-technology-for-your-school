resource "azurerm_dns_zone" "waf" {
  for_each = local.waf_application == "CDN" && local.create_dns_zone ? local.cdn_custom_domains : {}

  name                = each.value
  resource_group_name = local.resource_group.name

  tags = local.tags
}

resource "azurerm_dns_a_record" "frontdoor" {
  for_each = local.waf_application == "CDN" && local.create_dns_zone ? local.cdn_custom_domains : {}

  name                = "@"
  zone_name           = azurerm_dns_zone.waf[each.key].name
  resource_group_name = local.resource_group.name
  ttl                 = 3600

  target_resource_id = azurerm_cdn_frontdoor_endpoint.waf[each.key].id
}

resource "azurerm_dns_txt_record" "frontdoor_validation" {
  for_each = local.waf_application == "CDN" && local.create_dns_zone ? local.cdn_custom_domains : {}

  name                = "_dnsauth"
  zone_name           = azurerm_dns_zone.waf[each.key].name
  resource_group_name = local.resource_group.name
  ttl                 = 3600

  record {
    value = azurerm_cdn_frontdoor_custom_domain.waf[each.key].validation_token
  }
}
