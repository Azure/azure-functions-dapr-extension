import logging


def main(payload, data: str) -> None:
    logging.info(
        'Python function processed a RetrieveOrder request from the Dapr Runtime.')
    logging.info(data)
