FROM mcr.microsoft.com/dotnet/sdk:6.0 AS installer-env

# Copy the extension source
COPY /src/Microsoft.Azure.WebJobs.Extensions.Dapr /src/src/Microsoft.Azure.WebJobs.Extensions.Dapr
COPY /src/Microsoft.Azure.Functions.Extensions.Dapr.Core /src/src/Microsoft.Azure.Functions.Extensions.Dapr.Core

# Copy the test app
COPY /test/EndToEnd/Apps/csharpapp /src/test/EndToEnd/Apps/csharpapp

# Build project
RUN cd /src/test/EndToEnd/Apps/csharpapp && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --output /home/site/wwwroot

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet:4.0-appservice
FROM mcr.microsoft.com/azure-functions/dotnet:4.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]
