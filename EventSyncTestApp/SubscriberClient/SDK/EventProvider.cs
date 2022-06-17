using Microsoft.AspNetCore.SignalR.Client;
using SubscriberClient.SDK.Models;

namespace SubscriberClient.SDK;
public class EventProvider : IEventProvider
{
    private readonly HubConnection _hubConnection;

    public EventProvider(string eventSyncUrl, string token)
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

    public Task RegisterListenerForNewEvents(Func<ReceiveEventMessage, Task> eventHandler)
    {
        _hubConnection.On<ReceiveEventMessage>("ReceiveEvent", async (obj) =>
        {
            if (obj.EventId != Guid.Empty)
            {
                await eventHandler(obj);
                await _hubConnection.InvokeAsync("ConfirmEventDelivery", obj.EventId);
            }
        });

        _hubConnection.On<SubscribeToListenEventsResponse>("ReceiveSubscriptionResult", async (response) =>
        {
            if (response?.Result?.NewEvents?.Count > 0)
            {
                foreach (InboxEventDto newEvent in response.Result.NewEvents)
                {
                    await eventHandler(ToReceiveEventMessage(newEvent, response.Result.SubscriberName));
                    await _hubConnection.InvokeAsync("ConfirmEventDelivery", newEvent.Id);
                }
            }
        });

        return Task.CompletedTask;
    }

    private static ReceiveEventMessage ToReceiveEventMessage(InboxEventDto inboxEvent, string? groupName)
    {
        return new ReceiveEventMessage
        {
            CreatedOn = inboxEvent.CreatedOn,
            EventContent = inboxEvent.Content,
            EventId = inboxEvent.Id,
            GroupName = groupName
        };
    }

    public async Task SubscribeToEvents()
    {
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

    private static void Log(string content) => Log($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] {content}");
}
