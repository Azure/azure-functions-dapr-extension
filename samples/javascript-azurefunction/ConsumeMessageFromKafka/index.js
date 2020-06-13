// The function is triggered by Kafka messages in the Kafka instance referenced by
// the Kafka binding configured under components/kafka-bindings.yaml
// Can be used as an alternative for the node-app in the Dapr Bindings sample
// found at https://github.com/dapr/samples/tree/master/5.bindings/nodeapp
module.exports = async function (context) {
    context.log("Hello from Kafka!");

    context.log(`Trigger data: ${context.bindings.triggerData}`);
};