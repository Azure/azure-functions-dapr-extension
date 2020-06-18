import logging
import json
import azure.functions as func

def main(payload,
         order: func.Out[str]) -> None:
    logging.info('Python function processed a CreateNewOrder request from the Dapr Runtime.')  
    payload_json = json.loads(payload)
    logging.info(payload_json["data"])
    order.set(json.dumps({"value": payload_json["data"]}))
