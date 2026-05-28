
output "container_app_environment_id" {
description = "Id of the used Container App environment - either existing or created"
  value = (
    local.existing_container_app_environment.name == ""
    ? azurerm_container_app_environment.container_app_env[0].id
    : data.azurerm_container_app_environment.existing_container_app_environment[0].id
  )
}
