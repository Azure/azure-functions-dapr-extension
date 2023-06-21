@minLength(3)
@maxLength(14)
@description('Resource name prefix')
param resourceNamePrefix string

@description('Resource location')
param location string = resourceGroup().location

var envResourceNamePrefix = toLower(resourceNamePrefix)
var storageAccountName = '${envResourceNamePrefix}storage'

// Storage account is a prerequisite for functions app
resource azStorageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    defaultToOAuthAuthentication: true
  }
}

resource environment 'Microsoft.App/managedEnvironments@2022-10-01' = {
  name: '${envResourceNamePrefix}-env'
  location: location
  properties: {}
}

resource azfunctionapp 'Microsoft.Web/sites@2022-09-01' = {
  name: '${envResourceNamePrefix}-funcapp'
  location: location
  kind: 'functionapp'
  properties: {
    managedEnvironmentId: environment.id
    name: '${envResourceNamePrefix}-funcapp'
    siteConfig: {
      linuxFxVersion: 'Docker|mcr.microsoft.com/azure-functions/dotnet7-quickstart-demo:1.0'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${azStorageAccount.listKeys().keys[0].value}'
        }
      ]
    }
  }
}

output functionAppName string = azfunctionapp.name
