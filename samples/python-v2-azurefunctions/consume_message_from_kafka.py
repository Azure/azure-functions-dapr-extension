import logging 
import azure.functions as func 

consumeMessageFromKafka = func.DaprBlueprint()

# Dapr binding trigger
@consumeMessageFromKafka.function_name(name="ConsumeMessageFromKafka")
@consumeMessageFromKafka.dapr_binding_trigger(arg_name="triggerData", binding_name="%KafkaBindingName%")
def main(triggerData: str) -> None:
    logging.info('Python function processed a ConsumeMessageFromKafka request from the Dapr Runtime.')
    logging.info('Trigger data: ' + triggerData)