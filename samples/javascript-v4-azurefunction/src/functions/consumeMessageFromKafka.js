const { app, trigger } = require('@azure/functions');

app.generic('ConsumeMessageFromKafka', {
    trigger: trigger.generic({
        type: 'daprBindingTrigger',
        bindingName: "%KafkaBindingName%",
        name: "triggerData"
    }),
    handler: async (request, context) => {
        context.log("Node function processed a ConsumeMessageFromKafka request from the Dapr Runtime.");
        context.log(context.triggerMetadata.triggerData)
    }
});