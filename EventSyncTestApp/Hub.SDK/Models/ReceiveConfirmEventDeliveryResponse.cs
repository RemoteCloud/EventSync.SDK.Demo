namespace Hub.SDK.Models;

public class ReceiveConfirmEventDeliveryResponse
{
    public bool IsSuccess { get; set; }
    public ReceiveConfirmEventDeliveryError? Error { get; set; }
}
