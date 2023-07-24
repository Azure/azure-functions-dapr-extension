/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 */

package main.java.com.function;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
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

import java.net.URI;
import java.util.Optional;

/**
 * Azure Functions with DaprTopicTrigger and DaprPublishOutput binding.
 */
public class TransferEventBetweenTopics {
    /**
     * This function gets invoked by dapr runtime:
     * dapr publish  --publish-app-id functionapp --pubsub messagebus --topic A --data 'This is a test' 
     */
    @FunctionName("TransferEventBetweenTopics")
    public String run(
            @DaprTopicTrigger(
                name = "topicMessage",
                pubSubName = "%PubSubName%",
                topic = "A")
                String request,
            @DaprPublishOutput(
                name = "state",
                pubSubName = "%PubSubName%",
                topic = "B")
            OutputBinding<String> payload,
            final ExecutionContext context) throws JsonProcessingException {
        context.getLogger().info("Java function processed a TransferEventBetweenTopics request from the Dapr Runtime.");

        // Get the CloudEvent data from the request body as a JSON string
        ObjectMapper objectMapper = new ObjectMapper();
        JsonNode jsonNode = objectMapper.readTree(request);

        String data = jsonNode.get("data").asText();

        context.getLogger().info("Printing Topic A received a message: " + data);

        String pubsubPayload = String.format("{\"payload\":\" Transfer from Topic A: %s \"}", data);      
        payload.setValue(pubsubPayload);

        return data;
    }
}
