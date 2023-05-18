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
| DOCKER_REGISTRY | URL to Docker registry, required for uploading sample image|
| GITHUB_TOKEN | Token to publish binaries to GitHub during release (*automatically created by GitHub*) |
| NUGETORG_DAPR_API_KEY | API key to publish package to NuGet |

4. Go to `Settings` -> `Actions` -> `General` and make sure to allow running GitHub Actions.


