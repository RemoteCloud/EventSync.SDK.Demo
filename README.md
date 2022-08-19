# EventSync.SDK.Demo
This repository contains 2 console applications:
- EdgeClient - publishes an event to the Cloud instance and is subscribed to receive events from Cloud;
- CloudClient - publishes an event to the Edge instance and is subscribed to receive events from Edge.

In order to have a working version, please update the configuration file with the required values:
- AppStore Url;
- Client ID;
- Client Secret;
- Destination Location Id for CloudClient;
- EventSync Cloud/Edge base url.

### Publisher
In order to start working with Event Sync, to send and listen events, you need to create subscription first, through SignalR protocol on ```/events``` hub url.
Afterwards you can send a new event so it will be synced to the Cloud/Edge instance, using HTTP POST REST API ```api/events```, in request body you will specify your event in ```application/json``` format. Response will contain id and status of the posted event.
In order to verify the status of the event, you can call HTTP GET REST API ```api/events/{id}```.

**Possible statuses**:
- Acknowledged - message received by sync engine.
- In transit - message was sent to the destination but not yet delivered.
- Delivered - destination accepted an event.
- Finished - message delivery confirmed by the client. All related processes to the event are finished.
- Subscriber is missing - message was delivered to the destination but no active subscription was present to process the event.
- Unknown - Unknown message id / no status information.

### Subscriber
There are several SignalR methods:
- **SubscribeToEvents** - initial method, which is invoked by client to create subscription, afterwards you can listen to new events and send new ones to destination;
- **ReceiveEvent** - through this method subscriber will receive new events live.
- **ReceiveSubscriptionResult** - this method contains result from *SubscribeToListenEvents* method. In response, it will contain client's application name and tenants.If subscription was unsuccessful, it will contain error details;
- **ConfirmEventDelivery** - invoked from client side to server. Client should pass event id in order to mark it as listened. Event sync engine won't retrieve this event to subscriber anymore. Client has responsibility to call this method for each delivered event. Client should supply tenant name to confirm event.


### Event Sync diagram
It is possible to post events from both Edge and Cloud instances and to receive from both Edge and Cloud instances.
Source code of the demo app contains publishing event from Edge to Cloud.

![signalreventsync-drawio drawio](https://user-images.githubusercontent.com/85729931/174261838-f6a50321-d594-4ecf-a5f9-92e2992273fd.png)
