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

## List of resources bicep will create

1. Azure Storage Account
2. Blob Service
3. Blob Container
4. Log Analytics Workspace
5. Application Insights
6. Managed Environment
7. Redis Cache
8. Event Hub Namespace
9. Event Hub Authorization Rule
10. Event Hub
11. Dapr Component for State Management
12. Dapr Component for Message Bus
13. Dapr Component for Event Hub
14. Azure Function App