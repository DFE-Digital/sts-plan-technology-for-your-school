terraform {
  backend "azurerm" {
      container_name       = "tfstate"
      key                  = "dns.tfstate"
      use_azuread_auth     = true
  }
}
