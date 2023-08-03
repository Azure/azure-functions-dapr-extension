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
import com.microsoft.azure.functions.dapr.annotation.DaprStateInput;
import com.microsoft.azure.functions.dapr.annotation.DaprStateOutput;
import com.microsoft.azure.functions.OutputBinding;

import java.util.Optional;
import java.util.logging.Logger;

/**
 * Azure Functions with HTTP Trigger.
 */
public class RetrieveOrder {
    /**
     * This function gets invoked by dapr runtime:
     * dapr invoke --app-id functionapp --method RetrieveOrder
     */
    @FunctionName("RetrieveOrder")
    public String run(
            @DaprServiceInvocationTrigger(
                methodName = "RetrieveOrder") 
            String payload,
            @DaprStateInput(
                stateStore = "%StateStoreName%",
                key = "order")
            String product,
            final ExecutionContext context) {
        Logger logger = context.getLogger();
        logger.info("Java function processed a RetrieveOrder request from the Dapr Runtime.");
        logger.info(product);

        return product;
    }
}
