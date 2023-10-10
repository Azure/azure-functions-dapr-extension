param location string = resourceGroup().location
param redisCacheName string = 'redisCacheDaprStateStore-${uniqueString(resourceGroup().id)}'
param imageName string
param envResourceNamePrefix string

module azureServices 'azure-services.bicep' = {
  name: 'azure-services'
  scope: resourceGroup()
  params:{
    envResourceNamePrefix: envResourceNamePrefix
    location: location
    redisCacheName: redisCacheName
  }
}

module daprComponents 'dapr-components.bicep' = {
  name: 'dapr-components'
  scope: resourceGroup()
  params:{
    envResourceNamePrefix: envResourceNamePrefix
    redisCacheName: redisCacheName
  }
  dependsOn: [
    azureServices
  ]
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
    imageName: imageName
  }
}
