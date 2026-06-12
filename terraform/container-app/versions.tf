terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.74.0"
    }
    azapi = {
      source  = "Azure/azapi"
      version = "2.10.0"
    }
    null = {
      source  = "hashicorp/null"
      version = "= 3.2.3"
    }
    random = {
      source  = "hashicorp/random"
      version = "= 3.6.3"
    }
  }
}
