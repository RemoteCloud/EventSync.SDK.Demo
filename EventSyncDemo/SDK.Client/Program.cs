using EventSyncService.SDK;
using EventSyncService.SDK.Contracts;
using Maranics.AppStore.SDK;
using Maranics.AppStore.SDK.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Development.json", false, true)
                .Build();

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.ConfigureAppStore(configuration);
    })
    .ConfigureLogging((context, configureLogging) => configureLogging.AddConfiguration(configuration.GetSection("Logging")))
    .Build();

await RunClient(host.Services);

await host.RunAsync();


async Task RunClient(IServiceProvider services)
{
    using IServiceScope serviceScope = services.CreateScope();
    IAccessTokenProvider tokenProvider = serviceScope.ServiceProvider.GetRequiredService<IAccessTokenProvider>();
    var subscriber = new EventSyncConnectionBuilder()
        .WithCredentials(opt =>
        {
            opt.AccessTokenProvider = async () => await tokenProvider.GetTokenAsync();
            opt.EventSyncUrl = "https://eventSyncService";
        })
        .WithEventHandler(ProcessNewEvent)
        .WithEventStatusHandler(ProcessStatus)
        .ConfigureLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Debug);
        })
        .Build();

    SubscriptionResult result = await subscriber.Connect();
    Guid eventId = await subscriber.SendEvent(new { EventName = "EventName" }, "nameOfTheEvent", "TenantName", new Guid("06e853d2-4dbe-442e-b17b-2e4e525acea9"));
    Console.ReadLine();
}

Task ProcessNewEvent(EventMessage eventMessage)
{
    Log("New event received from edge:");
    Log("    Event Id: " + eventMessage.EventId.ToString());
    Log("    Event created on: " + eventMessage.CreatedOn.ToString());
    Log("    Event content: " + eventMessage.EventContent);
    Log("    Event registered on: " + eventMessage.RegisteredOn.ToString());
    Log("    Event for tenant: " + eventMessage.TenantName);
    if (eventMessage.IsEventFromEdge)
    {
        Log("    Event's edge location Id: " + eventMessage.SourceLocationId.ToString());
    }
    else
    {
        Log("    Event is from Cloud");
    }

    Console.WriteLine();

    return Task.CompletedTask;
}

Task ProcessStatus(EventStatusMessage publishedEventStatus)
{
    Log("New status:");
    Log("    Event Id: " + publishedEventStatus.EventId.ToString());
    Log("    Event status: " + publishedEventStatus.Status);
    Console.WriteLine();

    return Task.CompletedTask;
}

static void Log(string content) => Console.WriteLine($"[{DateTime.UtcNow.ToShortDateString()} {DateTime.UtcNow.ToLongTimeString()}] {content}");
