# Building Dapr Function App Container

If you have updated the sample code to fit your scenario, you need to create new images with your updated code. First you need to install docker on your machine. Next, follow these steps to build your custom container image for your function:

1. Update function app as you see fit!
2. Build Docker image.
   The sample project has a **project reference** for the `Dapr.AzureFunctions.Extension`, instead of a **nuget package reference**.    
   Run docker build command from repo root and specify your image name:
     ```
    docker build -f samples/dotnet-azurefunction/Dockerfile -t mydocker-image .
     ```
    If you're planning on hosting it on docker hub, then it should be
   
    ```
    docker build -f samples/dotnet-azurefunction/Dockerfile -t my-docker-id/mydocker-image .
    ```

    ***Note***
    To build docker image with Nuget packages generated during the build:
    a. Modify the samples .csproj file to use the nuget package from local build (instructions are in the .csproj file)
    b. Copy the latest `.nupkg` file from `$RepoRoot/bin/Debug/nugets` or  `$RepoRoot/bin/Release/nugets` to `samples/dotnet-azurefunction/localNuget` folder.
    c. To build samples with local nuget reference, use `nugetPackageRef.Dockerfile` in samples/dotnet-azurefunction directory.
    d. Run docker build command from samples/dotnet-azurefunction and specify your image name:    
    ```
     docker build -f samples/dotnet-azurefunction/nugetPackageRef.Dockerfile -t my-docker-id .
     ```

4.  Once your image has built you can see it on your machines by running `docker images`. Try run the image in a local container to test the build. Please use `-e` option to specify the app settings. Open a browser to http://localhost:8080, which should show your function app is up and running with `;-)`. You can ignore the storage connection to test this, but you might see exception thrown from your container log complaining storage is not defined.
    ```
    docker run -e AzureWebjobStorage='connection-string` -e StateStoreName=statestore -e KafkaBindingName=sample-topic -p 8080:80 my-docker-id/mydocker-image 
    ```

5.  To publish your docker image to docker hub (or another registry), first login: `docker login`. Then run `docker push my-docker-id/mydocker-image`.
6.  Update your .yaml file to reflect the new image name.
7.  Deploy your updated Dapr enabled app: `kubectl apply -f <YOUR APP NAME>.yaml`.