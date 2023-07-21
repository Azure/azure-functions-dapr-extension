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
import com.microsoft.azure.functions.dapr.annotation.DaprTopicTrigger;
import com.microsoft.azure.functions.dapr.annotation.DaprPublishOutput;
import com.microsoft.azure.functions.OutputBinding;

import java.util.Optional;

/**
 * Azure Functions with Dapr service invocation trigger.
 */
public class TransferEventBetweenTopics {
    /**
     * TODO: Add description to method
     */
    @FunctionName("TransferEventBetweenTopics")
    public String run(
            @DaprTopicTrigger(
                name = "topicMessage",
                pubSubName = "%PubSubName%",
                topic = "A")
            String topicMessage,
            @DaprPublishOutput(
                name = "state",
                pubSubName = "%PubSubName%",
                topic = "B")
            OutputBinding<String> payload,
            final ExecutionContext context) {
        context.getLogger().info("Java function processed a TransferEventBetweenTopics request from the Dapr Runtime.");


        payload.setValue("Transfer from Topic A: " + topicMessage);

        return topicMessage;
    }
}
