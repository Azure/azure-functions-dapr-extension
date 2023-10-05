param envResourceNamePrefix string
param azStorageConnectionString string
param appInsightsConnectionString string
param environmentId string
param stateStoreName string

resource daprComponentStateManagement 'Microsoft.App/managedEnvironments/daprComponents@2023-05-01' existing = {
  name: stateStoreName
}

/* ###################################################################### */
// Create Azure Function
/* ###################################################################### */
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
          value: appInsightsConnectionString
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
    managedEnvironmentId: environmentId
  }
  dependsOn: [
    daprComponentStateManagement
  ]
}
