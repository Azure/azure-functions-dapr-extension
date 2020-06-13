module.exports = async function (context, req) {
    context.log("Node HTTP trigger function processed a request.");
    context.bindings.output = { body: req.body };
    context.done(null);
};