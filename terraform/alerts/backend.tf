terraform {
  required_providers {
    azurerm = {
      source  = "azurerm"
      version = "4.66.0"
    }
  }

  backend "azurerm" {
    container_name   = "tfstate"
    key              = "alerts.tfstate"
    use_azuread_auth = true
  }
}

provider "azurerm" {
  features {}
}
