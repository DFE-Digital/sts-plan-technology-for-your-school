terraform {
  backend "azurerm" {
    container_name   = "tfstate"
    key              = "backup.tfstate"
    use_azuread_auth = true
  }
}
