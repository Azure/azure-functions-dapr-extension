module.exports = async function (context) {
    context.log("Node function processed a CreateNewOrder request from the Dapr Runtime.");
    context.bindings.order = {"value": context.bindings.payload["data"]};
};