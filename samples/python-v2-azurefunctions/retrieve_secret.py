import json
import logging 
import azure.functions as func 

retrieveSecret = func.DaprBlueprint()

# Dapr secret input binding with http dapr_service_invocation_trigger
@retrieveSecret.function_name(name="RetrieveSecret")
@retrieveSecret.dapr_service_invocation_trigger(arg_name="payload", method_name="RetrieveSecret")
@retrieveSecret.dapr_secret_input(arg_name="secret", secret_store_name="localsecretstore", key="my-secret", metadata="metadata.namespace=default")
def main(payload, secret: str) :
    # Function should be invoked with this command: dapr invoke --app-id functionapp --method RetrieveSecret  --data '{}'
    logging.info('Python function processed a RetrieveSecret request from the Dapr Runtime.')
    secret_dict = json.loads(secret)

    for key in secret_dict:
        logging.info("Stored secret: Key = " + key +
                     ', Value = ' + secret_dict[key])