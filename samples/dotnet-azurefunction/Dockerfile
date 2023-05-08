FROM mcr.microsoft.com/dotnet/sdk:6.0 AS installer-env

# Copy the Microsoft.Azure.WebJobs.Extensions.Dapr, style cop, and dotnet sample into the installer-env to build
COPY /src/Microsoft.Azure.WebJobs.Extensions.Dapr /src/src/Microsoft.Azure.WebJobs.Extensions.Dapr
COPY /src/Microsoft.Azure.Functions.Extensions.Dapr.Core /src/src/Microsoft.Azure.Functions.Extensions.Dapr.Core
COPY /.stylecop /src/.stylecop
COPY /samples/dotnet-azurefunction /src/samples/dotnet-function-app

# Build project
RUN cd /src/samples/dotnet-function-app && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --output /home/site/wwwroot

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet:4.0-appservice
FROM mcr.microsoft.com/azure-functions/dotnet:4.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]
