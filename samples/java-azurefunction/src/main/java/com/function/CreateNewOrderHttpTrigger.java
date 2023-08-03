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
import com.microsoft.azure.functions.dapr.annotation.DaprStateOutput;
import com.microsoft.azure.functions.OutputBinding;

import java.util.Optional;

/**
 * Azure Functions with HTTP Trigger.
 */
public class CreateNewOrderHttpTrigger {
    /**
     * This function listens at endpoint "/api/CreateNewOrderHttpTrigger". Two ways to invoke it using "curl" command in bash:
     * 1. curl -d "HTTP Body" {your host}/api/CreateNewOrderHttpTrigger
     * 2. curl -X POST \                                                         ─╯
            http://localhost:7071/api/CreateNewOrderHttpTrigger \
            -H 'Content-Type: application/json' \
            -d '{
                "value": {
                    "orderId": "45"
                }
            }'
     */
    @FunctionName("CreateNewOrderHttpTrigger")
    public String run(
            @HttpTrigger(
                name = "req",
                methods = {HttpMethod.POST},
                authLevel = AuthorizationLevel.ANONYMOUS)
                HttpRequestMessage<Optional<String>> request,
            @DaprStateOutput(
                stateStore = "%StateStoreName%",
                key = "product")
            OutputBinding<String> product,
            final ExecutionContext context) {
        context.getLogger().info("Java HTTP trigger (CreateNewOrderHttpTrigger) processed a request.");

        // Read the request body
        String requestBody = request.getBody().orElse(null);

        product.setValue(requestBody);

        return requestBody;
    }
}
