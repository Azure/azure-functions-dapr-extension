module.exports = async function (context) {
    context.log("Node function processed a TransferEventBetweenTopics request from the Dapr Runtime.");

    context.bindings.pubEvent = { "payload": "Transfer from Topic A: " + JSON.stringify(context.bindings.subEvent.data) };
}