import logging


def main(triggerData: str) -> None:
    logging.info(
        'Python function processed a ConsumeMessageFromKafka request from the Dapr Runtime.')
    logging.info('Trigger data: ' + triggerData)
