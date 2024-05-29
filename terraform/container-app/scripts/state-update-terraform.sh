#!/bin/bash

if [ "$#" -ne 6 ]; then
  echo "Usage: $0 <VAR_FILE> <SUBSCRIPTION_ID> <RESOURCE_GROUP> <STATE_CONTAINER> <STATE_FILE> <STATE_ACCOUNT>"
  exit 1
fi

VAR_FILE="$1"
SUBSCRIPTION_ID="$2"
RESOURCE_GROUP="$3"
STATE_CONTAINER="$4"
STATE_FILE="$5"
STATE_ACCOUNT="$6"
RESOURCE_GROUP_PREFIX="${RESOURCE_GROUP%%-plantech}"

terraform import -var-file="$VAR_FILE" module.main_hosting.azurerm_container_app_environment.container_app_env "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.App/managedEnvironments/${RESOURCE_GROUP}containerapp"

terraform state rm module.main_hosting.azapi_resource.container_app_env

terraform import -var-file="$VAR_FILE" 'module.main_hosting.azurerm_container_app.container_apps["main"]' "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.App/containerApps/${RESOURCE_GROUP}-plan-tech-app"

terraform state rm module.main_hosting.azapi_resource.default

az storage blob download -f currentstate.tfstate -c $STATE_CONTAINER -n $STATE_FILE --account-name $STATE_ACCOUNT

jq --arg analyticsid "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.OperationalInsights/workspaces/$RESOURCE_GROUP_PREFIX-plantechcontainerapp" '.resources |= map(if .type == "azurerm_container_app_environment" then .instances[].attributes.log_analytics_workspace_id |= $analyticsid else . end)' currentstate.tfstate > newstate.tfstate

az storage blob upload -f newstate.tfstate -c $STATE_CONTAINER -n $STATE_FILE --account-name $STATE_ACCOUNT --overwrite true