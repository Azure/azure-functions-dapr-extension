import logging 
import azure.functions as func 

retrieveOrder = func.Blueprint()

# Dapr state input binding with http dapr_service_invocation_trigger
@retrieveOrder.function_name(name="RetrieveOrder")
@retrieveOrder.dapr_service_invocation_trigger(arg_name="payload", method_name="RetrieveOrder")
@retrieveOrder.dapr_state_input(arg_name="data", state_store="%StateStoreName%", key="order")
def main(payload, data: str) :
    # Function should be invoked with this command: dapr invoke --app-id functionapp --method RetrieveOrder  --data '{}'
    logging.info('Python function processed a RetrieveOrder request from the Dapr Runtime.')
    logging.info(data)