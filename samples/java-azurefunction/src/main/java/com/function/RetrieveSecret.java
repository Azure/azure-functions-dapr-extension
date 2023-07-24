package com.function;

import java.util.Map;

import com.microsoft.azure.functions.ExecutionContext;
import com.microsoft.azure.functions.annotation.FunctionName;
import com.microsoft.azure.functions.dapr.annotation.DaprSecretInput;
import com.microsoft.azure.functions.dapr.annotation.DaprServiceInvocationTrigger;

public class RetrieveSecret {
        @FunctionName("RetrieveSecret")
    public void run(
        @DaprServiceInvocationTrigger(
            name = "args", 
            methodName = "RetrieveSecret") Object args,
        @DaprSecretInput(
            name = "secret",
            secretStoreName = "kubernetes", 
            key = "my-secret", 
            metadata = "metadata.namespace=default") 
            Map<String, String> secret,
        final ExecutionContext context) {

        context.getLogger().info("Java function processed a RetrieveSecret request from the Dapr Runtime.");

        // Print the fetched secret value
        // This is only for demo purpose
        // Please do not log any real secret in your production code
        for (Map.Entry<String, String> entry : secret.entrySet()) {
            context.getLogger().info(String.format("Stored secret: Key = %s, Value = %s", entry.getKey(), entry.getValue()));
        }
    }
}
