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
import com.microsoft.azure.functions.dapr.annotation.DaprTopicTrigger;
import com.microsoft.azure.functions.OutputBinding;

import java.util.Optional;
import java.util.logging.Logger;

/**
 * Azure Functions with HTTP Trigger.
 */
public class PrintTopicMessage {
    /**
     * TODO: Add description to method
     */
    @FunctionName("PrintTopicMessage")
    public String run(
            @DaprTopicTrigger(
                name = "payload",
                pubSubName = "%PubSubName%",
                topic = "B")
            String payload,
            final ExecutionContext context) {
        Logger logger = context.getLogger();
        logger.info("Java function processed a PrintTopicMessage request from the Dapr Runtime.");
        logger.info("Topic B received a message: " + payload);

        return payload;
    }
}
