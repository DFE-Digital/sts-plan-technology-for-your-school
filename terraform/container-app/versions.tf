terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "= 3.111.0"
    }
    azapi = {
      source  = "Azure/azapi"
      version = ">= 1.6.0"
    }
    null = {
      source  = "hashicorp/null"
      version = ">= 3.2.1"
    }
    random = {
      source  = "hashicorp/random"
      version = ">= 3.5.1"
    }
  }
}
