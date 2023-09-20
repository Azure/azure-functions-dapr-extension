import json
import logging
import azure.functions as func


def main(req: func.HttpRequest,
         payload: func.Out[bytes]) -> func.HttpResponse:
    logging.info('Python InvokeOutputBinding processed a request.')
    data = req.params.get('data')
    if not data:
        try:
            req_body = req.get_json()
        except ValueError:
            pass
        else:
            data = req_body.get('data')

    if data:
        logging.info(f"Url: {req.url}, Data: {data}")
        payload.set(json.dumps({"body": data}).encode('utf-8'))
        return func.HttpResponse(f"Url: {req.url}, Data: {data}")
    else:
        return func.HttpResponse(
            "Please pass a data on the query string or in the request body",
            status_code=400
        )
