terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.37"
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
