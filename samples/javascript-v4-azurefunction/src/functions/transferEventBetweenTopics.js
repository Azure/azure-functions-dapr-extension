const { app, output, trigger } = require('@azure/functions');

const daprPublishOutput = output.generic({
    type: "daprPublish",
    direction: "out",
    pubsubname: "%PubSubName%",
    topic: "B",
    name: "payload"
});

app.generic('TransferEventBetweenTopics', {
    trigger: trigger.generic({
        type: 'daprTopicTrigger',
        name: "subEvent",
        pubsubname: "%PubSubName%",
        topic: "A"
    }),
    return: daprPublishOutput,
    handler: async (request, context) => {
        context.log("Node function processed a TransferEventBetweenTopics request from the Dapr Runtime.");
        context.log(context.triggerMetadata.subEvent.data);

        return { payload: context.triggerMetadata.subEvent.data };
    }
});