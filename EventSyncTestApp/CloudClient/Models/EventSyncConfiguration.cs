namespace CloudClient.Models;
public class EventSyncConfiguration
{
    public string? EventsBaseRestUrl { get; set; }
    public string? EdgeHubUrl { get; set; }
    public Guid DestinationLocationId { get; set; }
}
