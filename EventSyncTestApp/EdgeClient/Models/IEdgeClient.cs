namespace EdgeClient.Models;

public interface IEdgeClient
{
    Task SendAndListenEvents();
}