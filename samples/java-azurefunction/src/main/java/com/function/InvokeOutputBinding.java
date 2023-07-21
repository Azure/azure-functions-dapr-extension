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
 * Azure Functions with HTTP Trigger.
 */
public class InvokeOutputBinding {
    /**
     * TODO: Add description to method
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
        final String name = request.getBody().orElse(query);

        String jsoString = String.format("{\"body\":\"%s\"}", name);

        payload.setValue(jsoString);

        return jsoString;
    }
}
