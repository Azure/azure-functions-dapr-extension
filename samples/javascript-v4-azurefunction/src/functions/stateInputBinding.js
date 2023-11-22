const { app, input, trigger } = require('@azure/functions');

const daprStateInput = input.generic({
    type: "daprState",
    stateStore: "%StateStoreName%",
    direction: "in",
    name: "order",
    key: "{key}"
});

app.generic('StateInputBinding', {
    trigger: trigger.generic({
        type: 'httpTrigger',
        authLevel: 'anonymous',
        methods: ['GET'],
        route: "state/{key}",
        name: "req"
    }),
    extraInputs: [daprStateInput],
    return: output.generic({
        type: "http",
    }),
    handler: async (request, context) => {
        context.log("Node HTTP trigger function processed a request.");

        const daprStateInputValue = context.extraInputs.get(daprStateInput);
        // print the fetched state value
        context.log(daprStateInputValue);

        return { body: daprStateInputValue };
    }
});