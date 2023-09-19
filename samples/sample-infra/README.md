# azure-functions-dapr-aca-deployment
Steps to deploy the Azure Function with Dapr extension in ACA.

## Create resource group
```
az group create --name {resourceGroupName} --location eastasia
```

## Deploy azure function samples with Dapr extension in ACA
```
az deployment group create --resource-group {resourceGroupName} --template-file deploy-samples.bicep
```