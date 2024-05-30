#!/bin/bash

###############################################################################################################################################
# DESCRIPTION                                                                                                                                 #
###############################################################################################################################################
# This script creates the Azure resources required to store Terraform state in.                                                               #
# Specifically: a Resource Group, a Storage Account, and a Container within that Storage Account                                              #
# Afterwards, it creates an Azure KeyVault, and saves the Storage Account's access key there as "storage-account-key"                         #
###############################################################################################################################################
## CREATED RESOURCE NAMES                                                                                                                     #
###############################################################################################################################################
## Resource Group:            Matches CLI argument                                                                                            #
## Storage account:           The resource group name but without "-". E.g. "resource-group-name" would be come "resourcegroupname"           #
## Storage account container: tfstate                                                                                                         #
## Keyvault:                  The resource group name with "-kv" at the end. E.g. "resource-group-name" would become "resource-group-name-kv" #
## Keyvault secret:           storage-account-key                                                                                             #
###############################################################################################################################################

# exit on failures
set -e
set -o pipefail

usage() {
  echo "Usage: $(basename "$0") [OPTIONS]" 1>&2
  echo "  -h               - help"
  echo "  -g               - Azure resource group name"
  echo "  -l               - Azure location (e.g. 'westeurope')"
  echo "  -a               - App Service Plan name"
  echo "  -f               - Name of Function App to create"
  echo "  -s               - Name of the Storage Account for the Function App to use"
  echo "  -p               - Product"
  echo "  -e               - Environment"
  echo "  -i               - User assigned managed identity ID"

  exit 1
}

##########################
# Command line arguments #
##########################

# Validate that commandline arguments exist
if [ $# -lt 1 ];
then
 usage
fi

# Get commandline arguments
while getopts "g:l:a:f:s:p:e:i:h" opt; do
  case $opt in
    g)
      RESOURCE_GROUP_NAME=$OPTARG
      ;;
    l)
      AZ_LOCATION=$OPTARG
      ;;
    a)
      APPSERVICE_PLAN_NAME=$OPTARG
      ;;
    f)
      FUNCTION_APP_NAME=$OPTARG
      ;;
    s)
      STORAGE_ACCOUNT_NAME=$OPTARG
      ;;
    e)
      AZ_ENVIRONMENT=$OPTARG
      ;;
    p)
      AZ_PRODUCT=$OPTARG
      ;;
    i)
      MI_ID=$OPTARG
      ;;
    h)
      usage
      exit;;
    *)
      usage
      exit;;
  esac
done

# Validate arg values exist
if [[
  -z "$RESOURCE_GROUP_NAME" ||
  -z "$APPSERVICE_PLAN_NAME" ||
  -z "$AZ_LOCATION" ||
  -z "$FUNCTION_APP_NAME" ||
  -z "$STORAGE_ACCOUNT_NAME" ||
  -z "$AZ_ENVIRONMENT" ||
  -z "$AZ_PRODUCT"
]]; then
  usage
fi

get_app_service_plan() {
    az appservice plan list --resource-group "$RESOURCE_GROUP_NAME" | jq '.[] | select(.name != "'$APPSERVICE_PLAN_NAME'")'
}

check_app_service_plan_SKU() {
    local APPSERVICE_PLAN="$1"
    local APPSERVICE_PLAN_NAME=$(echo "$APPSERVICE_PLAN" | jq -r '.name')
    local APPSERVICE_PLAN_SKU=$(echo "$APPSERVICE_PLAN" | jq -r '.sku.size')

    if [ "$APPSERVICE_PLAN_SKU" != "FC1" ]; then
        echo "Deleting $APPSERVICE_PLAN_NAME as SKU is $APPSERVICE_PLAN_SKU"
        az appservice plan delete --name "$APPSERVICE_PLAN_NAME" --resource-group "$RESOURCE_GROUP_NAME" --yes
        echo "Deleted plan"
    fi
}

get_function_app() {
    az functionapp list -g "$RESOURCE_GROUP_NAME" | jq '.[]'
}

check_function_app() {
    local FUNCTION_APP="$1"
    local APPSERVICE_PLAN="$2"

    if [ -z "$FUNCTION_APP" ]; then
        echo "No Function App - moving to create"
    else
        local FUNCTION_APP_SERVICE_PLAN=$(echo "$FUNCTION_APP" | jq -r '.appServicePlanId')
        local APPSERVICE_PLAN_id=$(echo "$APPSERVICE_PLAN" | jq -r '.id')

        if [ "$FUNCTION_APP_SERVICE_PLAN" != "$APPSERVICE_PLAN_id" ]; then
            local existing_function_app_NAME=$(echo "$FUNCTION_APP" | jq -r '.name')
            echo "Not using App Service plan - deleting $existing_function_app_NAME"
            az functionapp delete --resource-group "$RESOURCE_GROUP_NAME" --name "$existing_function_app_NAME"
        else
            echo "Function app exists and uses correct App Service plan; exiting"
            exit 0
        fi
    fi
}

create_function_app(){
  az functionapp create --resource-group $RESOURCE_GROUP_NAME \
                      --name $FUNCTION_APP_NAME \
                      --storage-account $STORAGE_ACCOUNT_NAME \
                      --flexconsumption-location northeurope \
                      --runtime dotnet-isolated \
                      --runtime-version 8.0 \
                      --functions-version 4 \
                      --assign-identity $MI_ID \
                      --https-only true \
                      --tags Environment=$AZ_ENVIRONMENT "Service Offering"="$AZ_PRODUCT" Product="$AZ_PRODUCT" 
}

main() {
    local APPSERVICE_PLAN=$(get_app_service_plan)

    if [ -n "$APPSERVICE_PLAN" ]; then
        check_app_service_plan_SKU "$APPSERVICE_PLAN"
    else
        echo "No app service plan"
    fi

    local function_app=$(get_function_app)
    check_function_app "$FUNCTION_APP" "$APPSERVICE_PLAN"
}

main
