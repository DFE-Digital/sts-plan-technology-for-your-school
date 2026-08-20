output "frontdoor_profile_id" {
  value = module.waf.frontdoor_profile_id
}

output "frontdoor_route_id" {
  value = module.waf.frontdoor_route_id
}

output "frontdoor_endpoint_id" {
  value = module.waf.frontdoor_endpoint_id
}

output "frontdoor_endpoint_host_name" {
  value = module.waf.frontdoor_endpoint_host_name
}

output "cdn_hostname" {
  value = local.cdn_hostname
}
