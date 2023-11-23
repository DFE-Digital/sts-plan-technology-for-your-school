provider "azurerm" {
  features {}

  skip_provider_registration = true
  storage_use_azuread        = true
}
