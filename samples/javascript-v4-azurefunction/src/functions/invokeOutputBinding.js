const { app, output, trigger } = require('@azure/functions');

const daprInvokeOutput = output.generic({
    type: "daprInvoke",
    direction: "out",
    appId: "{appId}",
    methodName: "{methodName}",
    httpVerb: "post",
    name: "invokePayload"
});

app.generic('InvokeOutputBinding', {
    trigger: trigger.generic({
        type: 'httpTrigger',
        authLevel: 'anonymous',
        methods: ['POST'],
        route: "invoke/{appId}/{methodName}",
        name: "req"
    }),
    return: daprInvokeOutput,
    handler: async (request, context) => {
        context.log("Node HTTP trigger function processed a request.");

        const payload = await request.text();
        context.log(JSON.stringify(payload));
        
        return { body: payload };
    }
});