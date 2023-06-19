#!/bin/bash

# Script Name: manage-azfunc
# Description: This script is used to invoke the Azure Functions CLI to manage the Azure Functions.

# Function to display help message
show_help() {
    echo "Usage: manage-azfunc [options] [arguments]"
    echo "Options:"
    echo "  -h, --help: Show help message."
    echo "Arguments:"
    echo "  init                      Initialize the Azure resources required for e2e tests using a bicep template."
    echo "  cleanup                   Delete the Azure resources created for e2e tests."
    echo "  deploy                    Set the container configuration to a specified Azure Function App image."
    echo "  geturl                    Get the URL of the specified Azure Function App."
    echo ""
}

# Function to handle the main functionality
main_function() {
    case $1 in
        init )
            # Create a random resource group name, also used as the prefix for all resources created.
            AZURE_RESOURCE_GROUP="e2e$(openssl rand -hex 3)"
            AZURE_LOCATION="eastus"
            TEMPLATE_FILE="$(dirname "${BASH_SOURCE[0]}")/DeployFunctionsOnAca.bicep"

            echo "Creating the resource group $AZURE_RESOURCE_GROUP..."
            az group create --name $AZURE_RESOURCE_GROUP --location $AZURE_LOCATION
            echo "Initializing the Azure resources required for e2e tests using a bicep template in the resource group $AZURE_RESOURCE_GROUP..."
            az deployment group create --resource-group $AZURE_RESOURCE_GROUP --template-file $TEMPLATE_FILE --parameters resourceNamePrefix=$AZURE_RESOURCE_GROUP
            echo "The Azure resources required for e2e tests have been created, and the resource group name is $AZURE_RESOURCE_GROUP"

            # Set the required environment variables for the e2e test.
            export DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP=$AZURE_RESOURCE_GROUP
            export DAPR_E2E_TEST_FUNCCAPPS_NAME=$AZURE_RESOURCE_GROUP-funcapp
            ;;
        cleanup )
            if [[ -z $DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP ]]; then
                echo "The environment variable DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP is not set"
                exit 1
            fi

            echo "Cleaning up the Azure resources created for e2e tests in the resource group $DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP..."
            az deployment group delete --resource-group $DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP --name DeployFunctionsOnAca
            az group delete --name $DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP --yes
            echo "The Azure resources created for e2e tests and the resource group $2 have been deleted"
            ;;
        deploy )
            echo "Setting the container configuration to a specified Azure Function App image..."
            az functionapp config container set --resource-group $AZURE_RESOURCE_GROUP --name $AZURE_FUNCTION_APP_NAME --docker-custom-image-name $AZURE_FUNCTION_APP_IMAGE
            ;;
        geturl )
            echo "Getting the URL of the specified Azure Function App..."
            az functionapp show --resource-group $AZURE_RESOURCE_GROUP --name $AZURE_FUNCTION_APP_NAME --query "defaultHostName" --output tsv
            ;;
        * )
            echo "Invalid command: $1"
            show_help
            exit 1
            ;;
    esac
}

# Parse command-line options
while [[ "$1" =~ ^- ]]; do
    case $1 in
        -h | --help )
            show_help
            exit
            ;;
        * )
            echo "Invalid option: $1"
            show_help
            exit 1
            ;;
    esac
    shift
done

# Check for the presence of a command
if [[ -z $1 ]]; then
    echo "No command specified"
    show_help
    exit 1
fi

# Execute the main functionality
main_function "$@"