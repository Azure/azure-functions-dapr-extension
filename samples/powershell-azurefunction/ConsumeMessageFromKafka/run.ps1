using namespace System
using namespace Microsoft.Azure.WebJobs
using namespace Microsoft.Extensions.Logging
using namespace Microsoft.Azure.WebJobs.Extensions.Dapr
using namespace Newtonsoft.Json.Linq

param (
    $triggerData
)

Write-Host "PowerShell function processed a ConsumeMessageFromKafka request from the Dapr Runtime."

$jsonString = $triggerData | ConvertTo-Json

Write-Host "Trigger data: $jsonString"

