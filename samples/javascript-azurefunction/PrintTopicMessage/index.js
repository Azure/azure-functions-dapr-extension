module.exports = async function (context) {
    context.log("Node function processed a PrintTopicMessage request from the Dapr Runtime.");
    context.log(`Topic B received a message: ${JSON.stringify(context.bindings.subEvent.data)}.`);
};