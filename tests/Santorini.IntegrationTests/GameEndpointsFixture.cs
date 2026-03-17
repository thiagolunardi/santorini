using Aspire.Hosting;
using Aspire.Hosting.Testing;
using JetBrains.Annotations;
using Projects;
using Xunit;

namespace Santorini.IntegrationTests;

[UsedImplicitly]
public class GameEndpointsFixture : IAsyncLifetime
{
    private DistributedApplication App { get; set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Santorini_AppHost>();
        App = await appHost.BuildAsync();
        await App.StartAsync();
        Client = App.CreateHttpClient("webapi");
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await App.DisposeAsync();
    }
}
