param location string = resourceGroup().location
param redisCacheName string = 'redisCacheDaprStateStore-${uniqueString(resourceGroup().id)}'
param eventHubNamespace string = 'daprEventHubNamespace-${uniqueString(resourceGroup().id)}'
param eventHubName string = 'daprEventHub-${uniqueString(resourceGroup().id)}'
param eventHubSku string = 'Standard'
param containerName string = 'daprazurefunctionseventhubstoragecontainer'

@description('Resource name prefix')
param resourceNamePrefix string
var envResourceNamePrefix = toLower(resourceNamePrefix)

module azureServices 'azure-services.bicep' = {
  name: 'azure-services'
  scope: resourceGroup()
  params:{
    containerName:containerName
    envResourceNamePrefix: envResourceNamePrefix
    eventHubName:eventHubName
    eventHubNamespace: eventHubNamespace
    eventHubSku: eventHubSku
    location: location
    redisCacheName: redisCacheName
  }
}

module daprComponents 'dapr-components.bicep' = {
  name: 'dapr-components'
  scope: resourceGroup()
  params:{
    containerName:containerName
    envResourceNamePrefix: envResourceNamePrefix
    eventHubName:eventHubName
    redisCacheName: redisCacheName
    azStorageAccountName: azureServices.outputs.azStorageAccountName
    azStorageConnectionString: azureServices.outputs.azStorageConnectionString
    eventHubNamespace: eventHubNamespace
  }
}

module azureFunction 'azure-function.bicep' = {
  name: 'azure-function'
  scope: resourceGroup()
  params:{
    location: location
    envResourceNamePrefix: envResourceNamePrefix
    azStorageConnectionString: azureServices.outputs.azStorageConnectionString
    appInsightsConnectionString: azureServices.outputs.appInsightsConnectionString
    environmentId: azureServices.outputs.environmentId
    stateStoreName: daprComponents.outputs.stateStoreName
  }
}
