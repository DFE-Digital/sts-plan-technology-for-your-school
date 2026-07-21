#add custom domain onto Azure Front Door here instead of using the module where the FD is created so that we can associate it to the DNS.

resource "azurerm_cdn_frontdoor_custom_domain" "custom_domain" {
  name                     = "${local.resource_prefix}-custom-domain"
  cdn_frontdoor_profile_id = var.frontdoor_profile_id
  dns_zone_id              = azurerm_dns_zone.primary.id

  host_name = var.primary_fqdn

  tls {
    certificate_type    = "ManagedCertificate"
    minimum_tls_version = "TLS12"
  }
}

resource "azurerm_cdn_frontdoor_custom_domain_association" "custom_domain_route" {
  cdn_frontdoor_custom_domain_id = azurerm_cdn_frontdoor_custom_domain.custom_domain.id
  cdn_frontdoor_route_ids        = [var.frontdoor_route_id]

  depends_on = [
    azurerm_dns_txt_record.frontdoor_validation,
    azurerm_dns_a_record.fd_domain
  ]
}
