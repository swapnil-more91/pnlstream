using PnLStream.Common.Contracts;

namespace PnLStream.FeedProcessor.Services;

public interface IKafkaConsumerService
{
    Task<PnlRecordEvent?> ConsumeAsync(CancellationToken cancellationToken);
}