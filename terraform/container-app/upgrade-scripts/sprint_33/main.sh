#!/usr/bin/env bash


usage() {
  echo "Usage: $(basename "$0") [OPTIONS]" 1>&2
  echo "  -h               - help"
  echo "  -g               - Resource group name"
  echo "  -t               - Terraform variables file path"

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
while getopts "g:t:h" opt; do
  case $opt in
    g)
      RESOURCE_GROUP_NAME=$OPTARG
      ;;
    t)
      TFVAR_PATH=$OPTARG
      ;;
    h)
      usage
      exit;;
    *)
      usage
      exit;;
  esac
done

# Validate required arguments exist
if [[
  -z "$RESOURCE_GROUP_NAME" ||
  -z "$TFVAR_PATH"
]]; then
  usage
fi

export PARENT_DIR=$( cd $(dirname $0)"/../"; pwd -P )

export RESOURCE_GROUP_NAME
export TFVAR_PATH
export PARENT_DIR

bash "$(dirname "$0")/import_keyvault_secrets.sh"