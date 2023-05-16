module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed StateInputBinding request.');
    context.log('Current state of this function: ' + JSON.stringify(context.bindings.state));
};