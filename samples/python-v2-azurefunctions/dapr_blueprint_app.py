import logging 
import azure.functions as func 

app = func.DaprBlueprint() 

@app.function_name("BlueprintFunc")
@app.dapr_service_invocation_trigger(arg_name="payload", method_name="BlueprintFunc")
@app.dapr_state_output(arg_name="state", state_store="statestore", key="order123")
def default_template(payload: str, state: func.Out[str]) -> None: 
    logging.info('Python HTTP trigger function processed a request.') 
    # request body must be passed this way '{\"value\": { \"key\": \"some value\" } }'
    state.set(payload)
    