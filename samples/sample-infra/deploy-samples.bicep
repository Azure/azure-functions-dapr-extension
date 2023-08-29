param location string = resourceGroup().location
param redisCacheName string = 'redisCacheDaprStateStore-${uniqueString(resourceGroup().id)}'
param eventHubNamespace string = 'daprEventHubNamespace-${uniqueString(resourceGroup().id)}'
param eventHubName string = 'daprEventHub-${uniqueString(resourceGroup().id)}'
param eventHubSku string = 'Standard'
param containerName string = 'daprazurefunctionseventhubstoragecontainer'

@description('Resource name prefix')
param resourceNamePrefix string
var envResourceNamePrefix = toLower(resourceNamePrefix)

/* ###################################################################### */
// Create storage account for function app prereq
/* ###################################################################### */
resource azStorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: '${envResourceNamePrefix}storage'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}
var azStorageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${azStorageAccount.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${azStorageAccount.listKeys().keys[0].value}'

// Create blob service
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2019-06-01' = {
  name: 'default'
  parent: azStorageAccount
}

// Create container
resource container 'Microsoft.Storage/storageAccounts/blobServices/containers@2019-06-01' = {
  name: containerName
  parent: blobService
  properties: {
    publicAccess: 'None'
    metadata: {}
  }
}


resource logAnalyticsWorkspace'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name: '${envResourceNamePrefix}-la'
  location: location
  properties: any({
    retentionInDays: 30
    features: {
      searchVersion: 1
    }
    sku: {
      name: 'PerGB2018'
    }
  })
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${envResourceNamePrefix}-ai'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

resource environment 'Microsoft.App/managedEnvironments@2022-10-01' = {
  name: '${envResourceNamePrefix}-env'
  location: location
  properties: {
    daprAIInstrumentationKey: appInsights.properties.InstrumentationKey
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
  }
}

resource redisCache 'Microsoft.Cache/Redis@2020-06-01' = {
  name: redisCacheName
  location: location
  properties: {
    enableNonSslPort: true
    minimumTlsVersion: '1.2'
    sku: {
      capacity: 1
      family: 'C'
      name: 'Standard'
    }
  }
}

resource eventHubNamespaceResource 'Microsoft.EventHub/namespaces@2021-11-01' = {
  name: eventHubNamespace
  location: location
  sku: {
    name: eventHubSku
    tier: eventHubSku
    capacity: 1
  }
  properties: {
    isAutoInflateEnabled: false
    maximumThroughputUnits: 0
    kafkaEnabled: true
  }
}

resource eventHubAuth 'Microsoft.EventHub/namespaces/authorizationRules@2022-10-01-preview' = {
  name: 'string'
  parent: eventHubNamespaceResource
  properties: {
    rights: [
      'Listen'
      'Manage'
      'Send'
    ]
  }
}

resource eventHub 'Microsoft.EventHub/namespaces/eventhubs@2021-11-01' = {
  parent: eventHubNamespaceResource
  name: eventHubName
  properties: {
    messageRetentionInDays: 7
    partitionCount: 1
  }
}

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
        value: azStorageAccount.name
      }
      {
        name: 'storageContainerName'
        value: container.name
      }
    ]
    scopes: []
  }
}

resource azfunctionapp 'Microsoft.Web/sites@2022-09-01' = {
  name: '${envResourceNamePrefix}-funcapp'
  location: 'East Asia (Stage)'
  kind: 'functionapp,linux,container,azurecontainerapps'
  properties: {
    name: '${envResourceNamePrefix}-funcapp'
    siteConfig: {

    appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: azStorageConnectionString
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'PubSubName'
          value: 'messagebus'
        }
        {
          name: 'StateStoreName'
          value: 'statestore'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'KafkaBindingName'
          value: 'sample-topic'
        }
      ]
      linuxFxVersion: 'Docker|mcr.microsoft.com/daprio/samples/dotnet-azurefunction:edge'  
    }
    DaprConfig: {
      enabled: true
      appId: '${envResourceNamePrefix}-funcapp'
      appPort: 3001
      httpReadBufferSize: ''
      httpMaxRequestSize: ''
      logLevel: ''
      enableApiLogging: true
    }
    managedEnvironmentId:environment.id
  }
  dependsOn: [
    daprComponentStateManagement
  ]
}
  
output functionAppName string = azfunctionapp.name
