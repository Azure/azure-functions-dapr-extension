# Quickstart: Deploying to Local

## Installing the extension

### .NET Functions

Run the following command from the path where your csproj is located  to add the Nuget package to your Azure Function project

**Isolated Worker Process:**

```
dotnet add package Microsoft.Azure.Functions.Worker.Extensions.Dapr --prerelease
```

**In-process**

```
dotnet add package Microsoft.Azure.WebJobs.Extensions.Dapr --prerelease
```

### Non-.NET Functions

Since this extension is in Preview, you need to add the preview extension by adding or replacing the following code in your host.json file: 

```
{
  "version": "2.0",
  "extensionBundle": {
    "id": "Microsoft.Azure.Functions.ExtensionBundle.Preview",
    "version": "[4.*, 5.0.0)"
  }
}
```

## Using Dapr Extension local build

The samples in this repo (other than the quickstart) are set up to run using a local build of the extension.

You can use a development build of the extension for any function by:

- Referencing the Microsoft.Azure.WebJobs.Extensions.Dapr project in your .NET function
- Publishing the extension to the `bin/` directory of your non-.NET function

Example for non-.NET function:

```sh
dotnet publish /path/to/Microsoft.Azure.WebJobs.Extensions.Dapr -o bin/
```

## Running and debugging an app

Normally when debugging an Azure Function you use the `func` command line tool to start up the function app process and trigger your code.  When debugging or running an Azure Function that will leverage Dapr, you need to use `dapr` alongside `func` so both processes are running.

So when running a Dapr app locally using the default ports, you would leverage the `dapr` CLI to start the `func` CLI.

### If no Dapr triggers are in the app
```sh
dapr run --app-id functionA --dapr-http-port 3501 -- func host start --no-build
```

### If Dapr triggers are in the app
```sh
dapr run --app-id functionA --app-port 3001 --dapr-http-port 3501 -- func host start --no-build
```
