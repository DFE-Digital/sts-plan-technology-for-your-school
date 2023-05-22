terraform {
  required_providers {
    dotenv = {
      source  = "jrhouston/dotenv"
      version = "~> 1.0"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.51.0"
    }
  }
  backend "azurerm" {}
}

provider "azurerm" {
  features {}
  skip_provider_registration = true
}