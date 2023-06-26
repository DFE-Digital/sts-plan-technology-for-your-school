#!/bin/bash

# exit on failures
set -e
set -o pipefail

usage() {
  echo "Usage: $(basename "$0") [OPTIONS]" 1>&2
  echo "  -h               - help"
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
while getopts "k:i:h" opt; do
  case $opt in
    k)
      KEYVAULT_NAME=$OPTARG
      ;;
    i)
      IP_ADDRESS=$OPTARG
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
  -z "$KEYVAULT_NAME" ||
  -z "$IP_ADDRESS"
]]; then
  usage
fi

echo "Commandline argument validation successful"

#########################################
# Assign user identity to container app #
#########################################
echo "Removing IP Address from KeyVault..."
az keyvault network-rule remove --name $KEYVAULT_NAME --ip-address $IP_ADDRESS