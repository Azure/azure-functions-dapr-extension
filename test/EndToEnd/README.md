# EndToEnd Tests

## TODO

Must be addressed before merging the code to master
1. In local environment, allow saving logs from app and dapr to a local folder.

## Overview

The e2e tests are designed to test the extension in a real environment, and consist of the following components:

1. Test apps: Written in various supported languages, each test app is a functions app containing multiple functions for different scenarios. Then test apps can be run locally, or deployed to Functions on CApps, CApps, or AKS.
1. Test cases: Test cases are written in dotnet, and are designed to run against test apps of different languages. The tests can be run locally or in a CI pipeline, and just need to know the test app's URL (which is provided by the test fixture).
1. Test fixture: The test fixture is reponsible for setting up the test environment, starting and cleaning up the test apps, and providing the test app's URL to the test cases.
1. Infrastructure setup: The infrastructure setup is a set of scripts to provision the required infrastructure for the test apps to run. This includes creating a Kubernetes cluster, installing Dapr, and installing the required Dapr components.

## Pre-requisites

- [Docker](https://docs.docker.com/get-docker/) (with buildx support) to build and run the test apps.
- [Dapr](https://dapr.io) installed locally with `daprd` added to the path - for running the test apps locally.
- TODO: infrastructure setup pre-requisites (including Dapr components)
 
## Running the tests

### Prepare the test app images

```bash
# Set the required environment variables.
export DAPR_E2E_TEST_APP_REGISTRY=myregistry.azurecr.io
export DAPR_E2E_TEST_APP_TAG=dev

# Build and push the test apps.
# You can also build and push individual test apps.
# make build-e2e-app-csharpapp
# make push-e2e-app-csharpapp
make build-e2e-app-all
make push-e2e-app-all
```

### Run the tests locally

```bash
DAPR_E2E_TEST_APP_ENVIRONMENT=local dotnet test
```

### Run the tests on Functions on CApps

```bash
# Initializes the test environment and sets the required environment variables.
./Framework/Scripts/manage-azfunc.sh init

# Runs the tests.
DAPR_E2E_TEST_APP_ENVIRONMENT=funccapps dotnet test

# Cleans up the test environment.
./Framework/Scripts/manage-azfunc.sh clean
```

## Adding a new test

TODO