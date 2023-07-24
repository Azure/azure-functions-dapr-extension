/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 */

package main.java.com.function;
import com.microsoft.azure.functions.ExecutionContext;
import com.microsoft.azure.functions.HttpMethod;
import com.microsoft.azure.functions.HttpRequestMessage;
import com.microsoft.azure.functions.HttpResponseMessage;
import com.microsoft.azure.functions.HttpStatus;
import com.microsoft.azure.functions.annotation.AuthorizationLevel;
import com.microsoft.azure.functions.annotation.FunctionName;
import com.microsoft.azure.functions.annotation.HttpTrigger;
import com.microsoft.azure.functions.dapr.annotation.DaprInvokeOutput;
import com.microsoft.azure.functions.dapr.annotation.DaprStateOutput;
import com.microsoft.azure.functions.OutputBinding;

import java.util.Optional;

/**
 * This function uses Dapr Invoke Output Binding to invoke another Dapr enabled function.
 * This function can be invoked by http trigger.
 */
public class InvokeOutputBinding {
    /**
     * This function listens at endpoint "/api/invoke/{appId}/{methodName}". Curl command to invoke it:
     * curl -X POST \                                                         ─╯
            http://localhost:7071/api/invoke/functionapp/CreateNewOrder \
            -H 'Content-Type: application/json' \
            -d '{
                "body":{
                    "value": {
                        "orderId": "45"
                    }
                }
            }'
     */
    @FunctionName("InvokeOutputBinding")
    public String run(
            @HttpTrigger(
                name = "req",
                methods = {HttpMethod.GET},
                authLevel = AuthorizationLevel.ANONYMOUS,
                route = "invoke/{appId}/{methodName}")
                HttpRequestMessage<Optional<String>> request,
            @DaprInvokeOutput(
                name = "payload",
                appId = "{appId}", 
                methodName = "{methodName}", 
                httpVerb = "post")
            OutputBinding<String> payload,
            final ExecutionContext context) {
        context.getLogger().info("Java HTTP trigger processed a request.");

        // Parse query parameter
        final String query = request.getQueryParameters().get("name");
        final String body = request.getBody().orElse(query);

        //request body must be passed this way "{\"body\":{\"value\":{\"key\":\"some value\"}}}" to use the InvokeOutputBinding, all the data must be enclosed in body property.
        String jsoString = String.format("{\"body\":\"%s\"}", body);

        payload.setValue(jsoString);

        return jsoString;
    }
}
