using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using PnLStream.Common.Contracts;
using PnLStream.FeedProcessor.Models;

namespace PnLStream.FeedProcessor.Services;

public class SignalRNotifier : IRealtimeNotifier
{
    private readonly HubConnection _connection;
    private readonly ILogger<SignalRNotifier> _logger;

    public SignalRNotifier(
        IOptions<SignalROptions> options,
        ILogger<SignalRNotifier> logger)
    {
        _logger = logger;

        _connection = new HubConnectionBuilder()
            .WithUrl(options.Value.HubUrl)
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task PublishAsync(PnlNotificationDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync( cancellationToken);

                _logger.LogInformation("SignalR connection established");
            }

            await _connection.InvokeAsync("PublishPnlRecord",dto, cancellationToken);

            _logger.LogDebug("SignalR notification sent for Portfolio={Portfolio}", dto.PortfolioNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish SignalR notification");
        }
    }
}