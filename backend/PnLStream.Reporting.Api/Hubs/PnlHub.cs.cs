using Microsoft.AspNetCore.SignalR;
using PnLStream.Common.Contracts;

namespace PnLStream.Reporting.Api.Hubs;

public class PnlHub : Hub
{
    public async Task PublishPnlRecord(PnlNotificationDto notificationDto)
    {
        var eventName = notificationDto.IsValid ? "ValidRecordReceived": "InvalidRecordReceived";

        await Clients.All.SendAsync(eventName, notificationDto);
    }
}