import json
import logging 
import azure.functions as func 

sendMessageToKafka = func.Blueprint()

# Dapr binding output
# Dapr state output binding with http dapr_service_invocation_trigger
@sendMessageToKafka.function_name(name="SendMessageToKafka")
@sendMessageToKafka.dapr_service_invocation_trigger(arg_name="payload", method_name="SendMessageToKafka")
@sendMessageToKafka.dapr_binding_output(arg_name="messages", binding_name="%KafkaBindingName%", operation="create")
def main(payload: str, messages: func.Out[bytes]) -> None:
    logging.info('Python processed a SendMessageToKafka request from the Dapr Runtime.')
    messages.set(json.dumps({"data": payload}).encode('utf-8'))