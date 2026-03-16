var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Santorini_Host>("api");

builder.AddNpmApp("ui", "../Santorini.UI", "dev")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(env: "PORT")
    .WithEnvironment("VITE_API_URL", api.GetEndpoint("http"))
    .WithExternalHttpEndpoints();

builder.Build().Run();
