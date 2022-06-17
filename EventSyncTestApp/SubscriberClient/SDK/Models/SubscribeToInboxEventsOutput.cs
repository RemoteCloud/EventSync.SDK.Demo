namespace SubscriberClient.SDK.Models;

public class SubscribeToInboxEventsOutput
{

    public string? SubscriberName { get; set; }
    public string? ApplicationId { get; set; }
    public List<InboxEventDto>? NewEvents { get; set; }
}
