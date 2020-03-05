module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');

    context.log('Current state of this function: ' + context.bindings.daprState);

    context.log('Sending data to function 2 to continue processing');

    context.bindings.daprInvoke = {
        appId: 'function2',
        methodName: 'process',
        httpVerb: 'post',
        body: context.bindings.daprState
    }

    context.res = {
        status: 200
    };
};