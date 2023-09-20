using namespace System
using namespace Microsoft.Azure.WebJobs
using namespace Microsoft.Extensions.Logging
using namespace Microsoft.Azure.WebJobs.Extensions.Dapr
using namespace Newtonsoft.Json.Linq

# Example to use Dapr Service Invocation Trigger and Dapr State Output binding to persist a new state into statestore
param (
    $payload, $order
)

# C# function processed a CreateNewOrder request from the Dapr Runtime.
Write-Host "PowerShell function processed a RetrieveOrder request from the Dapr Runtime."

# Convert the object to a JSON-formatted string with ConvertTo-Json
$jsonString = $order | ConvertTo-Json

Write-Host "$jsonString"
