namespace Hub.SDK.Models;

public class SubscribeToEventsOk
{
    public string? ApplicationName { get; set; }
    public List<string>? TenantNames { get; set; }
}
