package com.function;

import com.microsoft.azure.functions.ExecutionContext;
import com.microsoft.azure.functions.annotation.FunctionName;
import com.microsoft.azure.functions.dapr.annotation.DaprBindingTrigger;

public class ConsumeMessageFromKafka {
    /**
    * The function is triggered by Kafka messages in the Kafka instance referenced by
    * the Kafka binding configured under components/kafka-bindings.yaml
    * Can be used as an alternative for the node-app in the Dapr Bindings sample
    * found at https://github.com/dapr/quickstarts/tree/master/bindings/nodeapp
     */
    @FunctionName("ConsumeMessageFromKafka")
    public String run(
            @DaprBindingTrigger(
                name = "triggerData", 
                bindingName = "%KafkaBindingName%") 
            String triggerData,
            final ExecutionContext context) {
        context.getLogger().info("'Java function processed a ConsumeMessageFromKafka request from the Dapr Runtime.'");
        context.getLogger().info("Hello from Kafka!");

        context.getLogger().info(String.format("Trigger data: %s", triggerData));

        return triggerData;
    }
    
}
