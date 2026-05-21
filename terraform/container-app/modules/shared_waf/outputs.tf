output "frontdoor_endpoint_host_name" {
  value = azurerm_cdn_frontdoor_endpoint.waf[0].host_name
}
