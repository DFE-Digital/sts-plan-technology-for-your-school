
#front door created in WAF, needed as we are creating the custom domain here instead so that we can attach the dns zone id to it.
#this format can look up from another state file, in the case that we are using a separate dns state file.
data "terraform_remote_state" "app" {
  backend = "azurerm"

  config = {
    resource_group_name  = local.tf_rg
    storage_account_name = local.tf_sa
    container_name       = local.tf_container
    key                  = local.tf_key
  }
}
