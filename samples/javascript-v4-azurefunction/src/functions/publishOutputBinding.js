const { app, output, trigger } = require('@azure/functions');

const daprPublishOutput = output.generic({
    type: "daprPublish",
    direction: "out",
    pubsubname: "%PubSubName%",
    topic: "{topicName}",
    name: "payload"
});

app.generic('PublishOutputBinding', {
    trigger: trigger.generic({
        type: 'httpTrigger',
        authLevel: 'anonymous',
        methods: ['POST'],
        route: "topic/{topicName}",
        name: "req"
    }),
    return: daprPublishOutput,
    handler: async (request, context) => {
        context.log("Node HTTP trigger function processed a request.");
        const payload = await request.text();
        context.log(JSON.stringify(payload));

        return { payload: payload };
    }
});