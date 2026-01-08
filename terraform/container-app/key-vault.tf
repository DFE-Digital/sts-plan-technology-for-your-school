resource "azurerm_key_vault" "vault" {
  name                       = local.kv_name
  location                   = local.azure_location
  resource_group_name        = module.main_hosting.azurerm_resource_group_default.name
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = "standard"
  soft_delete_retention_days = 90
  enable_rbac_authorization  = false
  tags                       = local.tags
  purge_protection_enabled   = true

  network_acls {
    bypass                     = "None"
    default_action             = "Deny"
    virtual_network_subnet_ids = [module.main_hosting.networking.subnet_id]
    ip_rules                   = toset(concat(tolist(local.kv_firewall_cidr_rules), [var.workflow_runner_ip]))
  }
}

###################
# Access Policies #
###################

resource "azurerm_key_vault_access_policy" "vault_access_policy_tf" {
  key_vault_id = azurerm_key_vault.vault.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = local.current_user_id

  secret_permissions = ["List", "Get", "Set"]
  key_permissions    = ["List", "Get", "Create", "GetRotationPolicy", "SetRotationPolicy", "Delete", "Purge", "UnwrapKey", "WrapKey"]
}

resource "azurerm_key_vault_access_policy" "vault_access_policy_mi" {
  key_vault_id = azurerm_key_vault.vault.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_user_assigned_identity.user_assigned_identity.principal_id

  secret_permissions = ["List", "Get"]
  key_permissions    = ["List", "Get", "WrapKey", "UnwrapKey"]
}

###########
# Secrets #
###########

######################
# Contentful secrets #
######################

resource "azurerm_key_vault_secret" "vault_secret_contentful_deliveryapikey" {
  key_vault_id = azurerm_key_vault.vault.id
  name         = "contentful--deliveryapikey"
  value        = "temp value"

  lifecycle {
    ignore_changes = [
      value,
      expiration_date
    ]
  }
}

resource "azurerm_key_vault_secret" "vault_secret_contentful_previewapikey" {
  key_vault_id = azurerm_key_vault.vault.id
  name         = "contentful--previewapikey"
  value        = "temp value"

  lifecycle {
    ignore_changes = [
      value,
      expiration_date
    ]
  }
}

resource "azurerm_key_vault_secret" "vault_secret_contentful_spaceid" {
  key_vault_id = azurerm_key_vault.vault.id
  name         = "contentful--spaceid"
  value        = "temp value"

  lifecycle {
    ignore_changes = [
      value,
      expiration_date
    ]
  }
}

resource "azurerm_key_vault_secret" "vault_secret_contentful_environment" {
  key_vault_id = azurerm_key_vault.vault.id
  name         = "contentful--environment"
  value        = "temp value"

  lifecycle {
    ignore_changes = [
      value,
      expiration_date
    ]
  }
}

####################
# Database secrets #
####################

resource "azurerm_key_vault_secret" "vault_secret_database_connectionstring" {
  key_vault_id = azurerm_key_vault.vault.id
  name         = "connectionstrings--database"
  value        = local.az_sql_connection_string

  lifecycle {
    ignore_changes = [
      expiration_date
    ]
  }
}

###################################
# Content Security Policy secrets #
###################################

resource "azurerm_key_vault_secret" "csp_connect_src" {
  name         = "CSP--ConnectSrc"
  key_vault_id = azurerm_key_vault.vault.id
  value        = local.kv_secrets_csp_connectsrc

  lifecycle {
    ignore_changes = [
      value,
      expiration_date
    ]
  }
}

resource "azurerm_key_vault_secret" "csp_default_src" {
  name         = "CSP--DefaultSrc"
  key_vault_id = azurerm_key_vault.vault.id
  value        = local.kv_secrets_csp_defaultsrc

  lifecycle {
    ignore_changes = [
      value,
      expiration_date
    ]
  }
}

resource "azurerm_key_vault_secret" "csp_frame_src" {
  name         = "CSP--FrameSrc"
  key_vault_id = azurerm_key_vault.vault.id
  value        = local.kv_secrets_csp_framesrc

  lifecycle {
    ignore_changes = [
      value,
      expiration_date
    ]
  }
}

resource "azurerm_key_vault_secret" "csp_img_src" {
  name         = "CSP--ImgSrc"
  key_vault_id = azurerm_key_vault.vault.id
  value        = local.kv_secrets_csp_imgsrc

  lifecycle {
    ignore_changes = [
      value,
      expiration_date
    ]
  }
}

########
# Keys #
########

resource "azurerm_key_vault_key" "data_protection_key" {
  name         = "dataprotection"
  key_vault_id = azurerm_key_vault.vault.id

  key_type = var.key_type
  key_size = var.key_size
  key_opts = var.key_ops

  tags = local.tags

  rotation_policy {
    automatic {
      # Rotate (renew) 7 days before the key expires
      time_before_expiry = "P7D"
    }

    # Key expires after 1 month
    expire_after         = "P1M"

    # Notify 14 days before expiry
    notify_before_expiry = "P14D"
  }

  lifecycle {
    ignore_changes = all
  }
}

#######
# API #
#######

resource "random_password" "api_key_value" {
  length  = 32
  special = true
}

resource "azurerm_key_vault_secret" "api_key" {
  name         = "api--authentication--keyvalue"
  key_vault_id = azurerm_key_vault.vault.id
  value        = random_password.api_key_value.result

  expiration_date = timeadd(timestamp(), "${local.contentful_webhook_secret_timetolive_hours}h")
}
