module "dns_zone" {
  source = "../modules/dns_zone"

  environment    = var.environment
  project_name   = var.project_name
  azure_location = var.azure_location
  tags           = local.tags
  primary_fqdn   = var.primary_fqdn
  subdomains     = var.subdomains

  frontdoor_profile_id         = data.terraform_remote_state.app.outputs.frontdoor_profile_id
  frontdoor_route_id           = data.terraform_remote_state.app.outputs.frontdoor_route_id
  frontdoor_endpoint_host_name = data.terraform_remote_state.app.outputs.frontdoor_endpoint_host_name
}
