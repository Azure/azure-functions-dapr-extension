module.exports = async function (context) {
    let args = context.bindings.daprInput;
    context.log(`JavaScript processed a request from the Dapr runtime. Content: ${JSON.stringify(args)}`);

    // input binding
    const [operandOne, operandTwo] = [Number(args['arg1']), Number(args['arg2'])];

    // the return value of the function is the output
    let result = operandOne / operandTwo;
    return result.toString();
};
