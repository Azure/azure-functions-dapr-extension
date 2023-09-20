import json
import logging 
import azure.functions as func 

transferEventBetweenTopics = func.DaprBlueprint()

# Dapr publish output
# Dapr topic trigger with dapr_publish_output
@transferEventBetweenTopics.function_name(name="TransferEventBetweenTopics")
@transferEventBetweenTopics.dapr_topic_trigger(arg_name="subEvent", pub_sub_name="%PubSubName%", topic="A", route="A")
@transferEventBetweenTopics.dapr_publish_output(arg_name="pubEvent", pub_sub_name="%PubSubName%", topic="B")
def main(subEvent, pubEvent: func.Out[bytes]) -> None:
    logging.info('Python function processed a TransferEventBetweenTopics request from the Dapr Runtime.')
    subEvent_json = json.loads(subEvent)
    payload = "Transfer from Topic A: " + str(subEvent_json["data"])
    pubEvent.set(json.dumps({"payload": payload}).encode('utf-8'))