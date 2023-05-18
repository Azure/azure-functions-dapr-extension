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
| DOCKER_REGISTRY_ID | Username for Docker registry, required for uploading sample image|
| DOCKER_REGISTRY_PASS | Password for Docker registry, required for uploading sample image|
| DOCKER_REGISTRY | URL to Docker registry, required for uploading sample image |
| AZCOPY_SPA_APPLICATION_ID | Service principal application ID for AzCopy, required for uploading NuGet packages |
| AZCOPY_SPA_CLIENT_SECRET | Service principal client secret for AzCopy, required for uploading NuGet packages |
| GITHUB_TOKEN | GitHub token, required for creating release |

Notes
- `GITHUB_TOKEN` is automatically set by GitHub Actions, so you don't need to set it manually.
- `AZCOPY_*` secrets should not be set for forks, as they are only required for uploading NuGet packages for official release. The step that requires these secrets will be skipped for forks.

4. Go to `Settings` -> `Actions` -> `General` and make sure to allow running GitHub Actions.


