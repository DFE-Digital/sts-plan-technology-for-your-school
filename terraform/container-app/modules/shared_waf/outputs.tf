output "frontdoor_endpoint_host_name" {
  value = azurerm_cdn_frontdoor_endpoint.waf["container-app-url"].host_name
}
