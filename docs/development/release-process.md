# Release Process

This document describes the release process for Azure Functions Dapr Extension.

## Trigger a release

1. Create a new branch from `master` branch in the format of `release-<major>.<minor>` (e.g. `release-0.14`), from the GitHub UI.
2. Add a tag and push it.
```bash
$ git checkout release-0.14
$ git tag "v0.14.0-preview01" -m "v0.14.0-preview01"
$ git push --tags
```
3. CI will create a new release in GitHub, push the NuGet packages to Azure blob storage, and upload the sample image to Docker registry.
4. [MICROSOFT PROCESS] Upload the NuGet packages to NuGet.org using the Azure DevOps pipeline.
5. Edit the release notes if necessary.
6. Test and validate the functionalities with the specific version
7. If there are regressions and bugs, fix them in `release-*` branch and merge back to master
8. Create new tags (with suffix -preview02, -preview03, etc.) and push them to trigger CI to create new releases
9. Repeat from 6 to 8 until all bugs are fixed

## Releasing Azure Functions Dapr Extension Java Library Maven Packages
After completing steps 1 to 3 mentioned above, Maven packages will be uploaded to Azure blob storage. To release Java Maven packages to maven.apache.org, follow the tasks outlined below:

- [MICROSOFT PROCESS] Upload the Maven packages to maven.apache.org by executing the [Azure DevOps pipeline](https://dev.azure.com/azure-sdk/internal/_build?definitionId=1809&_a=summary).
- For comprehensive guidance on releasing Maven packages, refer to the [Microsoft internal doc for releasing maven packages](https://dev.azure.com/azure-sdk/internal/_wiki/wikis/internal.wiki/1/Partner-Release-Pipeline?anchor=java-to-maven-central-(via-oss.sonatype.org)).
