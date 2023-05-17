module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a StateOutputBinding request.');

    context.bindings.state = {
        value: req.body
    };
};