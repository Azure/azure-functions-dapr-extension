import json
import logging 
import azure.functions as func 

printTopicMessage = func.DaprBlueprint()

# Dapr topic trigger
@printTopicMessage.function_name(name="PrintTopicMessage")
@printTopicMessage.dapr_topic_trigger(arg_name="subEvent", pub_sub_name="%PubSubName%", topic="B", route="B")
def main(subEvent) -> None:
    logging.info('Python function processed a PrintTopicMessage request from the Dapr Runtime.')
    subEvent_json = json.loads(subEvent)
    logging.info("Topic B received a message: " + subEvent_json["data"])