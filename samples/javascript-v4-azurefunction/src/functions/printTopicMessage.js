const { app, trigger } = require('@azure/functions');

app.generic('PrintTopicMessage', {
    trigger: trigger.generic({
        type: 'daprTopicTrigger',
        name: "subEvent",
        pubsubname: "%PubSubName%",
        topic: "B"
    }),
    handler: async (request, context) => {
        context.log("Node function processed a PrintTopicMessage request from the Dapr Runtime.");
        context.log(`Topic B received a message: ${JSON.stringify(context.triggerMetadata.subEvent.data)}.`);
    }
});