using Hub.SDK.Models;

namespace Hub.SDK;

public interface IEventProvider
{
    Task Subscribe(
        Func<ReceiveEventMessage, Task> newEventHandler,
        Func<ReceiveEventStatusMessage, Task> statusHandler,
        Func<SubscribeToEventsResponse, Task> subscriptionHandler);
}
