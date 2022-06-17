namespace SubscriberClient.SDK.Models;

public class SubscribeToListenEventsResponse
{
    public SubscribeToInboxEventsOutput? Result { get; set; }
    public SubscribeToInboxEventsError? Error { get; set; }
}
