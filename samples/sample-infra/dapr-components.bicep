param azStorageConnectionString string
param envResourceNamePrefix string
param redisCacheName string
param eventHubName string
param azStorageAccountName string
param containerName string
param eventHubNamespace string

resource environment 'Microsoft.App/managedEnvironments@2022-10-01' existing = {
  name: '${envResourceNamePrefix}-env'
}

resource redisCache 'Microsoft.Cache/Redis@2020-06-01' existing = {
  name: redisCacheName
}

resource eventHubAuth 'Microsoft.EventHub/namespaces/authorizationRules@2022-10-01-preview' existing = {
  name: '${eventHubNamespace}/${envResourceNamePrefix}-eventHubAuth'
}

/* ###################################################################### */
// Setup Dapr componet Redis state store in ACA
/* ###################################################################### */
resource daprComponentStateManagement 'Microsoft.App/managedEnvironments/daprComponents@2023-05-01' = {
  parent: environment
  name: 'statestore'
  properties: {
    componentType: 'state.redis'
    version: 'v1'
    metadata: [
      {
        name: 'redisHost'
        value: '${redisCacheName}.redis.cache.windows.net:6379'
      }
      {
        name: 'redisPassword'
        value: redisCache.listKeys().primaryKey
      }
    ]
    scopes: []
  }
}

/* ###################################################################### */
// Setup Dapr componet Redis message bus in ACA
/* ###################################################################### */
resource daprComponentMessagebus 'Microsoft.App/managedEnvironments/daprComponents@2023-05-01' = {
  parent: environment
  name: 'messagebus'
  properties: {
    componentType: 'pubsub.redis'
    version: 'v1'
    metadata: [
      {
        name: 'redisHost'
        value: '${redisCacheName}.redis.cache.windows.net:6379'
      }
      {
        name: 'redisPassword'
        value: redisCache.listKeys().primaryKey
      }
    ]
    scopes: []
  }
}

/* ###################################################################### */
// Setup Dapr componet Eventhub in ACA
/* ###################################################################### */
resource daprComponentEventHub 'Microsoft.App/managedEnvironments/daprComponents@2023-05-01' = {
  parent: environment
  name: 'sample-topic'
  properties: {
    componentType: 'bindings.azure.eventhubs'
    version: 'v1'
    metadata: [
      {
        name: 'connectionString'
        value: '${eventHubAuth.listKeys().primaryConnectionString};EntityPath=${eventHubName}'
      }
      {
        name: 'storageConnectionString'
        value: azStorageConnectionString
      }
      {
        name: 'eventHub'
        value: eventHubName
      }
      {
        name: 'consumerGroup'
        value: '$Default'
      }
      {
        name: 'storageAccountName'
        value: azStorageAccountName
      }
      {
        name: 'storageContainerName'
        value: containerName
      }
    ]
    scopes: []
  }
}

output stateStoreName string = 'statestore'
