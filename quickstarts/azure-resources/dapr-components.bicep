param envResourceNamePrefix string
param redisCacheName string

resource environment 'Microsoft.App/managedEnvironments@2022-10-01' existing = {
  name: '${envResourceNamePrefix}-env'
}

resource redisCache 'Microsoft.Cache/Redis@2020-06-01' existing = {
  name: redisCacheName
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
  dependsOn: [
    redisCache
  ]
}

output stateStoreName string = 'statestore'
