namespace SubscriberClient.SDK.Models;

public class ReceiveEventMessage
{
    public Guid EventId { get; set; }
    public string? EventContent { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? GroupName { get; set; }
}
