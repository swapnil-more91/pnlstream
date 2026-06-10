using Microsoft.Extensions.Options;
using PnlStream.ValidationEngine.Interfaces;
using PnLStream.Common.Contracts;
using PnLStream.Common.Entities;
using PnLStream.Common.Enums;
using PnLStream.FeedProcessor.Models;
using PnLStream.FeedProcessor.Services;
using PnLStream.Persistence.Interfaces;


namespace PnLStream.FeedProcessor.Workers;

public class FeedProcessorWorker : BackgroundService
{
    private readonly ILogger<FeedProcessorWorker> _logger;
    private readonly IKafkaConsumerService _consumer;
    private readonly IValidationEngine _validationService;
    private readonly IPnlRepository _persistenceService;
    private readonly FeedProcessorOptions _feedProcessorOptions;
    private readonly KafkaOptions _kafkaOptions;
    private readonly DataSource _dataSourceType;
    private readonly IRealtimeNotifier _realtimeNotifier;


    public FeedProcessorWorker(ILogger<FeedProcessorWorker> logger,
                                IKafkaConsumerService consumer,
                                IValidationEngine validationService,
                                IPnlRepository persistenceService,
                                IOptions<FeedProcessorOptions> options,
                                IOptions<KafkaOptions> kafkaOptions,
                                IRealtimeNotifier realtimeNotifier)
    {
        _logger = logger;
        _consumer = consumer;
        _validationService = validationService;
        _persistenceService = persistenceService;
        _feedProcessorOptions = options.Value;
        _kafkaOptions = kafkaOptions.Value;
        _realtimeNotifier = realtimeNotifier;

        _dataSourceType = Enum.Parse<DataSource>(_feedProcessorOptions.DataSource, true);
    }

    //  Background worker responsible for processing incoming PnL feed messages.
    //  It consumes records from Kafka, applies validation rules, persists valid/invalid data,
    //  and broadcasts real-time updates to the reporting dashboard via SignalR.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FeedProcessor started. DataSource={DataSource}, Topic={Topic}", _dataSourceType, _kafkaOptions.Topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var record = await _consumer.ConsumeAsync(stoppingToken);

                if (record == null)
                    continue;

                var entity = ValidateRecord(record);
                var savedRecord = await PersistRecord(entity);
                if(savedRecord != null) await Notify(entity, stoppingToken);

                _logger.LogInformation($"Processed:{entity.SourceSystem}, {entity.PortfolioNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while processing message.");
            }
        }
    }

    private PnlRecord ValidateRecord(PnlRecordEvent record)
    {
        var result = _validationService.Validate(record);
        return new PnlRecord
        {
            SourceSystem = result.PnlRecordRecordEvent.SourceSystem,
            PortfolioNumber = result.PnlRecordRecordEvent.PortfolioNumber,
            PnlAmount = result.PnlRecordRecordEvent.PnlAmount,
            IsValid = result.IsValid,
            ValidationReasons = [.. result.Errors],
            DataSource = _dataSourceType,
            CreatedAt = DateTime.UtcNow
        };
    }

    private Task<PnlRecord?> PersistRecord(PnlRecord pnlRecord)
    {
        return _persistenceService.SaveAsync(pnlRecord);
    }

    private Task Notify(PnlRecord pnlRecord, CancellationToken stoppingToken)
    {
        var notificationDto = new PnlNotificationDto
        {
            SourceSystem = pnlRecord.SourceSystem,
            PortfolioNumber = pnlRecord.PortfolioNumber,
            PnlAmount = pnlRecord.PnlAmount,
            IsValid = pnlRecord.IsValid,
            ValidationReason = string.Join("; ", pnlRecord.ValidationReasons),
            DataSource = pnlRecord.DataSource.ToString(),
            CreatedAt = pnlRecord.CreatedAt,
            LastUpdatedAt = pnlRecord.LastUpdatedAt
        };

        _logger.LogInformation($"Notifying real-time subscribers for Portfolio:{pnlRecord.PortfolioNumber}, SourceSystem:{pnlRecord.SourceSystem}");

        return _realtimeNotifier.PublishAsync(notificationDto, stoppingToken);
    }
}