module.exports = async function (context) {
    context.log("Node HTTP trigger function processed a request.");
    context.bindings.messages = { "data": context.bindings.args };
};