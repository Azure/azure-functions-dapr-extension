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
import com.microsoft.azure.functions.dapr.annotation.DaprServiceInvocationTrigger;
import com.microsoft.azure.functions.dapr.annotation.DaprStateOutput;
import com.microsoft.azure.functions.OutputBinding;

import java.util.Optional;

/**
 * Azure Functions with HTTP Trigger.
 */
public class CreateNewOrderServiceInvocation {
    /**
     * This function listens at endpoint "/api/HttpExample". Two ways to invoke it using "curl" command in bash:
     * 1. curl -d "HTTP Body" {your host}/api/HttpExample
     * 2. curl "{your host}/api/HttpExample?name=HTTP%20Query"
     */
    @FunctionName("CreateNewOrderServiceInvocation")
    public String run(
            @DaprServiceInvocationTrigger(
                name = "payload", 
                methodName = "CreateNewOrderServiceInvocation") 
            String payload,
            @DaprStateOutput(
                name = "state",
                stateStore = "statestore",
                key = "CreateNewOrderServiceInvocation")
            OutputBinding<String> product,
            final ExecutionContext context) {
        context.getLogger().info("Java HTTP trigger processed a request.");

        // Parse query parameter
        // final String query = request.getQueryParameters().get("name");
        // final String name = request.getBody().orElse(query);

        product.setValue("{\"value\":{\"orderId\":\"43\"}}");

        return "{\"value\":{\"orderId\":\"43\"}}";
    }
}