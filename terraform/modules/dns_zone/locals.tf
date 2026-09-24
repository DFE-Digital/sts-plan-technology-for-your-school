locals {
  ###########
  # General #
  ###########
  #current_user_id     = coalesce(var.msi_id, data.azurerm_client_config.current.object_id)
  azure_location  = var.azure_location
  resource_prefix = "${var.environment}${var.project_name}"

  tags = var.tags

  # Resource Group
  existing_resource_group = var.existing_resource_group
  dns_rg_name             = "${var.environment}${var.project_name}-dns"
  rg_name                 = var.existing_resource_group == "" ? azurerm_resource_group.dns-zone[0].name : data.azurerm_resource_group.existing_resource_group[0].name
}
