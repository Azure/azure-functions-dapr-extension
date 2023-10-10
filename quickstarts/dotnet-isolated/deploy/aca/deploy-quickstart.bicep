
@description('Resource name prefix')
param resourceNamePrefix string
var envResourceNamePrefix = toLower(resourceNamePrefix)

param imageName string = 'mcr.microsoft.com/daprio/samples/dotnet-isolated-dapr-azure-function-orderservice:edge'
param location string = resourceGroup().location

module deployQuickStart '../../../azure-resources/main.bicep' = {
  name: 'functionson-on-aca'
  scope: resourceGroup()
  params:{
    imageName: imageName
    envResourceNamePrefix: envResourceNamePrefix
    location: location
  }
}
