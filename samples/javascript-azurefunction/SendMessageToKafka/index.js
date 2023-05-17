module.exports = async function (context) {
    context.log("Node function processed a SendMessageToKafka request from the Dapr Runtime.");
    context.bindings.messages = { "data": context.bindings.args };
};