using Confluent.Kafka;
using Microsoft.Extensions.Options;
using PnLStream.Common.Contracts;
using PnLStream.FeedProcessor.Models;
using PnLStream.FeedProcessor.Workers;
using System.Text.Json;

namespace PnLStream.FeedProcessor.Services;

public class KafkaConsumerService : IKafkaConsumerService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<FeedProcessorWorker> _logger;

    public KafkaConsumerService(IOptions<KafkaOptions> options, ILogger<FeedProcessorWorker> logger)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            GroupId = options.Value.ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _consumer.Subscribe(options.Value.Topic);
        
        _logger = logger;
    }

    public Task<PnlRecordEvent?> ConsumeAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = _consumer.Consume(cancellationToken);

            if (result?.Message?.Value == null)
                return Task.FromResult<PnlRecordEvent?>(null);

            _logger.LogInformation("RECV: Consumed message with key: {Key}, value: {Value}", result.Message.Key, result.Message.Value);

            var record = JsonSerializer.Deserialize<PnlRecordEvent>(result.Message.Value);

            return Task.FromResult(record);
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult<PnlRecordEvent?>(null);
        }
    }
}