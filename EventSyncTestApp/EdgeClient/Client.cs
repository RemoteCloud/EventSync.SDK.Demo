using System.Net.Http.Headers;
using System.Text;
using EdgeClient.Models;
using Hub.SDK;
using Hub.SDK.Models;
using Maranics.AppStore.SDK.Interfaces;
using Newtonsoft.Json;

namespace EdgeClient
{
    public class Client : IEdgeClient
    {
        private const string _appName = "TestApp";
        private const string _tenantName = "mycompanyname";
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly HttpClient _httpClient;
        private readonly EventSyncConfiguration _configuration;

        public Client(IAccessTokenProvider accessTokenProvider, HttpClient httpClient, EventSyncConfiguration configuration)
        {
            _accessTokenProvider = accessTokenProvider;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task SendAndListenEvents()
        {
            string token = await _accessTokenProvider.GetTokenAsync();
            var eventProvider = new EventProvider(_configuration.EdgeHubUrl!, token);
            await eventProvider.Subscribe(ProcessNewEvent, ProcessStatus, ProcessSubscription);
            await SendEventsToSyncService(token);
        }

        private Task ProcessNewEvent(ReceiveEventMessage eventMessage)
        {
            Log("New event received from edge:");
            Log("    Event Id: " + eventMessage.EventId.ToString());
            Log("    Event created on: " + eventMessage.CreatedOn.ToString());
            Log("    Event content: " + eventMessage.EventContent);
            Log("    Event registered on: " + eventMessage.RegisteredOn.ToString());
            Log("    Event for tenant: " + eventMessage.TenantName);
            if (eventMessage.IsEventFromEdge)
            {
                Log("    Event's edge location Id: " + eventMessage.SourceLocationId.ToString());
            }
            else
            {
                Log("    Event is from Cloud");
            }

            Console.WriteLine();

            return Task.CompletedTask;
        }

        private Task ProcessStatus(ReceiveEventStatusMessage publishedEventStatus)
        {
            Log("New status:");
            Log("    Event Id: " + publishedEventStatus.EventId.ToString());
            Log("    Event status: " + publishedEventStatus.Status);
            Console.WriteLine();

            return Task.CompletedTask;
        }

        private Task ProcessSubscription(SubscribeToEventsResponse subscription)
        {
            Log("Subscription result:");
            if (!string.IsNullOrEmpty(subscription.Result?.ApplicationName))
            {
                Log(string.Format("Application: {0} connected!", subscription.Result.ApplicationName));
                foreach (var tenant in subscription.Result.TenantNames ?? Enumerable.Empty<string>())
                {
                    Log(string.Format("    Tenant {0} is connected to listen events.", tenant));
                }
            }
            else
            {
                Log("    SubscriptionId error: " + subscription.Error?.Message);
            }
            Console.WriteLine();

            return Task.CompletedTask;
        }

        private object GetEventToSend()
        {
            string? eventName = null;
            while (string.IsNullOrWhiteSpace(eventName))
            {
                Log("Please enter event name to send: ");
                eventName = Console.ReadLine();
            }

            return new { EventName = eventName };
        }

        private async Task SendEventsToSyncService(string token)
        {
            while (true)
            {
                object eventToSend = GetEventToSend();
                Log("Press any key to send an event from Edge to Cloud..");
                Console.ReadKey();
                Log("Sending..");
                PostOutboxEventResponse? result = await PostOutboxEventWithRestApi(eventToSend, token);
                if (result != null && result.Id != Guid.Empty)
                {
                    Log("Response from Sync engine: ");
                    Log("    Event Id: " + result.Id.ToString());
                    Log("    Event status: " + result.Status);
                }
            }
        }

        private async Task<PostOutboxEventResponse?> PostOutboxEventWithRestApi(object? eventToSend, string token)
        {
            var request = new StringContent(JsonConvert.SerializeObject(eventToSend), Encoding.UTF8, "application/json");
            if (token != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            request.Headers.Add("ApplicationName", _appName);
            request.Headers.Add("TenantName", _tenantName);
            HttpResponseMessage response = await _httpClient.PostAsync(_configuration.EventsBaseRestUrl, request);
            string? responseJson = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<PostOutboxEventResponse>(responseJson);
        }

        private static void Log(string content) => Console.WriteLine($"[{DateTime.UtcNow.ToShortDateString()} {DateTime.UtcNow.ToLongTimeString()}] {content}");
    }
}
