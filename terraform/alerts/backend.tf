terraform {
  required_providers {
    azurerm = {
      source  = "azurerm"
      version = "4.66.0"
    }
  }
}

provider "azurerm" {
  features {
      container_name       = "tfstate"
      key                  = "alerts.tfstate"
      use_azuread_auth     = true
  }
}
