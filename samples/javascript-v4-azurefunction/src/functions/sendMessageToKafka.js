const { app, output, trigger } = require('@azure/functions');

const daprBindingOuput = output.generic({
    type: "daprBinding",
    direction: "out",
    bindingName: "%KafkaBindingName%",
    operation: "create",
    name: "messages"
});

app.generic('SendMessageToKafka', {
    trigger: trigger.generic({
        type: 'daprServiceInvocationTrigger',
        name: "payload"
    }),
    return: daprBindingOuput,
    handler: async (request, context) => {
        context.log("Node function processed a SendMessageToKafka request from the Dapr Runtime.");
        context.log(context.triggerMetadata.payload)

        return { "data": context.triggerMetadata.payload };
    }
});