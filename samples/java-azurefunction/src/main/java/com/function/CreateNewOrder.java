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
 * Azure Functions with Dapr service invocation trigger.
 */
public class CreateNewOrder {
    /**
     * This function gets invoked by dapr runtime:
     * dapr invoke --app-id functionapp --method CreateNewOrder --data '{"value": { "orderId": "41" } }'
     */
    @FunctionName("CreateNewOrder")
    public String run(
            @DaprServiceInvocationTrigger(
                methodName = "CreateNewOrder") 
            String payload,
            @DaprStateOutput(
                stateStore = "%StateStoreName%",
                key = "order")
            OutputBinding<String> product,
            final ExecutionContext context) {
        context.getLogger().info("'Java function processed a CreateNewOrder request from the Dapr Runtime.'");
        product.setValue(payload);

        return payload;
    }
}
