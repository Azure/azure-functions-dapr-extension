# Setup Continuous Integration

This repository uses GitHub Actions to automate the build and release. As long as you have a GitHub Account, you can set up your own private Actions in your own fork. This document helps you set up the continuous integration for Azure Functions Dapr Extension.

## Prerequisites

A GitHub account.

## Setup

1. Fork [Azure/azure-functions-dapr-extension](https://github.com/Azure/azure-functions-dapr-extension) to your GitHub account.
2. Go to `Settings`-> `Secrets and variables` -> `Actions`.
3. Add the required repository secrets.

### Required repository secrets

| Name | Description |
|--|--|
| DOCKER_REGISTRY_URL | URL of Docker registry, required for logging in to Docker registry, e.g. `myregistry.azurecr.io` |
| DOCKER_REGISTRY_PATH | Path to store Docker images, required for uploading sample image, e.g. `samples/dotnet` |
| DOCKER_REGISTRY_ID | Username for Docker registry, required for uploading sample image |
| DOCKER_REGISTRY_PASS | Password for Docker registry, required for uploading sample image |
| AZCOPY_SPA_APPLICATION_ID | Service principal application ID for AzCopy, required for uploading NuGet and Maven artifacts |
| AZCOPY_SPA_CLIENT_SECRET | Service principal client secret for AzCopy, required for uploading NuGet and Maven artifacts |
| AZCOPY_TENANT_ID | Tenant ID used by AzCopy to authenticate, required for uploading NuGet and Maven artifacts |
| GITHUB_TOKEN | GitHub token, required for creating release |
| CODECOV_TOKEN | [Codecov token](#generate-codecov_token), required for uploading code coverage results |

Notes
- `GITHUB_TOKEN` is automatically set by GitHub Actions, so you don't need to set it manually.
- `AZCOPY_*` secrets should not be set for forks, as they are only required for uploading NuGet packages for official release. The step that requires these secrets will be skipped for forks.

4. Go to `Settings` -> `Actions` -> `General` and make sure to allow running GitHub Actions.

### Generate CODECOV_TOKEN
To generate`CODECOV_TOKEN`, go to [this website](https://app.codecov.io/github/azure/azure-functions-dapr-extension/settings) and generate the upload token. Same should be updated in GitHub secrets for `CODECOV_TOKEN`. Make sure you are part of Azure GitHub Org and are owner of `azure-functions-dapr-extension` GitHub repo.


