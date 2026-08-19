output "resource_group_name" {
  value = length(azurerm_resource_group.dns-zone) > 0 ? azurerm_resource_group.dns-zone[0].name : var.existing_resource_group
}
output "primary-zone" {
  value = {
    "domain"       = azurerm_dns_zone.primary.name
    "name_servers" = azurerm_dns_zone.primary.name_servers
  }
}

#output "subdomain-zones" {
#  value = [
#    for subdomain in azurerm_dns_zone.subdomains : {
#      "domain"       = subdomain.name
#      "name_servers" = subdomain.name_servers
#    }
#  ]
#}
