// The function is triggered by Kafka messages in the Kafka instance referenced by
// the Kafka binding configured under components/kafka-bindings.yaml
// Can be used as an alternative for the node-app in the Dapr Bindings sample
// found at https://github.com/dapr/quickstarts/tree/master/bindings/nodeapp
module.exports = async function (context) {
    context.log("Node function processed a ConsumeMessageFromKafka request from the Dapr Runtime.");
    context.log(`Trigger data: ${JSON.stringify(context.bindings.triggerData)}`);
};