package com.function;

import com.microsoft.azure.functions.ExecutionContext;
import com.microsoft.azure.functions.OutputBinding;
import com.microsoft.azure.functions.annotation.FunctionName;
import com.microsoft.azure.functions.dapr.annotation.DaprBindingOutput;
import com.microsoft.azure.functions.dapr.annotation.DaprServiceInvocationTrigger;

public class SendMessageToKafka {
    /**
     * This function gets invoked by dapr runtime:
     * dapr invoke --app-id functionapp --method SendMessageToKafka --data '{"data":{"message": "hello!" }}'
     * Once this function gets invoked, it will send a message to Kafka.
     */
    @FunctionName("SendMessageToKafka")
    public String run(
            @DaprServiceInvocationTrigger(
                name = "payload", 
                methodName = "SendMessageToKafka") 
            String payload,
            @DaprBindingOutput(
                name = "state",
                bindingName = "%KafkaBindingName%", 
                operation = "create")
            OutputBinding<String> product,
            final ExecutionContext context) {
        context.getLogger().info("Java  function processed a SendMessageToKafka request.");
        product.setValue(payload);

        return payload;
    }
}
