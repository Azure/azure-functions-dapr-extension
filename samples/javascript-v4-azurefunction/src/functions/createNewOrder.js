const { app, output, trigger } = require('@azure/functions');

const daprStateOuput = output.generic({
    type: "daprState",
    stateStore: "%StateStoreName%",
    direction: "out",
    name: "order",
    key: "order"
});

app.generic('CreateNewOrder', {
    trigger: trigger.generic({
        type: 'daprServiceInvocationTrigger',
        name: "payload"
    }),
    return: daprStateOuput,
    handler: async (request, context) => {
        context.log("Node function processed a CreateNewOrder request from the Dapr Runtime.");
        context.log(context.triggerMetadata.payload.data)

       return context.triggerMetadata.payload.data;
    }
});