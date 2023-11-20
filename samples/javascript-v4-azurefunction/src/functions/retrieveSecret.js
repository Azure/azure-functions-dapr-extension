const { app, input, trigger } = require('@azure/functions');

const daprSecretInput = input.generic({
    type: "daprSecret",
    secretStoreName: "localsecretstore",
    metadata: "metadata.namespace=default",
    direction: "in",
    name: "secret",
    key: "my-secret"
});

app.generic('RetrieveSecret', {
    trigger: trigger.generic({
        type: 'daprServiceInvocationTrigger',
        name: "payload"
    }),
    extraInputs: [daprSecretInput],
    handler: async (request, context) => {
        context.log("Node function processed a RetrieveSecret request from the Dapr Runtime.");
        const daprSecretInputValue = context.extraInputs.get(daprSecretInput);

        // print the fetched secret value
        for (var key in daprSecretInputValue) {
            context.log(`Stored secret: Key=${key}, Value=${daprSecretInputValue[key]}`);
        }
    }
});