# EndToEnd Tests

## Scratch

- cross platform build support in makefile

## Overview

The e2e tests are designed to test the extension in a real environment, and consist of the following components:

1. Test apps: Written in various supported languages, each test app is a functions app containing multiple functions for different scenarios. Then test apps can be run locally, or deployed to Functions on CApps, CApps, or AKS.
1. Test cases: Test cases are written in dotnet, and are designed to run against test apps of different languages. The tests can be run locally or in a CI pipeline, and just need to know the test app's URL (which is injected as an environment variable).
1. Infrastructure setup: The infrastructure setup is a set of scripts that can be used to deploy the test apps to an environment, and run the test cases against the test apps. It is also responsible for provisioning the environment, and cleaning up after the tests are done.

## Pre-requisites

- [Docker](https://docs.docker.com/get-docker/) (with buildx support) to build and run the test apps
 
## Running the tests

### Prepare the test app images

```bash
# Set the required environment variables.
export E2E_TEST_APP_REGISTRY=myregistry.azurecr.io
export E2E_TEST_APP_TAG=dev

# Build and push the test apps.
# You can also build and push individual test apps.
# make build-e2e-app-csharpapp
# make push-e2e-app-csharpapp
make build-e2e-app-all
make push-e2e-app-all
```

### Run the tests locally

## Adding a new test