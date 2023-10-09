module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');

    let state = context.bindings.stateIn || "";
    context.log(`Current state:\n${state}\n`);

    // update state
    state = `${state}\n${req.body}`;

    context.log(`Updated state:\n${state}\n`);

    // save state using the Dapr output binding
    context.bindings.stateOut = 
    {
        "value": state,
        // "key": "{Optional. We defined in function.json}",
        // "etag": "{Optional. The etag value of the state record.}"
    };

    // publish a message using the Dapr topic publish output binding
    context.bindings.publish = 
    {
        "payload": state,
        "pubsubname": "pubsub",
        "topic": "myTopic"
    }

    // return an http response using the http output binding
    context.res = {
        // status: 200, /* Defaults to 200 */
        body: `State now updated to: \n${state}`
    };
};