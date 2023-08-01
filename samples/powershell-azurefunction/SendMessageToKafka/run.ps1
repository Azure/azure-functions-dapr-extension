using namespace System.Net

# Input bindings are passed in via param block.
param($req, $TriggerMetadata)

Write-Host "Powershell SendMessageToKafka processed a request."

$invoke_output_binding_req_body = @{
    "data" = $req
}

# Associate values to output bindings by calling 'Push-OutputBinding'.
Push-OutputBinding -Name messages -Value $invoke_output_binding_req_body