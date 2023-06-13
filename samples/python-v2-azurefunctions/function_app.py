# import logging
# import json
# import azure.functions as func


# def main(payload,
#          order: func.Out[str]) -> None:
#     logging.info(
#         'Python function processed a CreateNewOrder request from the Dapr Runtime.')
#     payload_json = json.loads(payload)
#     logging.info(payload_json["data"])
#     order.set(json.dumps({"value": payload_json["data"]}))


import azure.functions as func
import logging

dapp = func.DaprFunctionApp()

@dapp.function_name(name="HttpTrigger1")
@dapp.route(route="req", auth_level=dapp.auth_level.ANONYMOUS)
@dapp.dapr_state_output(arg_name="state", state_store="statestore", key="order")
def main(req: func.HttpRequest, state: func.Out[str] ) -> str:
    body = req.get_body()
    if body is not None:
        state.set(body.decode('utf-8'))
    else:
        logging.info('req body is none')
    return 'ok'

