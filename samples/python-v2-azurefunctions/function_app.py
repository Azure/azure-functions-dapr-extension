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


import json
import azure.functions as func
import logging
import json
import azure.functions as func
import logging
from dapr_blueprint_app import app

dapp = func.DaprFunctionApp()
dapp.register_blueprint(app)

# Dapr state output binding with http trigger
@dapp.function_name(name="HttpTriggerFunc")
@dapp.route(route="req", auth_level=dapp.auth_level.ANONYMOUS)
@dapp.dapr_state_output(arg_name="state", state_store="statestore", key="newOrder")
def main(req: func.HttpRequest, state: func.Out[str] ) -> str:
    # request body must be passed this way '{\"value\": { \"key\": \"some value\" } }'
    body = req.get_body()
    logging.info(body.decode('utf-8'))
    if body is not None:
        state.set(body.decode('utf-8'))
        logging.info(body.decode('utf-8'))
    else:
        logging.info('req body is none')
    return 'ok'

# Dapr state output binding with http dapr_service_invocation_trigger
@dapp.function_name(name="CreateNewOrder")
@dapp.dapr_service_invocation_trigger(arg_name="payload", method_name="CreateNewOrder")
@dapp.dapr_state_output(arg_name="state", state_store="statestore", key="order")
def main(payload: str, state: func.Out[str] ) :
    # request body must be passed this way '{\"value\": { \"key\": \"some value\" } }'
    logging.info('Python function processed a CreateNewOrder request from the Dapr Runtime.')
    logging.info(payload)
    if payload is not None:
        state.set(payload)
    else:
        logging.info('payload is none')

# Dapr state input binding with http dapr_service_invocation_trigger
@dapp.function_name(name="RetrieveOrder")
@dapp.dapr_service_invocation_trigger(arg_name="payload", method_name="RetrieveOrder")
@dapp.dapr_state_input(arg_name="data", state_store="statestore", key="order")
def main(payload, data: str) :
    # Function should be invoked with this command: dapr invoke --app-id functionapp --method RetrieveOrder  --data '{}'
    logging.info('Python function processed a RetrieveOrder request from the Dapr Runtime.')
    logging.info(data)

# Dapr secret input binding with http dapr_service_invocation_trigger
@dapp.function_name(name="RetrieveSecret")
@dapp.dapr_service_invocation_trigger(arg_name="payload", method_name="RetrieveSecret")
@dapp.dapr_secret_input(arg_name="secret", secret_store_name="localsecretstore", key="my-secret", metadata="metadata.namespace=default")
def main(payload, secret: str) :
    # Function should be invoked with this command: dapr invoke --app-id functionapp --method RetrieveSecret  --data '{}'
    logging.info('Python function processed a RetrieveSecret request from the Dapr Runtime.')
    secret_dict = json.loads(secret)

    for key in secret_dict:
        logging.info("Stored secret: Key = " + key +
                     ', Value = ' + secret_dict[key])
        
# Dapr binding output
# Dapr state output binding with http dapr_service_invocation_trigger
@dapp.function_name(name="SendMessageToKafka")
@dapp.dapr_service_invocation_trigger(arg_name="payload", method_name="SendMessageToKafka")
@dapp.dapr_binding_output(arg_name="messages", binding_name="%KafkaBindingName%", operation="create")
def main(payload: str, messages: func.Out[bytes]) -> None:
    logging.info('Python processed a SendMessageToKafka request from the Dapr Runtime.')
    messages.set(json.dumps({"data": payload}).encode('utf-8'))

# Dapr binding trigger
@dapp.function_name(name="ConsumeMessageFromKafka")
@dapp.dapr_binding_trigger(arg_name="triggerData", binding_name="%KafkaBindingName%")
def main(triggerData: str) -> None:
    logging.info('Python function processed a ConsumeMessageFromKafka request from the Dapr Runtime.')
    logging.info('Trigger data: ' + triggerData)

# Dapr topic trigger
@dapp.function_name(name="PrintTopicMessage")
@dapp.dapr_topic_trigger(arg_name="subEvent", pub_sub_name="%PubSubName%", topic="B", route="B")
def main(subEvent) -> None:
    logging.info('Python function processed a PrintTopicMessage request from the Dapr Runtime.')
    subEvent_json = json.loads(subEvent)
    logging.info("Topic B received a message: " + subEvent_json["data"])

# Dapr publish output
# Dapr topic trigger with dapr_publish_output
@dapp.function_name(name="TransferEventBetweenTopics")
@dapp.dapr_topic_trigger(arg_name="subEvent", pub_sub_name="%PubSubName%", topic="A", route="A")
@dapp.dapr_publish_output(arg_name="pubEvent", pub_sub_name="%PubSubName%", topic="B")
def main(subEvent, pubEvent: func.Out[bytes]) -> None:
    logging.info('Python function processed a TransferEventBetweenTopics request from the Dapr Runtime.')
    subEvent_json = json.loads(subEvent)
    payload = "Transfer from Topic A: " + str(subEvent_json["data"])
    pubEvent.set(json.dumps({"payload": payload}).encode('utf-8'))

# Dapr invoke output binding with http trigger
@dapp.function_name(name="InvokeOutputBinding")
@dapp.route(route="invoke/{appId}/{methodName}", auth_level=dapp.auth_level.ANONYMOUS)
@dapp.dapr_invoke_output(arg_name = "payload", app_id = "{appId}", method_name = "{methodName}", http_verb = "post")
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