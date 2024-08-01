if [[ -z "$SECRET_NAME" || -z "$KEYVAULT_NAME" ]]; then
  echo "Variable(s) not set. \$SECRET_NAME and \$KEYVAULT_NAME are required"
  exit 1
fi

echo "Fetching ID for $SECRET_NAME for Key Vault $KEYVAULT_NAME"

# Fetch secret value using CLI -> extract ID using JQ -> remove quotation marks
SECRET_ID=$(az keyvault secret show --vault-name $KEYVAULT_NAME --name $SECRET_NAME | jq .id | sed -e 's/"//g')

if [[ -z "$SECRET_ID" ]]; then
  echo "Could not retrieve ID for secret $SECRET_NAME"
  exit 1
fi

echo "Running command: terraform import -var-file=${TFVAR_PATH} $TF_ID_NAME $SECRET_ID"
terraform import -var-file=../$TF_VARS $TF_NAME $secret_id
