using namespace System.Net

# Input bindings are passed in via param block.
param($req, $TriggerMetadata)

# Write to the Azure Functions log stream.
Write-Host "Powershell InvokeOutputBinding processed a request."

$req_body = $req.Body

$invoke_output_binding_req_body = @{
    "body" = $req_body
}

# Associate values to output bindings by calling 'Push-OutputBinding'.
Push-OutputBinding -Name payload -Value $invoke_output_binding_req_body

Push-OutputBinding -Name res -Value ([HttpResponseContext]@{
    StatusCode = [HttpStatusCode]::OK
    Body = $req_body
})