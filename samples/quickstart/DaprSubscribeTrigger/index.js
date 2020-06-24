module.exports = async function (context) {
    context.log("Node function processed a Topic subscribe request from the Dapr Runtime.");
    context.log(`Topic B received a message: ${context.bindings.daprTrigger.data}.`);
};