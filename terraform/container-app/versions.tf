terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "= 4.4.0"
    }
    azapi = {
      source  = "Azure/azapi"
      version = "= 1.15.0"
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
