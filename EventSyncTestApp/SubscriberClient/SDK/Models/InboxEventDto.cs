namespace SubscriberClient.SDK.Models;

public class InboxEventDto
{
    public Guid Id { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedOn { get; set; }
}