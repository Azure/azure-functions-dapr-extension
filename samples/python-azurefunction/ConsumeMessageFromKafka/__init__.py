import logging
import json
import azure.functions as func

def main(triggerData: str) -> None:
    logging.info('Hello from Kafka!')
    logging.info('Trigger data: ' + triggerData)
