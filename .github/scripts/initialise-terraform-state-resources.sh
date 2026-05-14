#!/bin/bash

set -euo pipefail

usage() {
  echo "Usage: $(basename "$0") [OPTIONS]"
  echo "  -g   Resource group name"
  echo "  -l   Azure location"
  echo "  -s   Client ID (service principal / OIDC identity)"
  echo "  -p   Product"
  echo "  -e   Environment"
  echo "  -k   Key Vault name"
  echo "  -a   Storage account name"
  echo "  -c   Container name"
  exit 1
}

while getopts "g:s:l:p:e:k:a:c:h" opt; do
  case $opt in
    g) RESOURCE_GROUP_NAME=$OPTARG ;;
    l) AZ_LOCATION=$OPTARG ;;
    s) CLIENT_ID=$OPTARG ;;
    e) AZ_ENVIRONMENT=$OPTARG ;;
    p) AZ_PRODUCT=$OPTARG ;;
    k) KEYVAULT_NAME=$OPTARG ;;
    a) STORAGE_ACCOUNT_NAME=$OPTARG ;;
    c) CONTAINER_NAME=$OPTARG ;;
    h) usage ;;
    *) usage ;;
  esac
done

if [[
  -z "${RESOURCE_GROUP_NAME:-}" ||
  -z "${AZ_LOCATION:-}" ||
  -z "${CLIENT_ID:-}" ||
  -z "${AZ_ENVIRONMENT:-}" ||
  -z "${AZ_PRODUCT:-}" ||
  -z "${KEYVAULT_NAME:-}" ||
  -z "${STORAGE_ACCOUNT_NAME:-}" ||
  -z "${CONTAINER_NAME:-}"
]]; then
  usage
fi

AZ_TAGS=("Product=$AZ_PRODUCT" "Environment=$AZ_ENVIRONMENT")

echo "🔧 Bootstrapping infrastructure..."

#############################################
# Resource Group
#############################################
if az group show --name "$RESOURCE_GROUP_NAME" >/dev/null 2>&1; then
  echo "Resource group already exists: $RESOURCE_GROUP_NAME"
else
  az group create \
    --name "$RESOURCE_GROUP_NAME" \
    --location "$AZ_LOCATION" \
    --tags "${AZ_TAGS[@]}" \
    >/dev/null
fi

#############################################
# Storage Account
#############################################
if az storage account show \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --name "$STORAGE_ACCOUNT_NAME" \
  >/dev/null 2>&1; then
  echo "Storage account already exists: $STORAGE_ACCOUNT_NAME"
else
  az storage account create \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --name "$STORAGE_ACCOUNT_NAME" \
    --sku Standard_LRS \
    --kind StorageV2 \
    --allow-blob-public-access false \
    --https-only true \
    --min-tls-version TLS1_2 \
    >/dev/null
fi

#############################################
# Container
#############################################
if az storage container exists \
  --name "$CONTAINER_NAME" \
  --account-name "$STORAGE_ACCOUNT_NAME" \
  --auth-mode login \
  --query exists \
  -o tsv | grep -q true; then
  echo "Storage container already exists: $CONTAINER_NAME"
else
  az storage container create \
    --name "$CONTAINER_NAME" \
    --account-name "$STORAGE_ACCOUNT_NAME" \
    --auth-mode login \
    >/dev/null
fi

#############################################
# Key Vault
#############################################
TENANT_ID="$(az account show --query tenantId -o tsv)"

if az keyvault show --name "$KEYVAULT_NAME" >/dev/null 2>&1; then
  echo "Key Vault already exists: $KEYVAULT_NAME"
else
  az resource create \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --resource-type "Microsoft.KeyVault/vaults" \
    --name "$KEYVAULT_NAME" \
    --api-version "2025-05-01" \
    --is-full-object \
    --properties "{
      \"location\": \"$AZ_LOCATION\",
      \"type\": \"Microsoft.KeyVault/vaults\",
      \"name\": \"$KEYVAULT_NAME\",
      \"properties\": {
        \"tenantId\": \"$TENANT_ID\",
        \"sku\": {
          \"family\": \"A\",
          \"name\": \"standard\"
        },
        \"enableRbacAuthorization\": true,
        \"enableSoftDelete\": true,
        \"enablePurgeProtection\": true,
        \"softDeleteRetentionInDays\": 90
      }
    }" \
    >/dev/null
fi

#############################################
# RBAC Assignments
#############################################
echo "🔐 Assigning RBAC roles..."

ASSIGNEE_OBJECT_ID="$(az ad sp show \
  --id "$CLIENT_ID" \
  --query id \
  -o tsv)"

STORAGE_ID="$(az storage account show \
  --name "$STORAGE_ACCOUNT_NAME" \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --query id \
  -o tsv)"

KEYVAULT_ID="$(az keyvault show \
  --name "$KEYVAULT_NAME" \
  --query id \
  -o tsv)"

create_role_assignment_if_missing() {
  local role="$1"
  local scope="$2"

  if az role assignment list \
    --assignee "$ASSIGNEE_OBJECT_ID" \
    --role "$role" \
    --scope "$scope" \
    --query "[0].id" \
    -o tsv | grep -q .; then
    echo "Role assignment already exists: $role"
  else
    az role assignment create \
      --assignee-object-id "$ASSIGNEE_OBJECT_ID" \
      --role "$role" \
      --scope "$scope" \
      --assignee-principal-type "ServicePrincipal" \
      >/dev/null

    echo "Created role assignment: $role"
  fi
}

create_role_assignment_if_missing \
  "Storage Blob Data Contributor" \
  "$STORAGE_ID"

create_role_assignment_if_missing \
  "Key Vault Secrets User" \
  "$KEYVAULT_ID"

create_role_assignment_if_missing \
  "Key Vault Crypto Service Encryption User" \
  "$KEYVAULT_ID"

echo "✅ Bootstrap complete"
