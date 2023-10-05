param location string
param redisCacheName string
param eventHubNamespace string
param eventHubName string
param eventHubSku string
param containerName string
param envResourceNamePrefix string

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

/* ###################################################################### */
// Create managed ACA environment
/* ###################################################################### */
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

/* ###################################################################### */
// Create Redis Cache
/* ###################################################################### */
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

/* ###################################################################### */
// Create Azure EventHub
/* ###################################################################### */
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
  name: '${envResourceNamePrefix}-eventHubAuth'
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

output azStorageConnectionString string = azStorageConnectionString
output appInsightsConnectionString string = appInsights.properties.ConnectionString
output environmentId string = environment.id
output eventHubName string = eventHubName
output azStorageAccountName string = azStorageAccount.name
