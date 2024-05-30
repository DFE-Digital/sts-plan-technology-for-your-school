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
while getopts "g:s:l:p:e:h" opt; do
  case $opt in
    g)
      RESOURCE_GROUP_NAME=$OPTARG
      ;;
    l)
      AZ_LOCATION=$OPTARG
      ;;
    a)
      APP_SERVICE_PLAN_NAME=$OPTARG
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
    h)
      usage
      exit;;
    *)
      usage
      exit;;
  esac
done

# Validate calues exist
if [[
  -z "$RESOURCE_GROUP_NAME" ||
  -z "$APPSERVICE_PLAN_NAME"
  -z "$AZ_LOCATION" ||
  -z "$FUNCTION_APP_NAME" ||
  -z "$STORAGE_ACCOUNT_NAME" ||
  -z "$AZ_ENVIRONMENT" ||
  -z "$AZ_PRODUCT"
]]; then
  usage
fi

#If the app service plan exists and is already on the flex consumption plan, then exit early
APPSERVICE_PLAN=$(az appservice plan show --resource-group s190d01-plantech --name s190d01-plantechappserviceplan | jq '.sku.size')

if ["$APPSERVICE_PLAN" == "FC1"]
  exit 0
fi

#Delete old FA + app service plan, then recreate
az functionapp delete --name $FUNCTION_APP_NAME --resource-group $RESOURCE_GROUP_NAME
az appservice plan delete --name $APPSERVICE_PLAN_NAME --resource-group $RESOURCE_GROUP_NAME

az appservice plan create --name $APPSERVICE_PLAN_NAME --resource-group $RESOURCE_GROUP_NAME --sku FC1
az functionapp create --resource-group $RESOURCE_GROUP_NAME --name $FUNCTION_APP_NAME -p $APPSERVICE_PLAN_NAME --storage-account $STORAGE_ACCOUNT_NAME --flexconsumption-location $AZ_LOCATION --runtime dotnet-isolated --runtime-version 8.0 --tags Environment=$AZ_ENVIRONMENT "Service Offering"="$AZ_PRODUCT" Product="$AZ_PRODUCT"