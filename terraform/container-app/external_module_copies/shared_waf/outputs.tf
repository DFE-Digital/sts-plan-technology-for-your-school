output "frontdoor_profile_id" {
  value = azurerm_cdn_frontdoor_profile.waf[0].id
}

output "frontdoor_endpoint_host_name" {
  value = azurerm_cdn_frontdoor_endpoint.waf["container-app-url"].host_name
}

output "frontdoor_route_id" {
  value = azurerm_cdn_frontdoor_route.waf["container-app-url"].id
}

output "frontdoor_endpoint_id" {
  value = azurerm_cdn_frontdoor_endpoint.waf["container-app-url"].id
}
