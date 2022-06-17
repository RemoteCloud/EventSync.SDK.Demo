using Maranics.AppStore.SDK;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PublisherClient;
using PublisherClient.Models;

var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Development.json", false, true)
                .Build();

var apiConfig = new ApiConfiguration();
configuration.Bind(apiConfig);

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton(apiConfig);
        services.AddHttpClient<IPublisher, Publisher>();
        services.ConfigureAppStore(configuration);
    })
    .ConfigureLogging((context, config)
        => config.AddConfiguration(configuration.GetSection("Logging")))
    .Build();


await RunPublisher(host.Services);
await host.RunAsync();

async Task RunPublisher(IServiceProvider services)
{
    using IServiceScope serviceScope = services.CreateScope();
    IPublisher publisher = serviceScope.ServiceProvider.GetRequiredService<IPublisher>();
    await publisher.Execute();
    Console.WriteLine();
}