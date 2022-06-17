using Maranics.AppStore.SDK;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SubscriberClient;

var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Development.json", false, true)
                .Build();

var apiConfig = new ApiConfiguration();
configuration.Bind(apiConfig);

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton(apiConfig);
        services.AddScoped<ISubscriber, Subscriber>();
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
    ISubscriber publisher = serviceScope.ServiceProvider.GetRequiredService<ISubscriber>();
    await publisher.ExecuteAsync();
    Console.WriteLine();
}
