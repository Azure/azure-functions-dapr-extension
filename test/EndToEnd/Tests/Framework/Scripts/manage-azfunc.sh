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
    echo "  deploy <appname>          Set the container configuration to a specified Azure Function App image."
    echo "  geturl                    Get the URL of the specified Azure Function App."
    echo ""
}

# Function to initialize the Azure resources required for e2e tests using a bicep template.
azfunc_init() {
    AZURE_RESOURCE_GROUP="e2e$(openssl rand -hex 3)"
    AZURE_LOCATION="eastus"
    TEMPLATE_FILE="$(dirname "${BASH_SOURCE[0]}")/DeployFunctionsOnAca.bicep"

    echo "Creating the resource group $AZURE_RESOURCE_GROUP using template file $TEMPLATE_FILE..."
    az group create --name $AZURE_RESOURCE_GROUP --location $AZURE_LOCATION
    echo "Initializing the Azure resources required for e2e tests using a bicep template in the resource group $AZURE_RESOURCE_GROUP..."
    az deployment group create --resource-group $AZURE_RESOURCE_GROUP --template-file $TEMPLATE_FILE --parameters resourceNamePrefix=$AZURE_RESOURCE_GROUP
    echo "The Azure resources required for e2e tests have been created, and the resource group name is $AZURE_RESOURCE_GROUP"

    # Set the required environment variables for the e2e test.
    echo "Run the following commands to set the required environment variables for the e2e test:"
    echo "export DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP=$AZURE_RESOURCE_GROUP"
    echo "export DAPR_E2E_TEST_FUNCCAPPS_NAME=$AZURE_RESOURCE_GROUP-funcapp"
}

# Function to delete the Azure resources created for e2e tests.
azfunc_cleanup() {
    if [[ -z $DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP ]]; then
        echo "The environment variable DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP is not set"
        exit 1
    fi

    echo "Cleaning up the Azure resources created for e2e tests in the resource group $DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP..."
    az deployment group delete --resource-group $DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP --name DeployFunctionsOnAca --no-wait
    az group delete --name $DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP --yes --no-wait
    echo "The Azure resources created for e2e tests and the resource group $2 have been deleted"
}

# Function to deploy the Azure Functions container image.
azfunc_deploy() {
    if [[ -z $DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP ]]; then
        echo "The environment variable DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP is not set"
        exit 1
    fi
    if [[ -z $DAPR_E2E_TEST_FUNCCAPPS_NAME ]]; then
        echo "The environment variable DAPR_E2E_TEST_FUNCCAPPS_NAME is not set"
        exit 1
    fi
    if [[ -z $DAPR_E2E_TEST_APP_REGISTRY ]]; then
        echo "The environment variable DAPR_E2E_TEST_APP_REGISTRY is not set"
        exit 1
    fi
    if [[ -z $DAPR_E2E_TEST_APP_TAG ]]; then
        echo "The environment variable DAPR_E2E_TEST_APP_TAG is not set"
        exit 1
    fi
    if [[ -z $2 ]]; then
        echo "appname is not specified"
        exit 1
    fi

    AZURE_FUNCTION_APP_NAME=$DAPR_E2E_TEST_APP_REGISTRY/$2:$DAPR_E2E_TEST_APP_TAG

    echo "Setting the container configuration to $AZURE_FUNCTION_APP_NAME..."
    az functionapp config container set --resource-group $DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP \
    --name $DAPR_E2E_TEST_FUNCCAPPS_NAME --image $AZURE_FUNCTION_APP_NAME \
    --max-replicas 3 --min-replicas 1
}

# Function to get the URL of the specified Azure Function App.
azfunc_geturl() {
    if [[ -z $DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP ]]; then
        echo "The environment variable DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP is not set"
        exit 1
    fi
    if [[ -z $DAPR_E2E_TEST_FUNCCAPPS_NAME ]]; then
        echo "The environment variable DAPR_E2E_TEST_FUNCCAPPS_NAME is not set"
        exit 1
    fi
    az functionapp show --name $DAPR_E2E_TEST_FUNCCAPPS_NAME --resource-group $DAPR_E2E_TEST_FUNCCAPPS_RESOURCE_GROUP --query defaultHostName -otsv
}

# Function to handle the main functionality
main_function() {
    case $1 in
        init )
            azfunc_init
            ;;
        cleanup )
            azfunc_cleanup
            ;;
        deploy )
            azfunc_deploy "$@"
            ;;
        geturl )
            azfunc_geturl
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