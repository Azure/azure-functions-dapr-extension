import logging 
import azure.functions as func 

createNewOrder = func.DaprBlueprint()

# Dapr state output binding with http dapr_service_invocation_trigger
@createNewOrder.function_name(name="CreateNewOrder")
@createNewOrder.dapr_service_invocation_trigger(arg_name="payload", method_name="CreateNewOrder")
@createNewOrder.dapr_state_output(arg_name="state", state_store="%StateStoreName%", key="order")
def main(payload: str, state: func.Out[str] ) :
    # request body must be passed this way '{\"value\": { \"key\": \"some value\" } }'
    logging.info('Python function processed a CreateNewOrder request from the Dapr Runtime.')
    logging.info(payload)
    if payload is not None:
        state.set(payload)
    else:
        logging.info('payload is none')