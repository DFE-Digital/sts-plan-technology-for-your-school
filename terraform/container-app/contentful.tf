data "azurerm_cdn_frontdoor_endpoint" "app" {
  name                = "${local.resource_prefix}-container-app-url"
  profile_name        = "${local.resource_prefix}-cdnwaf"
  resource_group_name = local.resource_prefix
}

resource "null_resource" "upsert_contentful_webhook" {
  triggers = {
    api_key_change         = azurerm_key_vault_secret.api_key.value
    url_change             = data.azurerm_cdn_frontdoor_endpoint.app.host_name
    management_token       = var.contentful_management_token
    contentful_environment = azurerm_key_vault_secret.vault_secret_contentful_environment.value
    contentful_space       = azurerm_key_vault_secret.vault_secret_contentful_spaceid.value
    webhook_url            = var.contentful_webhook_endpoint
    should_upsert          = var.contentful_upsert_webhook == true ? timestamp() : var.contentful_upsert_webhook
  }

  provisioner "local-exec" {
    command = local.contentful_webhook_shell_command
  }
}
