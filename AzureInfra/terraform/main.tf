terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=4.1.0"
    }
  }
}

provider "azurerm" {
  features {
      resource_group {
        prevent_deletion_if_contains_resources = false
    }
  }

  subscription_id = "0ad5ac4a-b031-42a2-9fe5-5feb754f1822"

}

resource "azurerm_resource_group" "greenhouse_dev" {
  name     = "greenhouse"
  location = "polandcentral"

  tags = {
    environment = "development"
  }
}

resource "azurerm_container_registry" "acr" {
  name                          = "greenhouseContainerRegistry"
  resource_group_name           = azurerm_resource_group.greenhouse_dev.name
  location                      = azurerm_resource_group.greenhouse_dev.location
  sku                           = "Basic"
  admin_enabled                 = false
  public_network_access_enabled = true 
  zone_redundancy_enabled       = false

  tags = {
    environment = "development"
  }
}

resource "azurerm_app_configuration" "example" {
  name                       = "greenhouseAppConf"
  resource_group_name        = azurerm_resource_group.greenhouse_dev.name
  location                   = azurerm_resource_group.greenhouse_dev.location
  sku                        = "standard"
  local_auth_enabled         = true
  public_network_access      = "Enabled"
  purge_protection_enabled   = false
  soft_delete_retention_days = 1
  
  tags = {
    environment = "development"
  }
}
