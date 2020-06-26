module.exports = async function (context) {
    context.log("Node function processed a RetrieveSecret request from the Dapr Runtime.");

    // print the fetched secret value
    for( var key in context.bindings.secret)
    {
        context.log(`Stored secret: Key = ${key}, Value =${context.bindings.secret[key]}`);
    }
};