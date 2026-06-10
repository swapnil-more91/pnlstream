using PnLStream.Common.Contracts;

namespace PnLStream.FeedProcessor.Services;

public interface IRealtimeNotifier
{
    Task PublishAsync(PnlNotificationDto dto, CancellationToken cancellationToken);
}