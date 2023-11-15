const { app, output, trigger } = require('@azure/functions');

const daprStateOuput = output.generic({
    type: "daprState",
    stateStore: "%StateStoreName%",
    direction: "out",
    name: "order",
    key: "{key}"
});

app.generic('StateOutputBinding', {
    trigger: trigger.generic({
        type: 'httpTrigger',
        authLevel: 'anonymous',
        methods: ['POST'],
        route: "state/{key}",
        name: "req"
    }),
    return: daprStateOuput,
    handler: async (request, context) => {
        context.log("Node HTTP trigger function processed a request.");

        const payload = await request.text();
        context.log(JSON.stringify(payload));
        
        return { value : payload };
    }
});