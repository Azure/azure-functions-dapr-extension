import logging 
import azure.functions as func 

invokeOutputBinding = func.DaprBlueprint()

# Dapr invoke output binding with http trigger
@invokeOutputBinding.function_name(name="InvokeOutputBinding")
@invokeOutputBinding.route(route="invoke/{appId}/{methodName}", auth_level=func.AuthLevel.ANONYMOUS)
@invokeOutputBinding.dapr_invoke_output(arg_name = "payload", app_id = "{appId}", method_name = "{methodName}", http_verb = "post")
def main(req: func.HttpRequest, payload: func.Out[str] ) -> str:
    # request body must be passed this way "{\"body\":{\"value\":{\"key\":\"some value\"}}}" to use the InvokeOutputBinding, all the data must be enclosed in body property.
    logging.info('Python function processed a InvokeOutputBinding request from the Dapr Runtime.')
    
    body = req.get_body()
    logging.info(body)
    if body is not None:
        payload.set(body)
    else:
        logging.info('req body is none')
    return 'ok'