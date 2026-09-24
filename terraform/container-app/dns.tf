#dns module no longer called from central TF, but the shared waf version is used - as it needs to be coupled with the FD creation

#module "dns" {
#  source = "../modules/dns_zone"
#  count = var.manage_dns_in_app_state ? 1 : 0
#  depends_on     = [module.main_hosting, module.waf]
#
#  environment    = local.environment
#  project_name   = local.project_name
#  azure_location = local.azure_location
#  tags           = local.tags
#  primary_fqdn   = var.primary_fqdn
#  subdomains     = var.subdomains
#  existing_resource_group = local.resource_group_name
#  frontdoor_profile_id         = module.waf.frontdoor_profile_id
#  frontdoor_route_id           = module.waf.frontdoor_route_id
#  frontdoor_endpoint_id = module.waf.frontdoor_endpoint_id
#}
