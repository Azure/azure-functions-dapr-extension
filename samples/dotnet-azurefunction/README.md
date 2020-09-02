# .NET Azure Function Sample

This tutorial will demonstrate how to use Azure Functions programming model to integrate with multiple Dapr components. Please first go through the [samples](https://github.com/dapr/samples) to get some contexts on various Dapr building blocks as well as go through Azure Functions [hello-world sample](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-first-function-vs-code?pivots=programming-language-csharp) to familiarize with function programming model.
We'll be running a Darp'd function app locally:
1) Invoked by [Dapr Service Invocation](https://github.com/dapr/docs/tree/master/concepts/service-invocation) and persist/retrieve state using [Dapr State Management](https://github.com/dapr/components-contrib/tree/master/state)
2) Publish/consume message on a specific topic powered by [Dapr pub/sub](https://github.com/dapr/components-contrib/tree/master/pubsub) and `DaprPublish`/`DaprTopicTrigger`
3) Interact with [Dapr Bindings](https://github.com/dapr/components-contrib/tree/master/bindings) using `DaprBinding`

## Prerequisites
This sample requires you to have the following installed on your machine:
- [Setup Dapr](https://github.com/dapr/samples/tree/master/1.hello-world) : Follow [instructions](https://github.com/dapr/docs/blob/master/getting-started/environment-setup.md#environment-setup) to download and install the Dapr CLI and initialize Dapr.
- [Install Azure Functions Core Tool](https://github.com/Azure/azure-functions-core-tools/blob/master/README.md#windows)
- [Run Kafka Docker Container Locally](https://github.com/dapr/samples/tree/master/5.bindings). The required Kafka files is located in `sample\dapr-kafka` directory.

# Step 1 - Understand the Settings 

Now that we've locally set up Dapr, clone the repo, then navigate to the dotnet-azurefunction sample: 

```bash
git clone https://github.com/dapr/azure-functions-extension.git
cd samples/dotnet-azurefunction
```
In this folder, you will find `local.settings.json`, which lists a few app settings we used in our trigger/binding attributes. 

```json
"StateStoreName": "statestore"
```

The `%` denotes an app setting value, for the following binding as an example:

`[DaprState("%StateStoreName%", Key = "order")]`

 In the runtime, the binding will check the `local.settings.json` file and resolve `%StateStoreName%` into `statestore`. The function will then make a call into the state store named as `statestore`.


Please make sure the value in `local.settings.json` matches with the name specified in the YAML files in Dapr `/component` folder:

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

```
dapr run --app-id functionapp --app-port 3001 --dapr-http-port 3501 --components-path ..\components\ -- func host start
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

```csharp
[FunctionName("CreateNewOrder")]
public static void Run(
    [DaprServiceInvocationTrigger] JObject payload, 
    [DaprState("%StateStoreName%", Key = "order")] out object order,
    ILogger log)
{
    log.LogInformation("C# function processed a CreateNewOrder request from the Dapr Runtime.");

    order = payload["data"];
}
```

Here we use `DaprServiceInvocationTrigger` to receive and handle `CreateNewOrder` request. We first log that this function is successfully triggered. Then we binds the content to the `order` object. The `DaprState` *output binding* will persist the order into the state store by serializing `order` object into a state arrary format and posting it to `http://localhost:${daprPort}/v1.0/state/${stateStoreName}`.

Now we can invoke this function by using the Dapr cli in a new command line terminal.  

Windows Command Prompt
```sh
dapr invoke --app-id functionapp --method CreateNewOrder --payload "{\"data\": { \"orderId\": \"41\" } }"
```

Windows PowerShell
```powershell
dapr invoke --app-id functionapp --method CreateNewOrder --payload '{\"data\": { \"orderId\": \"41\" } }'
```

Linux or MacOS
```sh
dapr invoke --app-id functionapp --method CreateNewOrder --payload '{"data": { "orderId": "41" } }'
```

We can also do this using the Visual Studio Code [Rest Client Plugin](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)

```http
POST  http://localhost:3501/v1.0/invoke/functionapp/method/CreateNewOrder

{
    "data": {
        "orderId": "42"
    } 
}
```

**Note**: in this sample, `DaprServiceInvocationTrigger` attribute does not specify the method name, so it defaults to use the FunctionName. Alternatively, we can use `[DaprServiceInvocationTrigger(MethodName = "newOrder")]` to specify the service invocation method name that your function should respond. In this case, then we need to use the following command:

```powershell
dapr invoke --app-id functionapp --method newOrder --payload "{\"data\": { \"orderId\": \"41\" } }"
```

In your terminal window, you should see logs indicating that the message was received and state was updated:

```
== APP == [TIMESTAMP] Executing 'CreateNewOrder' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP] C# function processed a CreateNewOrder request from the Dapr Runtime.
== APP == [TIMESTAMP] Executed 'CreateNewOrder' (Succeeded, Id=<ExecutionId>)
```
----------------
In order to confirm the state is now persisted. We now can move to our next function:

```csharp
[FunctionName("RetrieveOrder")]
public static void Run(
    [DaprServiceInvocationTrigger] object args,
    [DaprState("%StateStoreName%", Key = "order")] string data,
    ILogger log)
{
    log.LogInformation("C# function processed a RetrieveOrder request from the Dapr Runtime.");

    // print the fetched state value
    log.LogInformation(data);
}
```

Similarly, the function will be triggered by any `RetrieveOrder` service invocation request. Here we use `DaprState` *input binding* to fetch the latest value of the key `order` and bind the value to string object `data`' before we start exectuing the function block.

In your terminal window, you should see logs to confirm the expected result:

```
== APP == [TIMESTAMP]  Executing 'RetrieveOrder' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP]  C# function processed a RetrieveOrder request from the Dapr Runtime.
== APP == [TIMESTAMP]  {"orderId":"41"}
== APP == [TIMESTAMP]  Executed 'RetrieveOrder' (Succeeded, Id=<ExecutionId>)
```


## 2. Pub/Sub: TransferEventBetweenTopics and PrintTopicMessage

```csharp
[FunctionName("TransferEventBetweenTopics")]
public static void Run(
    [DaprTopicTrigger("%PubSubName%", Topic = "A")] CloudEvent subEvent,
    [DaprPublish(PubSubName = "%PubSubName%", Topic = "B")] out DaprPubSubEvent pubEvent,
    ILogger log)
{
    log.LogInformation("C# function processed a TransferEventBetweenTopics request from the Dapr Runtime.");


    pubEvent = new DaprPubSubEvent("Transfer from Topic A: " + subEvent.Data);
}
```

Here we use `DaprTopicTrigger` to subscribe to topic `A`, so whenever a message is published on topic `A`, the message will bind to `CloudEvent` `subEvent`. Please see the [`CloudEvent`](https://github.com/cloudevents/spec/blob/master/spec.md) for details. 


> **Note**: Alternatively, any other JSON-serializable datatype binds directly to the data field of the cloud event. For example, int, double, and custom “POCO” types can be used as the trigger type and will be deserialized from the event’s data field. 

Then we use `DaprPublish` *output binding* to publish a new event to topic `B` using the strongly-typed `DaprPubSubEvent` class, or it can be written using the attribute `[DaprPublish(Topic = "B")] out object pubEvent`:

```csharp
    pubEvent = "Transfer from Topic A:" + subEvent.Data;
```

At the same time, we also have a function that subscribes to topic `B`, and it will simply just print the message content when an event arrives. 

```csharp
[FunctionName("PrintTopicMessage")]
public static void Run(
    [DaprTopicTrigger("%PubSubName%", Topic = "B")] CloudEvent subEvent,
    ILogger log)
{
    log.LogInformation("C# function processed a PrintTopicMessage request from the Dapr Runtime.");
    log.LogInformation($"Topic B received a message: {subEvent.Data}.");
}
```

Then let's see what will happen if we publish a message to topic A using the Dapr cli:

```powershell
dapr publish --pubsub messagebus --topic A --data 'This is a test'
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
Next we will show how this extension integrates with Dapr Binding component. Here we uses Kafka binding as an example. Please refer to [Dapr Bindings Sample](https://github.com/dapr/samples/tree/master/5.bindings) to spin up your the Kafka locally. In the example below, we use `DaprBindingTrigger` to have our function triggerred when a new message arrives at Kafka.

```csharp
[FunctionName("ConsumeMessageFromKafka")]
public static void Run(
    // Note: the value of BindingName must match the binding name in components/kafka-bindings.yaml
    [DaprBindingTrigger(BindingName = "%KafkaBindingName%")] JObject triggerData,
    ILogger log)
{
    log.LogInformation("Hello from Kafka!");

    log.LogInformation($"Trigger data: {triggerData}");
}
```
Now let's look at how our function uses `DaprBinding` to push messages into our Kafka instance.

```csharp
[FunctionName("SendMessageToKafka")]
public static async void Run(
    [DaprServiceInvocationTrigger] JObject payload,
    [DaprBinding(BindingName = "%KafkaBindingName%")] IAsyncCollector<object> messages,
    ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");

    await messages.AddAsync(payload);
}
```
`DaprBinding` *output binding* sends the payload to the `sample-topic` Kafka Dapr binding. `IAsyncCollector<object>` allows you to send multiple message by calling `AddAsync` with different payloads. 

Now we can use service invocation to invoke this function:

```powershell
dapr invoke --app-id functionapp --method SendMessageToKafka --payload '{\"message\": \"hello!\" }'
```

The Dapr function logs should show the following:
```
== APP == [TIMESTAMP] Executing 'SendMessageToDaprBinding' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP] C# HTTP trigger function processed a request.
== APP == [TIMESTAMP] Executed 'SendMessageToDaprBinding' (Succeeded, Id=<ExecutionId>)
```

Since we have both functions deployed in the same app, you should also see we have consumed the message by see the folowing:
```
== APP == [TIMESTAMP] Executing 'ConsumeMessageFromKafka' (Reason='', Id=<ExecutionId>)
== APP == [TIMESTAMP] Hello from Kafka!
== APP == [TIMESTAMP] Trigger data: { message: 'hello!' }
== APP == [TIMESTAMP] Executed 'ConsumeMessageFromKafka' (Succeeded, Id=<ExecutionId>)
```

## 4. Dapr Secret: 
Next we will show how `DaprSecret` **input binding** integrates with Dapr Secret component. Here we use Kubernetes Secret Store which does not require special configuration. This requires a Kubernetes cluster. Please refer to [Dapr Secret Store doc](https://github.com/dapr/docs/tree/master/howto/setup-secret-store) to set up other supported secret stores.

```csharp
[FunctionName("RetrieveSecret")]
public static void Run(
    [DaprServiceInvocationTrigger] object args,
    [DaprSecret("kubernetes", "my-secret", Metadata = "metadata.namespace=default")] IDictionary<string, string> secret,
    ILogger log)
{
    log.LogInformation("C# function processed a RetrieveSecret request from the Dapr Runtime.");
      
    foreach (var kvp in secret)
    {
        log.LogInformation("Stored secret: Key = {0}, Value = {1}", kvp.Key, kvp.Value);
    }

}
```

`DaprSecret` *input binding* retreives the secret named by `my-secret` and binds to `secret` as a dictionary object. Since Kubernetes Secret supports multiple keys in a secret, the secret dictionary could include multiple key value pairs and you can access the specfic one. For other secret store only supports one keys, the dictionary will only contain one key value pair where key matches the secret name, namely `my-secret` in this example, and the actual secret value is in the propoerty value. This sample just simply print out all secrets, but please do not log any real secret in your production code  

Given differnt secret store, the metadata string needs to be provided. In order to specify multiple metadata fields, join them by `&`, see the below [Hashicorp Vault](https://github.com/dapr/docs/blob/master/howto/setup-secret-store/hashicorp-vault.md) example. 
```csharp
[DaprSecret("vault", "my-secret",  Metadata = "metadata.version_id=15&metadata.version_stage=AAA"`.
```
However, secrets for this example are only availble in the cluster and currently Dapr does not have a local secret store development experience, so we cannot verify this locally as the other samples. 

# Step 4 - Cleanup

To stop your services from running, simply stop the "dapr run" process. Alternatively, you can spin down each of your services with the Dapr CLI "stop" command. For example, to spin down both services, run these commands in a new command line terminal: 

```bash
dapr stop --app-id functionapp
```


# Deploy Dapr Function App into Kubernetes
Next step, we will show steps to get your Dapr function app running in a Kubernetes cluster.
(To generate your custom container image please see these [instructions](./BuildContainerImage.md))

## Prerequisites
Since our sample does cover multiple Dapr components, here we have a long list of requirements. Please skip any step that is not required for your own function app.  
- Install [kubectl](https://kubernetes.io/docs/tasks/tools/install-kubectl/)
- Install [helm](https://helm.sh/docs/intro/install/) (you can skip this if your function app does not use Kafka bindings)
- A Kubernetes cluster, such as [Minikube](https://github.com/dapr/docs/blob/master/getting-started/cluster/setup-minikube.md), [AKS](https://github.com/dapr/docs/blob/master/getting-started/cluster/setup-aks.md) or [GKE](https://cloud.google.com/kubernetes-engine/)
- A State Store, such as [Redis Store](https://github.com/dapr/docs/blob/master/howto/configure-redis/README.md) for Dapr state store and pub/sub message delivery (you can skip this if your function does not use the aforementioned components)

## Setup Dapr on your Kubernetes Cluster
Once you have a cluster, run `dapr init --kubernetes` to deploy Dapr to it. Please follow this( guide on [how to install Dapr on your kubrtnetes](https://github.com/dapr/docs/blob/master/getting-started/environment-setup.md#installing-dapr-on-a-kubernetes-cluster) via Dapr CLI or Helm. Dapr CLI does not support non-default namespaces and only is recommended for testing purposes.
If you need a non-default namespace or in production environment, Helm has to be used.

```
⌛  Making the jump to hyperspace...
✅  Deploying the Dapr Operator to your cluster...
✅  Success! Dapr has been installed. To verify, run 'kubectl get pods -w' in your terminal
``` 
## Deploy Dapr components
#### [Optional] Configure the State Store
  - Replace the hostname and password in `deploy/redis.yaml`. https://github.com/dapr/samples/tree/master/2.hello-kubernetes#step-2---create-and-configure-a-state-store
  - Run `kubectl apply -f ./deploy/redis.yaml` and observe that your state store was successfully configured!
    ```
    component.dapr.io/statestore configured
    ```
   - Follow [secret management](https://github.com/dapr/docs/tree/master/concepts/secrets) instructions to securely manage your secrets in a production-grade application.
   - More detail can be found in Dapr sample repo [2.hello-kubernetes](https://github.com/dapr/samples/tree/master/2.hello-kubernetes#step-2---create-and-configure-a-state-store)


#### [Optional] Setting up a Kafka in Kubernetes
  - Install Kafka via incubator/kafka helm 
    ```
    helm repo add incubator http://storage.googleapis.com/kubernetes-charts-incubator
    helm repo update
    kubectl create ns kafka
    helm install dapr-kafka incubator/kafka --namespace kafka -f ./kafka-non-persistence.yaml
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
- Follow [secret management](https://github.com/dapr/docs/tree/master/concepts/secrets) instructions to securely manage your secrets in a production-grade application.

#### [Optional] Setting up the Pub/Sub in Kubernetes
  - In this demo, we use Redis Stream (Redis Version 5 and above) to enable pub/sub. Replace the hostname and password in `deploy/redis-pubsub.yaml`. https://github.com/dapr/samples/tree/master/2.hello-kubernetes#step-2---create-and-configure-a-state-store
  - Run `kubectl apply -f .\deploy\redis.yaml` and observe that your state store was successfully configured!
    ```
    component.dapr.io/messagebus configured
    ```
   - See Dapr sample repo [4.pub-sub](https://github.com/dapr/samples/tree/master/4.pub-sub) for more instructions.



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

Now you should have all Dapr components up and running in your kubernetes cluster. Next we will show how to deploy your function app into your kubernetes cluster with the Dapr Side Car.

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
Now similar to what we have done when testing locally, use any of your preferred tool to send HTTP request. Here we use the Rest Client Plugin.

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