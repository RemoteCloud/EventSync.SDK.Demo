using System.Text;
using Maranics.AppStore.SDK.Interfaces;
using Newtonsoft.Json;
using PublisherClient;
using PublisherClient.Models;

public class Publisher : IPublisher
{
    public Publisher(IAccessTokenProvider accessTokenProvider, HttpClient httpClient, ApiConfiguration apiConfiguration)
    {
        _accessTokenProvider = accessTokenProvider;
        _httpClient = httpClient;
        _apiConfiguration = apiConfiguration;
    }

    private const string FinishedStatus = "Finished";
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly HttpClient _httpClient;
    private readonly ApiConfiguration _apiConfiguration;

    public async Task Execute()
    {
        string token = await _accessTokenProvider.GetTokenAsync();
        object eventToSend = GetEventObject();
        await SendEventToSyncService(eventToSend, token);
    }

    private object GetEventObject()
    {
        Console.WriteLine("Enter event name: ");
        string? eventName = Console.ReadLine();

        return new { EventName = eventName };
    }

    private async Task SendEventToSyncService(object? eventToSend, string token)
    {
        Log("Press any key to send an event to shore..");
        Console.WriteLine();
        Console.ReadKey();
        while (true)
        {
            PostOutboxEventResponse? result = await PostOutboxEventWithRestApi(eventToSend, token);
            if (result != null && result.Id != Guid.Empty)
            {
                Log("Response from Sync engine: ");
                Log("    Event Id: " + result.Id.ToString());
                Log("    Event status: " + result.Status);
                Console.WriteLine();
                while (true)
                {
                    Log("Retrieve new status of a posted event?");
                    Console.ReadKey();
                    GetOutboxEventStatus? status = await GetOutboxEventStatusWithRestApi(token, result);
                    if (status?.Status == FinishedStatus)
                    {
                        Log("Event was successfully delivered and consumed by subscriber.");
                        Log("Press any key to send another event to sync engine:");
                        Console.ReadKey();
                        break;
                    }
                }
            }
        }
    }

    private async Task<GetOutboxEventStatus?> GetOutboxEventStatusWithRestApi(string token, PostOutboxEventResponse? result)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, _apiConfiguration.EdgeEventsBaseUrl + "/" + result.Id);
        request.Headers.Add("Authorization", $"Bearer {token}");
        HttpResponseMessage statusResponse = await _httpClient.SendAsync(request);
        string? jsonStatusResponse = await statusResponse.Content.ReadAsStringAsync();
        Log(jsonStatusResponse);
        Console.WriteLine();
        var status = JsonConvert.DeserializeObject<GetOutboxEventStatus>(jsonStatusResponse);

        return status;
    }

    private async Task<PostOutboxEventResponse?> PostOutboxEventWithRestApi(object? eventToSend, string token)
    {
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(_apiConfiguration.EdgeEventsBaseUrl!),
            Method = HttpMethod.Post,
            Content = new StringContent(JsonConvert.SerializeObject(eventToSend), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", $"Bearer {token}");
        HttpResponseMessage response = await _httpClient.SendAsync(request);
        string? responseJson = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<PostOutboxEventResponse>(responseJson);
    }

    private static void Log(string content) => Console.WriteLine($"[{DateTime.UtcNow.ToShortDateString()} {DateTime.UtcNow.ToLongTimeString()}] {content}");
}