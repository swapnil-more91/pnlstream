using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using PnLStream.Common.Contracts;
using PnlFileIngester.Services;

namespace PnlFileIngester;

public class PnlFileWorkerService(
    PnlFileReader fileReader,
    IOptions<PnlFileWorkerOptions> options,
    ILogger<PnlFileWorkerService> logger) : BackgroundService
{

    private const string TopicName = "pnl.file.feed";

    private readonly PnlFileWorkerOptions _opts = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PnL File Worker started. Scheduled run time: {Time}", _opts.RunAtUtc);

        while (!stoppingToken.IsCancellationRequested)
        {
            var nextRun = ComputeNextRun(_opts.RunAtUtc);
            var delay = nextRun - DateTime.UtcNow;
            logger.LogInformation("Next run scheduled at {NextRun:O} UTC (in {Delay:hh\\:mm\\:ss})", nextRun, delay);

            // If the delay is long, sleep most of it in one go but keep a small cushion
            // to allow a short polling loop to align more tightly to the configured time.
            if (delay > TimeSpan.FromSeconds(2))
            {
                var longWait = delay - TimeSpan.FromSeconds(1);
                try
                {
                    await Task.Delay(longWait, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Propagate cancellation so host can stop promptly
                    throw;
                }
            }

            // Final short-poll loop to reduce scheduling jitter
            while (DateTime.UtcNow < nextRun)
            {
                var remaining = nextRun - DateTime.UtcNow;
                var poll = remaining > TimeSpan.FromMilliseconds(200) ? TimeSpan.FromMilliseconds(200) : TimeSpan.FromMilliseconds(20);
                try
                {
                    await Task.Delay(poll, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
            }

            var actualStart = DateTime.UtcNow;
            var driftMs = (actualStart - nextRun).TotalMilliseconds;
            logger.LogInformation("Triggering run. Scheduled: {NextRun:O}, Actual: {Now:O}, DriftMs: {DriftMs}", nextRun, actualStart, driftMs);

            await RunAsync(stoppingToken);
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting PnL file processing at {Now}", DateTime.UtcNow);

        var config = new ProducerConfig { BootstrapServers = _opts.KafkaBootstrapServers };

        using var producer = new ProducerBuilder<Null, string>(config).Build();

        int sent = 0;
        int failed = 0;

        try
        {
            await foreach (var record in fileReader.ReadAsync(_opts.FilePath, cancellationToken))
            {
                var message = BuildMessage(record);

                try
                {
                    await producer.ProduceAsync(
                        TopicName,
                        new Message<Null, string> { Value = message },
                        cancellationToken);

                    sent++;

                    if (sent % 10_000 == 0)
                        logger.LogInformation("Progress: {Sent} records sent", sent);
                }
                catch (ProduceException<Null, string> ex)
                {
                    failed++;
                    logger.LogError(ex, "Failed to produce message for Portfolio {Portfolio}", record.PortfolioNumber);
                }
            }

            producer.Flush(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Processing cancelled after {Sent} records sent", sent);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error during PnL file processing");
        }

        logger.LogInformation("Processing complete. Sent: {Sent}, Failed: {Failed}", sent, failed);
    }

    private static string BuildMessage(PnlRecordEvent record)
    {
        var payload = new
        {
            record.SourceSystem,
            record.PortfolioNumber,
            record.PnlAmount,
            record.DataSource
        };

        return JsonSerializer.Serialize(payload);
    }


    private static DateTime ComputeNextRun(TimeOnly runAtUtc)
    {
        var now = DateTime.UtcNow;
        var nextRun = now.Date.Add(runAtUtc.ToTimeSpan());

        if (nextRun <= now)
            nextRun = nextRun.AddDays(1);

        return nextRun;
    }
}
