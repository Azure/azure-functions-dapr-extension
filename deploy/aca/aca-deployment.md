# Quickstart: DAPR with Functions on Azure Container Apps

In this tutorial, a sample Dapr solution is deployed to Functions on Azure Container Apps via Bicep template.
This Bicep script will deploy Function on Azure Container App along with DAPR extension, Azure Service Bus, DAPR Components - Redis as state store, Azure Event Hub as pub-sub.

You learn how to:

- Create an Azure Redis Cache for use as a Dapr state store
- Deploy a Container Apps environment to host container apps
- Deploy dapr-enabled Function on container apps: one that invokes the other service which will create an Order, saves it to storage via DAPR statestore.
- Verify the interaction between the two apps.

## Prerequisites:

- [Install Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- An Azure account with an active subscription is required. If you don't already have one, you can [create an account for free.](https://azure.microsoft.com/free/?WT.mc_id=A261C142F)

## Setup

After installing Azure CLI, launch the Command window and login to Azure:

```
az login
```
Setup  your az login with active subscription:

```
az account set --subscription <subscription-id-or-name>
```

## Clone the Azure function Dapr Extension Repo

```
git clone https://github.com/Azure/azure-functions-dapr-extension.git

```

## Create resource group

Container Apps support for Functions is currently in preview and is only available in the following regions. Specify the location from this list:
Australia East
Central US
East US
East US 2
North Europe
South Central US
UK South
West Europe
West US 3

```
az group create --name {resourceGroupName} --location {region}
```

## Deploy 

The Template deploys 
- A Container App Environment
- A Function App
- A Blob Storage Account and a default storage container
- Application Insights
- Log Analytics WorkSpace
- Dapr Component for StateManagement
- .NET Dapr enabled Function :  OrderService
- .NET DAPR enabled function : CreateNewOrder
- .NET DAPR enabled function: RetrieveOrder

From az CLI run the following command:

```
cd azure-functions-dapr-extension/aca-deployment
az deployment group create --resource-group {resourceGroupName} --template-file deploy-samples.bicep
```

## Verify the Result

Run the following CURL command to initiate a OrderService Function that will trigger CreateNewOrder process. A new Order is created and stored in the Redis store.

Replace the {sample-functionapp-url} value with your actual function app URL ex: https://daprext-funcapp.wittyglacier-20884174.eastus.azurecontainerapps.io.

Replace {sample-functionapp-name} with your function app name

```
curl --location 'https://{sample-functionapp-url.io}/api/invoke/{sample-functionapp-name}/CreateNewOrder' \
--header 'Content-Type: application/json' \
--data '{
    "data": {
        "value": {
            "orderId": "Order22"
        }
    }
}'
```

## View Logs

Data logged via a Function app are stored in the ContainerAppConsoleLogs_CL custom table in the Log Analytics workspace. You can view logs through the Azure portal or from the command line. Wait a few minutes for the analytics to arrive for the first time before you query the logged data.

From Portal: 
Navigate to your container app environment.

In the left side menu, under Monitoring, select Logs.

Run a query to check the container app console logs to verify your function app is receiving the invoked message from Dapr.

```
ContainerAppsConsoleLogs_CL
| where RevisionName_s == $revision_name
| where Log_s contains "Order22"
| project Log_s

## Clean up resources

Once you're done, run the following command to delete your resource group along with all the resources you created in this tutorial.

```
az group delete --resource-group $RESOURCE_GROUP
```
