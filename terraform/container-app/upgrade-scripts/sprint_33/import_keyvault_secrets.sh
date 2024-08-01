set -e
set -o pipefail

echo $RESOURCE_GROUP_NAME
echo $TFVAR_PATH

exit 0

keyvault_name="${RESOURCE_GROUP_NAME}-kv"
declare -a secret_names=("connect" "img" "frame" "default")

for secret in "${secret_names[@]}";
do
  secret_name="csp--${secret}src"
  tf_name="azurerm_key_vault_secret.csp_${secret}_src"
  
  echo "Fetching ID for $secret_name"
  # Fetch secret value using CLI -> extract ID using JQ -> remove quotation marks
  secret_id=$(az keyvault secret show --vault-name ${keyvault_name} --name $secret_name | jq .id | sed -e 's/"//g')

  echo "Running command: terraform import -var-file=${TFVAR_PATH} $tf_name $secret_id"
  terraform import -var-file=../terraform-${terraform_environment}.tfvars $tf_name $secret_id
done
