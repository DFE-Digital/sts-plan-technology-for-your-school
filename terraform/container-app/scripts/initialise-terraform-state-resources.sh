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
  echo "  -g               - resource group name"
  echo "  -l               - Azure location"
  echo "  -s               - Object id for Service principal"
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
    s)
      OBJ_ID=$OPTARG
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
  -z "$AZ_LOCATION" ||
  -z "$OBJ_ID" ||
  -z "$AZ_ENVIRONMENT" ||
  -z "$AZ_PRODUCT"
]]; then
  usage
fi

# Use the same name as the resource group but without the '-'s in it
STORAGE_ACCOUNT_NAME=$(echo "$RESOURCE_GROUP_NAME" | sed 's/-//g')state

CONTAINER_NAME=tfstate
KEYVAULT_NAME=$RESOURCE_GROUP_NAME-kv
AZ_TAGS=("Product=$AZ_PRODUCT" "Environment=$AZ_ENVIRONMENT" "Service Offering=$AZ_PRODUCT")

echo "Commandline argument validation successful"

#############################################
# Create resources to store terraform state #
#############################################
echo "Creating Azure resources required for use with Terraform state"

# Create resources required for TF State
az group create --name $RESOURCE_GROUP_NAME --location $AZ_LOCATION --tags "${AZ_TAGS[@]}"
az storage account create --resource-group $RESOURCE_GROUP_NAME --name $STORAGE_ACCOUNT_NAME --sku Standard_LRS --encryption-services blob --tags "${AZ_TAGS[@]}"
az storage container create --name $CONTAINER_NAME --account-name $STORAGE_ACCOUNT_NAME

echo "Created key resources. Creating Azure Keyvault"

# Create KV to store account key
az keyvault create --name $KEYVAULT_NAME --resource-group $RESOURCE_GROUP_NAME --location $AZ_LOCATION --enable-rbac-authorization false --no-self-perms true --tags "${AZ_TAGS[@]}"

echo "Retrieving Azure Storge account access key, and saving to Keyvault"

# Retrieve storage account access key
ACCOUNT_KEY=$(az storage account keys list --resource-group $RESOURCE_GROUP_NAME --account-name $STORAGE_ACCOUNT_NAME --query '[0].value' -o tsv)

# Save access key as KeyVault secret
az keyvault set-policy -n $KEYVAULT_NAME --secret-permissions set --object-id $OBJ_ID
az keyvault secret set --name storage-account-key --vault-name $KEYVAULT_NAME --value $ACCOUNT_KEY
