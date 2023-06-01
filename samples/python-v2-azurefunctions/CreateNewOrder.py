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

app = func.FunctionApp()

@app.dapr_service_invocation_trigger(arg_name="dapr", method_name="CreateNewOrder") 
def dapr_service_trigger(json_payload: str):
    logging.info('Python EventHub trigger processed an event: %s', json_payload)