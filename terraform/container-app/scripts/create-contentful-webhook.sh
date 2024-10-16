#!/bin/bash
set -e
set -o pipefail

usage() {
  echo "Usage: $(basename "$0") [OPTIONS]" 1>&2
  echo "  -h                 - help"
  echo "  --env-id           - Contentful environment id"
  echo "  --env-name         - Azure environment name"
  echo "  --management-token - Contentful management token"
  echo "  --space-id         - Contentful space ID"
  echo "  --webhook-api-key  - API key for webhook URL"
  echo "  --webhook-name     - Webhook name prefix"
  echo "  --webhook-url      - Webhook URL"

  exit 1
}

##########################
# Command line arguments #
##########################

# Validate that command line arguments exist
if [[ $# -lt 1 ]]; then
  usage
fi

declare -A args_map=(
  ["--env-id"]="ENVIRONMENT_ID"
  ["--env-name"]="ENVIRONMENT_NAME"
  ["--management-token"]="MANAGEMENT_TOKEN"
  ["--space-id"]="SPACE_ID"
  ["--webhook-api-key"]="WEBHOOK_API_KEY"
  ["--webhook-name"]="WEBHOOK_NAME"
  ["--webhook-url"]="WEBHOOK_URL"
)

# Get command line arguments
while [[ $# -gt 0 ]]; do
  case $1 in
    -h|--help)
      usage
      ;;
    *)
      if [[ -n "${args_map[$1]}" ]]; then
        eval "export ${args_map[$1]}=\"$2\""
        shift 2
      else
        echo "Unknown option: $1"
        usage
      fi
      ;;
  esac
done

# Validate values exist
if [[ -z "$ENVIRONMENT_ID" || -z "$MANAGEMENT_TOKEN" || -z "$ENVIRONMENT_NAME" || -z "$SPACE_ID" || -z "$WEBHOOK_API_KEY" || -z "$WEBHOOK_NAME" || -z "$WEBHOOK_URL" ]]; then
  usage
fi

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd "$SCRIPT_DIR/../../../contentful/webhook-creator"
echo "Building Contentful webhooks project"
npm run build

echo "Running webhook script"
node ./dist/create-contentful-webhook.js
echo "Finished webhook run"
