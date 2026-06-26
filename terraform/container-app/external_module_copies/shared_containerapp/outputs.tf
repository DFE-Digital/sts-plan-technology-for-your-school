
output "container_app_environment_id" {
description = "Id of the used Container App environment - either existing or created"
  value = (
    local.existing_container_app_environment.name == ""
    ? azurerm_container_app_environment.container_app_env[0].id
    : data.azurerm_container_app_environment.existing_container_app_environment[0].id
  )
}
output "azurerm_resource_group_default" {
  value       = local.existing_resource_group == "" ? azurerm_resource_group.default[0] : null
  description = "Default Azure Resource Group"
}

output "azurerm_log_analytics_workspace_container_app" {
  value       = azurerm_log_analytics_workspace.container_app
  description = "Container App Log Analytics Workspace"
}

output "azurerm_eventhub_container_app" {
  value       = local.enable_event_hub ? azurerm_eventhub.container_app[0] : null
  description = "Container App Event Hub"
}

#output "azurerm_dns_zone_name_servers" {
#  value       = local.enable_dns_zone ? azurerm_dns_zone.default[0].name_servers : null
#  description = "Name servers of the DNS Zone"
#}

output "azurerm_container_registry" {
  value       = local.enable_container_registry ? azurerm_container_registry.acr[0] : null
  description = "Container Registry"
}

output "azurerm_virtual_network" {
  value       = local.virtual_network
  description = "Virtual Network"
}

output "networking" {
  value = local.launch_in_vnet ? {
    vnet_id : local.existing_virtual_network == "" ? azurerm_virtual_network.default[0].id : null
    subnet_id : azurerm_subnet.container_apps_infra_subnet[0].id
  } : null
  description = "IDs for various VNet resources if created"
}

output "container_fqdn" {
  description = "FQDN for the Container App"
  value       = local.container_fqdn
}

output "container_app_managed_identity" {
  description = "User-Assigned Managed Identity assigned to the Container App"
  value       = local.registry_use_managed_identity ? azurerm_user_assigned_identity.containerapp[0] : null
}

output "container_app_environment_ingress_ip" {
  description = "Ingress IP address assigned to the Container App environment"
  value       = local.container_app_environment.static_ip_address
}

output "vnet_id" {
  value = local.virtual_network.id
}

output "vnet_name" {
  value = local.virtual_network.name
}

output "route_table_id" {
  value = try(azurerm_route_table.default[0].id, null)
}
