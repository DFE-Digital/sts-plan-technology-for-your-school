declare -a SECRET_NAMES=("connect" "img" "frame" "default")

KEYVAULT_NAME="${RESOURCE_GROUP_NAME}-kv"

export KEYVAULT_NAME
export TFVAR

for secret in "${SECRET_NAMES[@]}";
do
  SECRET_NAME="csp--${secret}src"
  TF_ID_NAME="azurerm_key_vault_secret.csp_${secret}_src"
  
  export SECRET_NAME
  export TF_ID_NAME
  bash "$PARENT_DIR/shared/keyvault/state_import.sh"
done
