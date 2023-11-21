# Java Azure Function Sample

This tutorial will demonstrate how to use Azure Functions programming model to integrate with multiple Dapr components. Please first go through the [Dapr quickstarts](https://github.com/dapr/quickstarts) to get some contexts on various Dapr building blocks as well as go through Azure Functions [hello-world sample](https://learn.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-java) to familiarize with function programming model.
We'll be running a Dapr-ized function app locally:
1) Invoked by [Dapr Service Invocation](https://docs.dapr.io/developing-applications/building-blocks/service-invocation/service-invocation-overview/) and persist/retrieve state using [Dapr State Management](https://github.com/dapr/components-contrib/tree/master/state)
2) Publish/consume message on a specific topic powered by [Dapr pub/sub](https://github.com/dapr/components-contrib/tree/master/pubsub) and `DaprPublish`/`DaprTopicTrigger`
3) Interact with [Dapr Bindings](https://github.com/dapr/components-contrib/tree/master/bindings) using `DaprBinding`

## Prerequisites
This sample requires you to have the following installed on your machine:
- Setup Dapr : Follow instructions to [download and install the Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/) and [initialize Dapr](https://docs.dapr.io/getting-started/install-dapr-selfhost/).
- [Install Azure Functions Core Tool](https://github.com/Azure/azure-functions-core-tools/blob/master/README.md#windows)
- [Run Kafka Docker Container locally](../../samples/dapr-kafka/README.md)
- Follow the [Configure your local environment](https://learn.microsoft.com/azure/azure-functions/create-first-function-vs-code-java#configure-your-environment) instructions for Java.
- Install maven
- Install the [.NET SDK](https://aka.ms/dotnet-download)
- Steps to run the Java Azure functions with Dapr extension
  1. Go to `/java-library` folder and run below commands.
      ```
      mvn clean package
      ```
      Install the created package, change the azure-functions-java-library-dapr version as per pom.xml
      ```
      mvn install:install-file -Dfile=target/azure-functions-java-library-dapr-1.0-SNAPSHOT.jar -DgroupId=com.microsoft.azure.functions -DartifactId=azure-functions-java-library-dapr -Dversion=1.0-SNAPSHOT -Dpackaging=jar 
      ```
  2. Go to `Samples/java-azurefunction` directory and perform below steps.
   
      Install the Microsoft.Azure.WebJobs.Extensions.Dapr package (required until the extension is not added to the extension bundle).
   
      ```
      func extensions install --package Microsoft.Azure.WebJobs.Extensions.Dapr --version <version>
      ```
      `<version>` should be the latest version of the extension from [NuGet](https://www.nuget.org/packages/Microsoft.Azure.WebJobs.Extensions.Dapr)
# Step 1 - Understand the Settings 

Now that we've locally set up Dapr, clone the repo, then navigate to the dotnet-azurefunction sample: 

```bash
git clone https://github.com/dapr/azure-functions-extension.git
cd samples/java-azurefunction
```
In this folder, you will find `local.settings.json`, which lists a few app settings by the trigger/binding attributes.

```json
"StateStoreName": "statestore"
```

The `%` denotes an app setting value, for the following binding as an example:

`[DaprStateOuput("%StateStoreName%", Key = "order")]`

 In the runtime, the binding will check the `local.settings.json` file and resolve `%StateStoreName%` into `statestore`. The function will then make a call into the state store named as `statestore`.


Please make sure the value in `local.settings.json` matches with the name specified in the YAML files in Dapr `/components` folder:

```yaml
...

kind: Component
metadata:
  name: statestore
spec:

....
```

# Step 2 - Run Function App with Dapr


Run function host with Dapr: 

Windows
```
dapr run --app-id functionapp --app-port 3001 --dapr-http-port 3501 --resources-path ..\components\ -- mvn azure-functions:run
```

Linux/Mac OS
```
dapr run --app-id functionapp --app-port 3001 --dapr-http-port 3501 --resources-path ../components/ -- mvn azure-functions:run
```

The command should output the dapr logs that look like the following:

```
Starting Dapr with id functionapp. HTTP Port: 3501. gRPC Port: 55377
Updating metadata for app command: func host start
You're up and running! Both Dapr and your app logs will appear here.
...
```

> **Note**: there are three ports in this service. The `--app-port`(3001) is where our function host listens on for any Dapr trigger. The `--dapr-http-port`(3501) is where Dapr APIs runs on as well as the  grpc port. The function port (default 7071) is where function host listens on for any HTTP triggred function using `api/{functionName}` URl path. All of these ports are configurable.
> 


# Step 3 - Understand the Sample

## 1. Service Invocation and State Management: Create New Order and Retrieve Order

```Java
@FunctionName("CreateNewOrder")
public String run(
        @DaprServiceInvocationTrigger(
            name = "payload", 
            methodName = "CreateNewOrder") 
        String payload,
        @DaprStateOutput(
            name = "state",
            stateStore = "%StateStoreName%",
            key = "order")
        OutputBinding<String> product,
        final ExecutionContext context) {
    context.getLogger().info("'Java function processed a CreateNewOrder request from the Dapr Runtime.'");
    product.setValue(payload);

    return payload;
}
```

Here the `DaprServiceInvocationTrigger` is used to receive and handle `CreateNewOrder` request and it first logs that this function is successfully triggered. Then it binds the content to the `order` object. The `DaprState` *output binding* will persist the order into the state store by serializing `order` object into a state arrary format and posting it to `http://localhost:${daprPort}/v1.0/state/${stateStoreName}`.

Now you can invoke this function by using the Dapr cli in a new command line terminal.  

Windows Command Prompt
```sh
dapr invoke --app-id functionapp --method CreateNewOrder --data "{\"value\": { \"orderId\": \"41\" } }"
```

Windows PowerShell
```powershell
dapr invoke --app-id functionapp --method CreateNewOrder --data '{"value": { "orderId": "41" } }'
```

Linux or MacOS
```sh
dapr invoke --app-id functionapp --method CreateNewOrder --data '{"value": { "orderId": "41" } }'
```

You can also do this using the Visual Studio Code [Rest Client Plugin](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)

```http
POST  http://localhost:3501/v1.0/invoke/functionapp/method/CreateNewOrder

{
    "value": {
            "orderId": "41"
        }
}
```

**Note**: in this sample, `DaprServiceInvocationTrigger` attribute does not specify the method name, so it defaults to use the FunctionName. Alternatively, you can use `[DaprServiceInvocationTrigger(name = "payload", methodName = "newOrder")]` to specify the service invocation method name that your function should respond. In this case, then you need to use the following command:

```powershell
dapr invoke --app-id functionapp --method newOrder --data "{\"data\": { \"orderId\": \"41\" } }"
```

In your terminal window, you should see logs indicating that the message was received and state was updated:

```
== APP == [TIMESTAMP] Executing 'CreateNewOrder' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP] Java function processed a CreateNewOrder request from the Dapr Runtime.
== APP == [TIMESTAMP] Executed 'CreateNewOrder' (Succeeded, Id=<ExecutionId>)
```
----------------
In order to confirm the state is now persisted. You can now move to the next function:

```Java
@FunctionName("RetrieveOrder")
public String run(
        @DaprServiceInvocationTrigger(
            name = "payload", 
            methodName = "CreateNewOrder") 
        String payload,
        @DaprStateInput(
            name = "state",
            stateStore = "statestore",
            key = "product")
        String product,
        final ExecutionContext context) {
    context.getLogger().info("Java HTTP trigger processed a request.");

    // Parse query parameter
    final String query = request.getQueryParameters().get("name");
    final String name = request.getBody().orElse(query);

    context.getLogger().info(product);

    return product;
}
```

Similarly, the function will be triggered by any `RetrieveOrder` service invocation request. Here `DaprState` *input binding* is used to fetch the latest value of the key `order` and bind the value to JsonElement object `data`' before exectuing the function block.

Invoke input binding function with Dapr cli

```
dapr invoke --app-id functionapp --method RetrieveOrder
```

In your terminal window, you should see logs to confirm the expected result:

```
== APP == [TIMESTAMP] Executing 'Functions.RetrieveOrder' (Reason='(null)', Id=<ExecutionId>)
== APP == [TIMESTAMP] {"orderId":"42"}
== APP == [TIMESTAMP] Java function processed a RetrieveOrder request from the Dapr Runtime.
== APP == [TIMESTAMP] Executed 'Functions.RetrieveOrder' (Succeeded, Id=<ExecutionId>, Duration=186ms)
```


## 2. Pub/Sub: TransferEventBetweenTopics and PrintTopicMessage

```java
@FunctionName("TransferEventBetweenTopics")
public String run(
        @DaprTopicTrigger(
            name = "topicMessage",
            pubSubName = "%PubSubName%",
            topic = "A")
            String request,
        @DaprPublishOutput(
            name = "state",
            pubSubName = "%PubSubName%",
            topic = "B")
        OutputBinding<String> payload,
        final ExecutionContext context) throws JsonProcessingException {
    context.getLogger().info("Java function processed a TransferEventBetweenTopics request from the Dapr Runtime.");

    // Get the CloudEvent data from the request body as a JSON string
    ObjectMapper objectMapper = new ObjectMapper();
    JsonNode jsonNode = objectMapper.readTree(request);

    String data = jsonNode.get("data").asText();

    context.getLogger().info("Printing Topic A received a message: " + data);

    String pubsubPayload = String.format("{\"payload\":\" Transfer from Topic A: %s \"}", data);      
    payload.setValue(pubsubPayload);

    return data;
}
```

Here, `DaprTopicTrigger` is used to subscribe to topic `A`, so whenever a message is published on topic `A`, the message will bind to `CloudEvent` `subEvent`. Please see the [`CloudEvent`](https://github.com/cloudevents/spec/blob/master/spec.md) for details. 


> **Note**: Alternatively, any other JSON-serializable datatype binds directly to the data field of the cloud event. For example, int, double, and custom “POCO” types can be used as the trigger type and will be deserialized from the event’s data field. 

Then, `DaprPublish` *output binding* is used to publish a new event to topic `B` using the strongly-typed `DaprPubSubEvent` class, or it can be written using the attribute `[DaprPublish(Topic = "B")] out object pubEvent`:

```java
    pubEvent = "Transfer from Topic A:" + subEvent.Data;
```

The function below subscribes to topic `B`, and it simply prints the message content when an event arrives.

```java
@FunctionName("PrintTopicMessage")
public String run(
        @DaprTopicTrigger(
            name = "payload",
            pubSubName = "%PubSubName%",
            topic = "B")
        String payload,
        final ExecutionContext context) throws JsonProcessingException {
    Logger logger = context.getLogger();
    logger.info("Java function processed a PrintTopicMessage request from the Dapr Runtime.");

    // Get the CloudEvent data from the request body as a JSON string
    ObjectMapper objectMapper = new ObjectMapper();
    JsonNode jsonNode = objectMapper.readTree(payload);

    String data = jsonNode.get("data").asText();

    logger.info("Topic B received a message: " + data);

    return data;
}
```

You can publish a message to topic A using the Dapr cli:

```powershell
dapr publish --pubsub messagebus --publish-app-id functionapp --topic A --data 'This is a test'
```

The Dapr logs should show the following:
```
== APP == [TIMESTAMP] Executing 'TransferEventBetweenTopics' (Reason='',Id={ExectuionId})
== APP == [TIMESTAMP] C# function processed a TransferEventBetweenTopics request from the Dapr Runtime.
== APP == [TIMESTAMP] Executed 'TransferEventBetweenTopics' (Succeeded, Id={ExectuionId})
== APP == [TIMESTAMP] Executing 'PrintTopicMessage' (Reason='', Id={AnotherExectuionId})
== APP == [TIMESTAMP] C# function processed a PrintTopicMessage request from the Dapr Runtime.
== APP == [TIMESTAMP] Topic B received a message: Transfer from Topic A: This is a test.
== APP == [TIMESTAMP] Executed 'PrintTopicMessage' (Succeeded, Id={AnotherExectuionId})
```

## 3. Dapr Binding: 
This section demonstrates the integration of this extension with Dapr Binding component. A Kafka binding as an example. Please refer [this doc to spin up Kafka locally](../../samples/dapr-kafka/README.md). In the example below, `DaprBindingTrigger` is used to have the azure function triggerred when a new message arrives at Kafka.

```java
@FunctionName("ConsumeMessageFromKafka")
public String run(
        @DaprBindingTrigger(
            name = "triggerData", 
            bindingName = "%KafkaBindingName%") 
        String triggerData,
        final ExecutionContext context) {
    context.getLogger().info("'Java function processed a ConsumeMessageFromKafka request from the Dapr Runtime.'");
    context.getLogger().info("Hello from Kafka!");

    context.getLogger().info(String.format("Trigger data: %s", triggerData));

    return triggerData;
}
```
Now let's look at how our function uses `DaprBinding` to push messages into our Kafka instance.

```java
@FunctionName("SendMessageToKafka")
public String run(
        @DaprServiceInvocationTrigger(
            name = "payload", 
            methodName = "SendMessageToKafka") 
        String payload,
        @DaprBindingOutput(
            name = "state",
            bindingName = "%KafkaBindingName%", 
            operation = "create")
        OutputBinding<String> product,
        final ExecutionContext context) {
    context.getLogger().info("Java  function processed a SendMessageToKafka request.");
    product.setValue(payload);

    return payload;
}
```
`DaprBinding` *output binding* sends the payload to the `sample-topic` Kafka Dapr binding. 

You can use service invocation to invoke this function:

Windows
```powershell
dapr invoke --app-id functionapp --method SendMessageToKafka --data '{\"data\":{\"message\": \"hello!\" }}'
```

Linux/MacOS
```shell
dapr invoke --app-id functionapp --method SendMessageToKafka --data '{"data":{"message": "hello!" }}'
```

The Dapr function logs should show the following:
```
== APP == [TIMESTAMP] Executing 'SendMessageToDaprBinding' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP] C# HTTP trigger function processed a request.
== APP == [TIMESTAMP] Executed 'SendMessageToDaprBinding' (Succeeded, Id=<ExecutionId>)
```

Since both functions have been deployed in the same app, you can see the logs below which indicate that the message has been consumed.
```
== APP == [TIMESTAMP] Executing 'ConsumeMessageFromKafka' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP] Hello from Kafka!
== APP == [TIMESTAMP] Trigger data: { message: 'hello!' }
== APP == [TIMESTAMP] Executed 'ConsumeMessageFromKafka' (Succeeded, Id=<ExecutionId>)
```

## 4. Dapr Secret:
This section demonstrates how `DaprSecret` **input binding** integrates with Dapr Secret component. Here, Local file Secret Store is used and you can follow the setup instructions at [Local file secret store](https://docs.dapr.io/operations/components/setup-secret-store/supported-secret-stores/file-secret-store/) to configure a secret named "my-secret".

Please refer to [Dapr Secret Store doc](https://docs.dapr.io/operations/components/setup-secret-store/supported-secret-stores/file-secret-store/) to set up other supported secret stores.

```java
    @FunctionName("RetrieveSecretLocal")
    public void run(
        @DaprServiceInvocationTrigger(
            name = "args", 
            methodName = "RetrieveSecretLocal") Object args,
        @DaprSecretInput(
            name = "secret", 
            secretStoreName = "localsecretstore",
            key = "my-secret", 
            metadata = "metadata.namespace=default") 
            Map<String, String> secret,
        final ExecutionContext context) {

        context.getLogger().info("Java function processed a RetrieveSecret request from the Dapr Runtime.");

        // Print the fetched secret value
        // This is only for demo purpose
        // Please do not log any real secret in your production code
        for (Map.Entry<String, String> entry : secret.entrySet()) {
            context.getLogger().info(String.format("Stored secret: Key = %s, Value = %s", entry.getKey(), entry.getValue()));
        }
    }
```

`DaprSecret` *input binding* retreives the secret named by `my-secret` and binds to `secret` as a dictionary object. Since Local Secret Store supports multiple keys in a secret, the secret dictionary could include multiple key value pairs and you can access the specfic one. For other secret store only supports one keys, the dictionary will only contain one key value pair where key matches the secret name, namely `my-secret` in this example, and the actual secret value is in the property value. This sample just simply prints out all secrets, but please do not log any real secret in your production code.

You can retrieve the secret by invoking the RetrieveSecretLocal function using the command:-
```
dapr invoke --app-id functionapp --method RetrieveSecretLocal my-secret
```

Given differnt secret store, the metadata string needs to be provided. In order to specify multiple metadata fields, join them by `&`, see the below [Hashicorp Vault](https://docs.dapr.io/operations/components/setup-secret-store/supported-secret-stores/hashicorp-vault/) example.
```java
[DaprSecretInput(secretStoreName = "vault", key = "my-secret",  Metadata = "metadata.version_id=15&metadata.version_stage=AAA"`.
```

## 5. Dapr Invoke output binding
Dapr invoke output binding is could be used to invoke other azure functions or service where it will act as a proxy. For example, In the below Azure function, which gets triggered on HttpTrigger, can invoke antother azure functions like RetrieveOrder.

```Java
@FunctionName("InvokeOutputBinding")
public String run(
        @HttpTrigger(
            name = "req",
            methods = {HttpMethod.GET},
            authLevel = AuthorizationLevel.ANONYMOUS,
            route = "invoke/{appId}/{methodName}")
            HttpRequestMessage<Optional<String>> request,
        @DaprInvokeOutput(
            name = "payload",
            appId = "{appId}", 
            methodName = "{methodName}", 
            httpVerb = "post")
        OutputBinding<String> payload,
        final ExecutionContext context) {
    context.getLogger().info("Java HTTP trigger processed a request.");

    // Parse query parameter
    final String query = request.getQueryParameters().get("name");
    final String name = request.getBody().orElse(query);

    String jsoString = String.format("{\"body\":\"%s\"}", name);

    payload.setValue(jsoString);

    return jsoString;
}
```

Invoke the above function (InvokeOutputBinding) with http get call

  ```
  http://localhost:7071/api/invoke/functionapp/RetrieveOrder
  ```

Once InvokeOutputBinding is called, it will invoke the RetrieveOrder azure fucntion and the output will look like as shown below.

```
== APP == [TIMESTAMP] Executing 'InvokeOutputBinding' (Reason='This function was programmatically called via the host APIs.', Id=<ExecutionId>)
== APP == [TIMESTAMP] Java HTTP trigger processed a request.
== APP == [TIMESTAMP] Executing 'RetrieveOrder' (Reason='(null)', Id=<ExecutionId>)
== APP == [TIMESTAMP] Java function processed a RetrieveOrder request from the Dapr Runtime.
== APP == [TIMESTAMP] {"orderId":"41"}
== APP == [TIMESTAMP] Executed 'RetrieveOrder' (Succeeded, Id=<ExecutionId>)
== APP == [TIMESTAMP] Executed 'InvokeOutputBinding' (Succeeded, Id=<ExecutionId>)
```

# Step 6 - Cleanup

To stop your services from running, simply stop the "dapr run" process. Alternatively, you can spin down each of your services with the Dapr CLI "stop" command. For example, to spin down both services, run these commands in a new command line terminal: 

```bash
dapr stop --app-id functionapp
```


# Deploy Dapr Function App into Kubernetes
This section describes the steps to get the Dapr function app running in a Kubernetes cluster.
(To generate your custom container image please see these [instructions](./BuildContainerImage.md))

## Prerequisites
Below are the requirements for this sample which covers multiple Dapr components. Please skip any step that is not required for your own function app.  
- Install [kubectl](https://kubernetes.io/docs/tasks/tools/install-kubectl/)
- Install [helm](https://helm.sh/docs/intro/install/) (you can skip this if your function app does not use Kafka bindings)
- A Kubernetes cluster, such as [Minikube](https://docs.dapr.io/operations/hosting/kubernetes/cluster/setup-minikube/), [AKS](https://docs.dapr.io/operations/hosting/kubernetes/cluster/setup-aks/) or [GKE](https://cloud.google.com/kubernetes-engine/)
- A State Store, such as [Redis Store](https://docs.dapr.io/getting-started/configure-redis/) for Dapr state store and pub/sub message delivery (you can skip this if your function does not use the aforementioned components)

## Setup Dapr on your Kubernetes Cluster
Once you have a cluster, run `dapr init --kubernetes` to deploy Dapr to it. Please follow this( guide on [how to install Dapr on your Kubernetes](https://docs.dapr.io/getting-started/install-dapr/#install-dapr-on-a-kubernetes-cluster) via Dapr CLI or Helm. Dapr CLI does not support non-default namespaces and only is recommended for testing purposes.
If you need a non-default namespace or in production environment, Helm has to be used.

```
⌛  Making the jump to hyperspace...
✅  Deploying the Dapr Operator to your cluster...
✅  Success! Dapr has been installed. To verify, run 'kubectl get pods -w' in your terminal
``` 
## Deploy Dapr components
#### [Optional] Configure the State Store
  - Replace the hostname and password in `deploy/redis.yaml`. https://github.com/dapr/quickstarts/tree/master/hello-kubernetes#step-2---create-and-configure-a-state-store
  - Run `kubectl apply -f ./deploy/redis.yaml` and observe that your state store was successfully configured!
    ```
    component.dapr.io/statestore configured
    ```
   - Follow [secret management](https://docs.dapr.io/developing-applications/building-blocks/secrets/) instructions to securely manage your secrets in a production-grade application.
   - More detail can be found in Dapr sample repo [2.hello-kubernetes](https://github.com/dapr/quickstarts/tree/master/hello-kubernetes#step-2---create-and-configure-a-state-store)


#### [Optional] Setting up a Kafka in Kubernetes
  - Install Kafka via bitnami/kafka
```
    helm repo add bitnami https://charts.bitnami.com/bitnami
    helm repo update
    kubectl create ns kafka
    helm install dapr-kafka bitnami/kafka --wait --namespace kafka -f ./kafka-non-persistence.yaml
```

 - Run `kubectl -n kafka get pods -w` to see Kafka pods are running. This might take a few minute, but you should see.
   ```
    NAME                     READY   STATUS    RESTARTS   AGE
    dapr-kafka-0             1/1     Running   0          2m7s
    dapr-kafka-zookeeper-0   1/1     Running   0          2m57s
    dapr-kafka-zookeeper-1   1/1     Running   0          2m13s
    dapr-kafka-zookeeper-2   1/1     Running   0          109s
   ```
- Run `kubectl apply -f .\deploy\kafka.yaml` and observe that your kafka was successfully configured!
   ```
   component.dapr.io/sample-topic created
   ```
- Follow [secret management](https://docs.dapr.io/developing-applications/building-blocks/secrets/) instructions to securely manage your secrets in a production-grade application.

#### [Optional] Setting up the Pub/Sub in Kubernetes
  - This sample uses Redis Stream (Redis Version 5 and above) to enable pub/sub. Replace the hostname and password in `deploy/redis-pubsub.yaml`. https://github.com/dapr/quickstarts/tree/master/hello-kubernetes#step-2---create-and-configure-a-state-store
  - Run `kubectl apply -f .\deploy\redis.yaml` and observe that your state store was successfully configured!
    ```
    component.dapr.io/messagebus configured
    ```
   - See Dapr sample repo [pub-sub](https://github.com/dapr/quickstarts/tree/master/pub-sub) for more instructions.



#### [Optional] Setting up Secrets in Kubernetes

Create the secret in the kubernetes environment for our Dapr Secret binding sample:
```shell
kubectl create secret generic my-secret --from-literal=key1=supersecret --from-literal=key2=topsecret
```

Confirm the secret is persisted and the value are base64 encoded.

```powershell
$ kubectl get secret my-secret -o yaml

apiVersion: v1
data:
  key1: c3VwZXJzZWNyZXQ= # decoded value: supersecret
  key2: dG9wc2VjcmV0 # decoded value: topsecret
kind: Secret
...
```

Now you should have all Dapr components up and running in your kubernetes cluster. Next step is to deploy the function app into a kubernetes cluster with the Dapr Side Car.

## Deploy your Dapr Function App
You can find your function app deployment file `deploy/functionapp.yaml`.

In the second part of the deployment file, you need to put your image name and specify your app port where your Dapr Trigger will listen on. 

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: functionapp
  labels:
    app: functionapp
spec:
  replicas: 1
  selector:
    matchLabels:
      app: functionapp
  template:
    metadata:
      labels:
        app: functionapp
      annotations:
        dapr.io/enabled: "true"
        dapr.io/id: "functionapp"
        dapr.io/port: "<app-port>"
    spec:
      containers:
      - name: functionapp
        image: <your-docker-hub-id>/<your-image-name>
        ports:
        - containerPort: <app-port>
        imagePullPolicy: Always
        env:
        - name: StateStoreName
          value: <state-store-name>
        - name: KafkaBindingName
          value: <dapr-binding-name>
```

Now run the following command to deploy the function app into your kubernetes cluster.

``` powershell
$ kubectl apply -f ./deploy/functionapp.yaml

secret/functionapp created
deployment.apps/functionapp created
```

Run `kubectl get pods` to see your function app is up and running.
```
NAME                                     READY   STATUS    RESTARTS   AGE
dapr-operator-64b94c8b85-jtbpn           1/1     Running   0          10m
dapr-placement-844cf4c696-2mv88          1/1     Running   0          10m
dapr-sentry-7c8fff7759-zwph2             1/1     Running   0          10m
dapr-sidecar-injector-675df889d5-22wxr   1/1     Running   0          10m
functionapp-6d4cc6b7f7-2p9n9             2/2     Running   0          8s
```

## Test your Dapr Function App  
Now let's try invoke our function. You can use the follwoing commad to the logs. Use `--tail` to specify the last `n` lines of logs.
```powershell
kubectl logs --selector=app=functionapp -c functionapp --tail=50
```


In order to hit your function app endpoint, you can use port forwarding. Use the pod name for your function app.
```
kubectl port-forward functionapp-6d4cc6b7f7-2p9n9 {port-of-your-choice}:3001
```

You can use the Rest Client Plugin as below. You can use any of your preferred tools to send HTTP request.
``` http
POST  http://localhost:{port-of-your-choice}/CreateNewOrder  

{
    "data": { 
        "orderId": 41 
    }
}
```

``` http
POST  http://localhost:{port-of-your-choice}/RetrieveOrder
```

``` http
POST  http://localhost:{port-of-your-choice}/SendMessageToKafka 

{"message": "hello!" }
```

``` http
POST  http://localhost:{port-of-your-choice}/RetrieveSecret
```
Run kubectl logs command to retrieve the latest log. You should see your function app is getting invoked as you have seen when testing locally.

``` powershell
: Function.RetrieveOrder[0]
      Executing 'RetrieveOrder' (Reason='', Id=0f378098-d15a-4f13-81ea-20caee7ae10c)
: Function.RetrieveOrder.User[0]
      C# function processed a RetrieveOrder request from the Dapr Runtime.
: Function.RetrieveOrder.User[0]
      {"orderId":41}
: Function.RetrieveOrder[0]
      Executed 'RetrieveOrder' (Succeeded, Id=0f378098-d15a-4f13-81ea-20caee7ae10c)

: Function.CreateNewOrder[0]
      Executing 'CreateNewOrder' (Reason='', Id=faa53523-85c3-41cb-808c-02d47cb7dcdc)
: Function.CreateNewOrder.User[0]
      C# function processed a CreateNewOrder request from the Dapr Runtime.
: Function.CreateNewOrder[0]
      Executed 'CreateNewOrder' (Succeeded, Id=faa53523-85c3-41cb-808c-02d47cb7dcdc)

: Function.SendMessageToKafka.User[0]
      C# function processed a SendMessageToKafka request.
: Function.SendMessageToKafka[0]
      Executed 'SendMessageToKafka' (Succeeded, Id=5aa8e383-9c8b-4686-90a7-089d71118d81)

: Function.ConsumeMessageFromKafka[0]
      Executing 'ConsumeMessageFromKafka' (Reason='', Id=aa8d92a6-2da1-44ff-a033-cb217b9c29541)
: Function.ConsumeMessageFromKafka.User[0]
     Hello from Kafka!
: Function.ConsumeMessageFromKafka[0]
      Trigger {data: {"message": "hello!"}
: Function.SendMessageToKafka[0]
      Executed 'ConsumeMessageFromKafka' (Succeeded, Id=aa8d92a6-2da1-44ff-a033-cb217b9c29541)

: Function.RetrieveSecret[0]
      Executing 'RetrieveSecret' (Reason='', Id=961af93f-9ddc-477e-a490-4d07bf6d026a))
: Function.RetrieveSecret.User[0]
      C# function processed a RetrieveSecret request from the Dapr Runtime.
: Function.RetrieveSecret.User[0]
      Stored secret: Key = key1, Value = super-secret
: Function.RetrieveSecret[0]
      Stored secret: Key = key2, Value = top-secret
: Function.RetrieveSecret[0]
      Executed 'RetrieveSecret' (Succeeded, Id=961af93f-9ddc-477e-a490-4d07bf6d026a))

```

## Cleanup
Once you're done using the sample, you can spin down your Kubernetes resources by navigating to the `./deploy` directory and running:
```
kubectl delete -f .
```
This will spin down each resource defined by the .yaml files in the deploy directory.
