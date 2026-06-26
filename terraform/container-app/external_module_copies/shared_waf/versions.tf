terraform {
  required_version = "~> 1.9"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 2.39"
    }

    azapi = {
      source  = "azure/azapi"
      version = "~> 1.13"
    }
  }
}
