using Maranics.AppStore.SDK.Interfaces;
using SubscriberClient;
using SubscriberClient.SDK;
using SubscriberClient.SDK.Models;

public class Subscriber : ISubscriber
{
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly ApiConfiguration _apiConfiguration;

    public Subscriber(IAccessTokenProvider accessTokenProvider, ApiConfiguration apiConfiguration)
    {
        _accessTokenProvider = accessTokenProvider;
        _apiConfiguration = apiConfiguration;
    }

    public async Task ExecuteAsync()
    {
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, a) =>
        {
            a.Cancel = true;
            cts.Cancel();
        };
        string token = await _accessTokenProvider.GetTokenAsync();
        var eventProvider = new EventProvider(_apiConfiguration.EventSyncCloudSubscribeUrl!, token);
        await eventProvider.RegisterListenerForNewEvents(ProcessNewEvent);
        Log("Starting connection. Press Ctrl-C to close.");
        Log("Press to subscribe: ");
        Console.WriteLine();
        Console.ReadKey();
        await eventProvider.SubscribeToEvents();
        while (!cts.IsCancellationRequested)
        {

        }
    }

    private Task ProcessNewEvent(ReceiveEventMessage eventMessage)
    {
        Log("New event received from edge:");
        Log("    Event Id: " + eventMessage.EventId.ToString());
        Log("    Event created on: " + eventMessage.CreatedOn.ToString());
        Console.WriteLine();

        return Task.CompletedTask;
    }

    private static void Log(string content) => Console.WriteLine($"[{DateTime.UtcNow.ToShortDateString()} {DateTime.UtcNow.ToLongTimeString()}] {content}");
}
