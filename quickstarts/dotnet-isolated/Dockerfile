FROM mcr.microsoft.com/dotnet/sdk:6.0 AS installer-env

COPY /src/Microsoft.Azure.Functions.Worker.Extensions.Dapr /src/src/Microsoft.Azure.Functions.Worker.Extensions.Dapr
COPY /src/Microsoft.Azure.WebJobs.Extensions.Dapr /src/src/Microsoft.Azure.WebJobs.Extensions.Dapr
COPY /src/Microsoft.Azure.Functions.Extensions.Dapr.Core /src/src/Microsoft.Azure.Functions.Extensions.Dapr.Core
COPY /.stylecop /src/.stylecop
COPY /quickstarts/dotnet-isolated /src/quickstarts/dotnet-isolated
COPY /local-packages /src/local-packages

RUN cd /src/quickstarts/dotnet-isolated && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --configfile nuget.config --output /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated6.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]