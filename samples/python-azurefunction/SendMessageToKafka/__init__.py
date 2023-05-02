import logging
import json
import azure.functions as func


def main(args, messages: func.Out[bytes]) -> None:
    logging.info(
        'Python processed a SendMessageToKafka request from the Dapr Runtime.')
    messages.set(json.dumps({"data": args}).encode('utf-8'))
