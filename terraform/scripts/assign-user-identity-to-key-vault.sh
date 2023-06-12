#!/bin/bash

# exit on failures
set -e
set -o pipefail

usage() {
  echo "Usage: $(basename "$0") [OPTIONS]" 1>&2
  echo "  -h               - help"
  echo "  -n               - container app environment name"
  echo "  -g               - resource group name"
  echo "  -u               - user identity name"

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
while getopts "n:g:u:h" opt; do
  case $opt in
    n)
      CONTAINER_APP_NAME=$OPTARG
      ;;
    g)
      RESOURCE_GROUP_NAME=$OPTARG
      ;;
    u)
      USER_IDENTITY_NAME=$OPTARG
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
  -z "$CONTAINER_APP_NAME" ||
  -z "$RESOURCE_GROUP_NAME" ||
  -z "$USER_IDENTITY_NAME"
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
  CONTAINER_APP=$(az containerapp show --name "$CONTAINER_APP_NAME" --resource-group "$RESOURCE_GROUP_NAME")
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
echo "Updating container app with user managed identity ..."
az containerapp identity assign -n "$CONTAINER_APP_NAME" -g "$RESOURCE_GROUP_NAME" --user-assigned "$USER_IDENTITY_NAME"