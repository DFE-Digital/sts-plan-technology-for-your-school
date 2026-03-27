#!/bin/bash

set -e
set -o pipefail

usage() {
  echo "Usage: $(basename "$0") [OPTIONS]"
  echo "  -g   Resource group name"
  echo "  -l   Azure location"
  echo "  -s   Object ID (service principal / OIDC identity)"
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
    s) OBJ_ID=$OPTARG ;;
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
  -z "$RESOURCE_GROUP_NAME" ||
  -z "$AZ_LOCATION" ||
  -z "$OBJ_ID" ||
  -z "$AZ_ENVIRONMENT" ||
  -z "$AZ_PRODUCT" ||
  -z "$KEYVAULT_NAME" ||
  -z "$STORAGE_ACCOUNT_NAME" ||
  -z "$CONTAINER_NAME"
]]; then
  usage
fi

AZ_TAGS=("Product=$AZ_PRODUCT" "Environment=$AZ_ENVIRONMENT")

echo "🔧 Bootstrapping infrastructure..."

#############################################
# Resource Group
#############################################
az group create \
  --name "$RESOURCE_GROUP_NAME" \
  --location "$AZ_LOCATION" \
  --tags "${AZ_TAGS[@]}" \
  >/dev/null

#############################################
# Storage Account (Terraform backend)
#############################################
az storage account create \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --name "$STORAGE_ACCOUNT_NAME" \
  --sku Standard_LRS \
  --kind StorageV2 \
  --allow-blob-public-access false \
  >/dev/null || true

#############################################
# Container (Terraform state)
#############################################
az storage container create \
  --name "$CONTAINER_NAME" \
  --account-name "$STORAGE_ACCOUNT_NAME" \
  --auth-mode login \
  >/dev/null || true

#############################################
# Key Vault (for tfvars secrets)
#############################################
az keyvault create \
  --name "$KEYVAULT_NAME" \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --location "$AZ_LOCATION" \
  --enable-rbac-authorization true \
  >/dev/null || true

#############################################
# RBAC Assignments
#############################################

echo "🔐 Assigning RBAC roles..."

STORAGE_ID=$(az storage account show \
  --name "$STORAGE_ACCOUNT_NAME" \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --query id -o tsv)

KEYVAULT_ID=$(az keyvault show \
  --name "$KEYVAULT_NAME" \
  --query id -o tsv)

# Terraform backend access
az role assignment create \
  --assignee "$OBJ_ID" \
  --role "Storage Blob Data Contributor" \
  --scope "$STORAGE_ID" \
  >/dev/null || true

# Read secrets for tfvars generation
az role assignment create \
  --assignee "$OBJ_ID" \
  --role "Key Vault Secrets User" \
  --scope "$KEYVAULT_ID" \
  >/dev/null || true

echo "✅ Bootstrap complete"
