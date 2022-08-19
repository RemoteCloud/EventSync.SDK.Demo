using EdgeClient;
using EdgeClient.Models;
using Maranics.AppStore.SDK;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Development.json", false, true)
                .Build();

var eConfig = new EventSyncConfiguration();
configuration.Bind("EventSync", eConfig);
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton(eConfig);
        services.AddHttpClient<IEdgeClient, Client>();
        services.ConfigureAppStore(configuration);
    })
    .ConfigureLogging((context, config)
        => config.AddConfiguration(configuration.GetSection("Logging")))
    .Build();

await RunClient(host.Services);

async Task RunClient(IServiceProvider services)
{
    using IServiceScope serviceScope = services.CreateScope();
    IEdgeClient client = serviceScope.ServiceProvider.GetRequiredService<IEdgeClient>();
    await client.SendAndListenEvents();
    Console.WriteLine();
}
