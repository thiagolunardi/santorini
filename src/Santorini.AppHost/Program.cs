using Aspire.Hosting.Azure;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var location = builder.AddParameter("location", "germanywestcentral");
var resourceGroup = builder.AddParameter("resourceGroup", "santorini");

builder.AddAzureEnvironment()
    .WithLocation(location)
    .WithResourceGroup(resourceGroup);

builder.AddAzureContainerAppEnvironment("env");

var webapi = builder.AddProject<Santorini_Host>("webapi");

if (builder.ExecutionContext.IsPublishMode)
{
    builder.AddDockerfile("webapp", "../Santorini.UI")
        .WithReference(webapi)
        .WaitFor(webapi)
        .WithHttpEndpoint(targetPort: 8080)
        .WithEnvironment("BACKEND_URL", webapi.GetEndpoint("https"))
        .WithExternalHttpEndpoints();
}
else
{
    builder.AddNpmApp("webapp", "../Santorini.UI", "dev")
        .WithReference(webapi)
        .WaitFor(webapi)
        .WithHttpEndpoint(env: "PORT")
        .WithEnvironment("VITE_API_URL", webapi.GetEndpoint("http"))
        .WithExternalHttpEndpoints();
}

builder.Build().Run();
