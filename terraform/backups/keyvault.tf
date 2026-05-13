terraform {
  required_providers {
    azurerm = {
      source  = "azurerm"
      version = "4.66.0"
    }
  }
}
provider "azurerm" {
  features {}
}
resource "azurerm_key_vault" "backup" {
  enable_rbac_authorization       = true
  enabled_for_deployment          = false
  enabled_for_disk_encryption     = false
  enabled_for_template_deployment = false
  location                        = var.azure_location
  name                            = locals.keyvaultname
  public_network_access_enabled   = true
  purge_protection_enabled        = true
  rbac_authorization_enabled      = true
  resource_group_name             = var.backup_resource_group_name
  sku_name                        = "standard"
  soft_delete_retention_days      = var.blob_delete_retention_days
  tags = {
    Environment        = var.az_tag_environment
    Product            = var.az_tag_product
    "Service Offering" = var.az_tag_product
  }
  network_acls {
    bypass                     = "None"
    default_action             = "Deny"
    ip_rules                   = ["13.88.20.215/32", "13.88.58.165/32", "13.91.63.236/32", "132.196.83.34/32", "135.232.193.32/32", "135.232.200.16/32", "135.232.224.81/32", "145.132.101.176/32", "145.132.99.17/32", "154.51.87.62/32", "157.55.172.154/32", "163.116.162.120/32", "163.116.165.101/32", "163.116.165.102/32", "163.116.165.113/32", "163.116.165.115/32", "163.116.165.118/32", "163.116.165.64/32", "163.116.165.66/32", "163.116.177.105/32", "163.116.177.35/32", "163.116.177.50/32", "172.173.233.53/32", "172.178.22.139/32", "172.182.224.192/32", "172.183.133.242/32", "172.183.249.255/32", "172.206.208.138/32", "172.208.85.146/32", "172.215.217.67/32", "176.24.209.28/32", "20.106.182.209/32", "20.163.53.216/32", "20.168.118.83/32", "20.169.160.74/32", "20.171.123.131/32", "20.171.147.63/32", "20.221.225.240/32", "20.241.39.152/32", "20.241.51.43/32", "20.241.99.83/32", "20.55.222.80/32", "20.62.207.84/32", "20.98.9.252/32", "208.127.46.236/32", "208.127.46.238/32", "208.127.46.239/32", "208.127.46.240/32", "208.127.46.241/32", "208.127.46.242/32", "208.127.46.243/32", "208.127.46.244/32", "208.127.46.245/32", "208.127.46.246/32", "208.127.46.247/32", "208.127.46.248/32", "208.127.46.251/32", "208.127.46.252/32", "208.127.46.253/32", "208.127.46.254/32", "208.127.46.255/32", "23.99.252.147/32", "4.227.236.140/32", "4.236.173.18/32", "40.122.118.128/32", "40.78.23.17/32", "40.78.6.224/32", "48.217.140.224/32", "52.138.80.37/32", "52.150.29.98/32", "52.160.39.122/32", "52.161.57.35/32", "52.162.121.125/32", "52.165.217.108/32", "52.166.126.216/32", "52.185.37.172/32", "52.190.140.128/32", "52.247.52.32/32", "52.247.54.139/32", "64.236.134.51/32", "64.236.135.4/32", "68.154.116.112/32", "9.234.151.22/32", "90.212.138.112/32", "90.214.33.196/32", "92.12.80.148/32", "92.21.132.68/32", "94.190.240.247/32"]
    virtual_network_subnet_ids = []
  }
}
###################
# Access Policies #
###################

resource "azurerm_key_vault_access_policy" "vault_access_policy_tf" {
  key_vault_id = azurerm_key_vault.backup.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = local.current_user_id

  secret_permissions = ["List", "Get", "Set"]
  key_permissions    = ["List", "Get", "Create", "GetRotationPolicy", "SetRotationPolicy", "Delete", "Purge", "UnwrapKey", "WrapKey"]
}

resource "azurerm_key_vault_access_policy" "vault_access_policy_mi" {
  key_vault_id = azurerm_key_vault.backup.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_user_assigned_identity.user_assigned_identity.principal_id

  secret_permissions = ["List", "Get"]
  key_permissions    = ["List", "Get", "WrapKey", "UnwrapKey"]
}
