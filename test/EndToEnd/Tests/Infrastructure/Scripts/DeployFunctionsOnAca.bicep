@minLength(3)
@maxLength(10)
@description('Resource name prefix')
param resourceNamePrefix string
var envResourceNamePrefix = toLower(resourceNamePrefix)

@description('Resource location')
param location string = resourceGroup().location

// Storage account is a prerequisite for functions app
resource azStorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: '${envResourceNamePrefix}storage'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}

resource environment 'Microsoft.App/managedEnvironments@2022-10-01' = {
  name: '${envResourceNamePrefix}-env'
  location: location
}

resource azfunctionapp 'Microsoft.Web/sites@2022-09-01' = {
  name: '${envResourceNamePrefix}-funcapp'
  location: location
  kind: 'functionapp'
  properties: {
    name: '${envResourceNamePrefix}-funcapp'
    managedEnvironmentId: environment.id
    siteConfig: {
      linuxFxVersion: 'Docker|mcr.microsoft.com/azure-functions/dotnet7-quickstart-demo:1.0'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: azStorageAccount.properties.ConnectionString
        }
      ]
    }
  }
}

output functionAppName string = azfunctionapp.name
