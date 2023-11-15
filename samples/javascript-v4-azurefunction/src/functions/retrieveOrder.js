const { app, input, trigger } = require('@azure/functions');

const daprStateInput = input.generic({
    type: "daprState",
    stateStore: "%StateStoreName%",
    direction: "in",
    name: "order",
    key: "order"
});

app.generic('RetrieveOrder', {
    trigger: trigger.generic({
        type: 'daprServiceInvocationTrigger',
        name: "payload"
    }),
    extraInputs: [daprStateInput],
    handler: async (request, context) => {
        context.log("Node function processed a RetrieveOrder request from the Dapr Runtime.");
        const daprStateInputValue = context.extraInputs.get(daprStateInput);
        // print the fetched state value
        context.log(daprStateInputValue);
    }
});