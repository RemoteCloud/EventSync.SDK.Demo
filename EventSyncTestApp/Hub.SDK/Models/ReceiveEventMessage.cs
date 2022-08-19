namespace Hub.SDK.Models;

public class ReceiveEventMessage
{
    public Guid EventId { get; set; }
    public string? EventContent { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime RegisteredOn { get; set; }
    public bool IsEventFromEdge { get; set; }
    public Guid? SourceLocationId { get; set; }
    public string TenantName { get; set; }
}
