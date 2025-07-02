# scripts/get-and-clean-tfvars.ps1
param (
    [string]$SecretName,
    [string]$KeyVaultName
)
az keyvault secret show `
  --name "$SecretName" `
  --vault-name "$KeyVaultName" `
  --query "value" `
  --output tsv > terraformVars.tfvars
$content = Get-Content ./terraformVars.tfvars -Raw
[System.IO.File]::WriteAllText("normalisedTerraformVars.tfvars", $content, [System.Text.Encoding]::UTF8)