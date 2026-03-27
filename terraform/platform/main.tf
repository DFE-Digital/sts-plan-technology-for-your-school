terraform {
  required_version = ">= 1.5.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.37"
    }
  }
}

provider "azurerm" {
  features {}
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location

  tags = {
    Product            = var.product
    Environment        = var.environment
    Service Offering = var.product
  }
}

# Azure Container Registry
resource "azurerm_container_registry" "acr" {
  name                = var.acr_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  admin_enabled = true
  sku           = "Standard"

  tags = {
    Product     = var.product
    Environment = var.environment
    Service Offering = var.product
  }
}

#costings storage acc
resource "azurerm_resource_group" "costings" {
  name     = "example-resources"
  location = "West Europe"
}

resource "azurerm_storage_account" "example" {
  name                     = var.costings_name
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"

  tags = {
    Product            = var.product
    Environment        = var.environment
    Service Offering = var.product
  }
}
