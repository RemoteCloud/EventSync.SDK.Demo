using Hub.SDK.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Hub.SDK;
public class EventProvider : IEventProvider
{
    private readonly HubConnection _hubConnection;

    public EventProvider(string eventSyncUrl, string? token)
    {
        _hubConnection = new HubConnectionBuilder()
                        .WithUrl(eventSyncUrl, options => options.AccessTokenProvider = () => Task.FromResult(token))
                        .Build();

        _hubConnection.Closed += e =>
        {
            Log("Connection closed with error:" + e?.ToString());
            return Task.CompletedTask;
        };
    }

    public async Task Subscribe(
        Func<ReceiveEventMessage, Task> newEventHandler,
        Func<ReceiveEventStatusMessage, Task> statusHandler,
        Func<SubscribeToEventsResponse, Task> subscriptionHandler)
    {
        _hubConnection.On<SubscribeToEventsResponse>("ReceiveSubscriptionResult", async (response) =>
        {
            await subscriptionHandler(response);
        });

        _hubConnection.On<ReceiveEventMessage>("ReceiveEvent", async (obj) =>
        {
            if (obj.EventId != Guid.Empty)
            {
                await newEventHandler(obj);
                await _hubConnection.InvokeAsync("ConfirmEventDelivery",
                    new ConfirmInboxEventDeliveryRequest
                    {
                        EventId = obj.EventId
                    },
                    new TargetTenantMetadata
                    {
                        TenantName = obj.TenantName
                    });
            }
        });

        _hubConnection.On<ReceiveEventStatusMessage>("ReceiveEventStatus", async (obj) =>
        {
            if (obj.EventId != Guid.Empty)
            {
                await statusHandler(obj);
            }
        });

        _hubConnection.On<ReceiveConfirmEventDeliveryResponse>("ReceiveEventDeliveryConfirmationResult", async (obj) =>
        {
            if (!string.IsNullOrEmpty(obj?.Error?.Message))
            {
                Log("ReceiveEventDeliveryConfirmationResult" + obj.Error.Message);
            }
        });

        await _hubConnection.StartAsync();
        try
        {
            await _hubConnection.InvokeAsync("SubscribeToListenEvents");
        }
        catch (Exception ex)
        {
            Log(ex.ToString());
        }
    }

    private static void Log(string content) => Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] {content}");
}
