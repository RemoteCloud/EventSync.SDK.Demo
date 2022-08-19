namespace Hub.SDK.Models;

public class ReceiveEventStatusMessage
{
    public Guid EventId { get; set; }
    public string Status { get; set; }
}
