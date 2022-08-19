namespace CloudClient.Models;

public interface IEdgeClient
{
    Task SendAndListenEvents();
}