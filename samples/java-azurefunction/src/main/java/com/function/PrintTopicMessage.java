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
import com.microsoft.azure.functions.dapr.annotation.DaprStateInput;
import com.microsoft.azure.functions.dapr.annotation.DaprStateOutput;
import com.microsoft.azure.functions.dapr.annotation.DaprTopicTrigger;
import com.microsoft.azure.functions.OutputBinding;

import java.util.Optional;
import java.util.logging.Logger;

/**
 * Azure Functions with DaprTopicTrigger.
 */
public class PrintTopicMessage {
    /**
     * This function gets invoked by dapr runtime, when a message is published to topic B.
     */
    @FunctionName("PrintTopicMessage")
    public String run(
            @DaprTopicTrigger(
                pubSubName = "%PubSubName%",
                topic = "B")
            String payload,
            final ExecutionContext context) throws JsonProcessingException {
        Logger logger = context.getLogger();
        logger.info("Java function processed a PrintTopicMessage request from the Dapr Runtime.");

        // Get the CloudEvent data from the request body as a JSON string
        ObjectMapper objectMapper = new ObjectMapper();
        JsonNode jsonNode = objectMapper.readTree(payload);

        String data = jsonNode.get("data").asText();

        logger.info("Topic B received a message: " + data);

        return data;
    }
}
