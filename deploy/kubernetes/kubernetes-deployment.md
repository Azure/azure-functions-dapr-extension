# Quickstart: Deploying to Kubernetes

You can annotate your function Kubernetes deployments to include the Dapr sidecar.

> IMPORTANT: Port 3001 will only be exposed and listened if a Dapr trigger is defined in the function app.  When using Dapr, the sidecar will wait to receive a response from the defined port before completing instantiation.  This means it is important to NOT define the `dapr.io/port` annotation or `--app-port` unless you have a trigger.  Doing so may lock your application from the Dapr sidecar. Port 3001 does not need to be exposed or defined if only using input and output bindings.

To generate a Dockerfile for your app if you don't already have one, you can run the following command in your function project:
`func init --docker-only`.

The Azure Function core tools can automatically generate for you Kubernetes deployment files based on your local app.  It's worth noting these manifests expect [KEDA](https://keda.sh) will be present to manage scaling, so if not using KEDA you may need to remove the `ScaledObjects` generated, or craft your own deployment YAML file.  We do recommend including KEDA in any cluster that is running Azure Functions containers (with or without Dapr), but it is an optional component to assist with scaling.

An example of a function app deployment for Kubernetes can be [found below](#sample-kubernetes-deployment).

The following command will generate a `deploy.yaml` file for your project:
`func kubernetes deploy --name {container-name} --registry {docker-registry} --dry-run > deploy.yaml`

You can then edit the generated `deploy.yaml` to add the dapr annotations.  You can also craft a deployment manually.

### Azure Storage account requirements

While an Azure Storage account is required to run functions within Azure, it is NOT required for functions that run in Kubernetes or from a Docker container.  The exception to that is functions that leverage a Timer trigger or Event Hub trigger.  In those cases the storage account is used to coordinate leases for instances, so you will need to set an `AzureWebJobsStorage` connection string if using those triggers.  You can set the `AzureWebJobsStorage` value to `none` if not using any of the triggers that require it.

### Sample Kubernetes deployment

```yml
apiVersion: v1
kind: Service
metadata:
  name: my-function
  namespace: default
spec:
  selector:
    app: my-function
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: LoadBalancer
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: my-function
  namespace: default
  labels:
    app: my-function
spec:
  replicas: 1
  selector:
    matchLabels:
      app: my-function
  template:
    metadata:
      labels:
        app: my-function
      annotations:
        dapr.io/enabled: "true"
        dapr.io/id: "functionapp"
        # Only define port of Dapr triggers are included
        dapr.io/port: "3001"
    spec:
      containers:
      - name: my-function
        image: myregistry/my-function
        ports:
        # Port for HTTP triggered functions
        - containerPort: 80
---
```