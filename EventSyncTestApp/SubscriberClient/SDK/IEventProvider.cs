using SubscriberClient.SDK.Models;

namespace SubscriberClient.SDK;

public interface IEventProvider
{
    Task RegisterListenerForNewEvents(Func<ReceiveEventMessage, Task> eventHandler);
    Task SubscribeToEvents();
}
