terraform {
  required_version = ">= 1.5.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 5.12"
    }
    azapi = {
      source  = "Azure/azapi"
      version = "~> 2.11"
    }
    null = {
      source  = "hashicorp/null"
      version = "3.3.2"
    }
    random = {
      source  = "hashicorp/random"
      version = "= 3.6.3"
    }
  }
}
