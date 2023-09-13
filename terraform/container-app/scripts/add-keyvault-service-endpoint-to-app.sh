#!/bin/bash

# exit on failures
set -e
set -o pipefail

usage() {
  echo "Usage: $(basename "$0") [OPTIONS]" 1>&2
  echo "  -h               - help"
  echo "  -n               - Subnet name"
  echo "  -g               - Resource Group name"
  echo "  -v               - Virtual Network resource name"
  echo "  -c               - Container App name"
  echo "  -k               - Keyvault Name"
  echo "  -i               - GitHub Runner IP Address"

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
while getopts "n:g:v:c:k:h" opt; do
  case $opt in
    n)
      SUBNET_NAME=$OPTARG
      ;;
    g)
      RESOURCE_GROUP_NAME=$OPTARG
      ;;
    v)
      VNET_NAME=$OPTARG
      ;;
    c) 
      CONTAINER_NAME=$OPTARG
      ;;
    k)
      KEYVAULT_NAME=$OPTARG
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
  -z "$SUBNET_NAME" ||
  -z "$RESOURCE_GROUP_NAME" ||
  -z "$VNET_NAME" ||
  -z "$CONTAINER_NAME" ||
  -z "$KEYVAULT_NAME"
  ]]; then
  usage
fi

echo "Commandline argument validation successful"

############################################
# Wait for container app to be provisioned #
############################################
CONTAINER_APP_PROVISIONING_STATE="InProgress"
#sleep 10
while [ "$CONTAINER_APP_PROVISIONING_STATE" == "InProgress" ]
do
  CONTAINER_APP=$(az containerapp show --name "$CONTAINER_NAME" --resource-group "$RESOURCE_GROUP_NAME")
  CONTAINER_APP_PROVISIONING_STATE=$(echo "$CONTAINER_APP" | jq -r ".properties.provisioningState")
  if [[
    "$CONTAINER_APP_PROVISIONING_STATE" != "InProgress" &&
    "$CONTAINER_APP_PROVISIONING_STATE" != "Succeeded"
  ]]
  then
    echo "Failed, Container App Environment is '$CONTAINER_APP_PROVISIONING_STATE'"
    exit 1
  fi
  if [ "$CONTAINER_APP_PROVISIONING_STATE" == "Succeeded" ]
  then
    break
  fi
  echo "Waiting for container app environment to be provisioned ..."
  sleep 5
done

#########################################
# Assign user identity to container app #
#########################################
echo "Adding Microsoft.KeyVault Service Endpoint to Subnet..."
az network vnet subnet update -n "$SUBNET_NAME" -g "$RESOURCE_GROUP_NAME" --vnet-name "$VNET_NAME" --service-endpoints Microsoft.KeyVault
az keyvault network-rule add --name $KEYVAULT_NAME --resource-group $RESOURCE_GROUP_NAME --vnet-name $VNET_NAME --subnet $SUBNET_NAME