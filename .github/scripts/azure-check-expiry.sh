#!/bin/bash
set -e

ENVIRONMENT="$ENVIRONMENT"
VAULT_NAME="$AZ_KEYVAULT_NAME"
OUTPUT_FILE="azure-secrets-expiring.yml"
THRESHOLD_DAYS=30

echo "Checking secrets and keys for $ENVIRONMENT environment"
today=$(date +%s)
threshold=$(($today + $THRESHOLD_DAYS*24*3600))

echo "# Auto-generated Azure expiring secrets" > "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"
echo "secrets:" >> "$OUTPUT_FILE"

check_expiry() {
  local type="$1"
  local list_cmd="$2"
  local show_cmd="$3"

  echo "🔎 Checking expiry for ${type}s..."

  count=0
  while IFS= read -r name; do
    set +e
    expiry_output=$(eval "$show_cmd" 2>&1)
    exit_code=$?
    # set -e

    if [[ $exit_code -ne 0 ]]; then
      if echo "$expiry_output" | grep -q "SecretDisabled"; then
        echo "⚠️  Skipping disabled $type: $name"
        continue
      else
        echo "❌ Unexpected error checking $type '$name':"
        echo "🔍 Exit code: $exit_code"
        echo "🔍 Command: $show_cmd"
        echo "🔍 Raw output:"
        echo "$expiry_output"
        exit 1
      fi
    fi

    expiry=$(echo "$expiry_output" | tr -d '\r')
    if [[ -n "$expiry" ]]; then
      expiry_ts=$(date -d "$expiry" +%s)
      if (( expiry_ts < threshold )); then
        {
          echo "- name: $name"
          echo "  type: $type"
          echo "  vault: $VAULT_NAME"
          echo "  expires: $expiry"
          echo ""
        } >> "$OUTPUT_FILE"
        ((count++))
      fi
    fi
  done <<< "$(eval "$list_cmd")"

  plural="$type"
  [[ $count -ne 1 ]] && plural="${type}s"
  echo "✅ Expiry info written ($count $plural flagged)"
}

# Secrets
check_expiry "secret" \
  "az keyvault secret list --vault-name \"$VAULT_NAME\" --query \"[].name\" -o tsv" \
  "az keyvault secret show --vault-name \"$VAULT_NAME\" --name \"\$name\" --query \"attributes.expires\" -o tsv"

# Keys
check_expiry "key" \
  "az keyvault key list --vault-name \"$VAULT_NAME\" --query \"[].name\" -o tsv" \
  "az keyvault key show --vault-name \"$VAULT_NAME\" --name \"\$name\" --query \"attributes.expires\" -o tsv"

echo "Expiry info written to $OUTPUT_FILE"
