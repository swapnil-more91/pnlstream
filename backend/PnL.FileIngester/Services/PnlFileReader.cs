using System.Runtime.CompilerServices;
using PnLStream.Common.Contracts;
using PnLStream.Common.Enums;

namespace PnlFileIngester.Services;

/// <summary>
/// Streams PnL records from a flat file line-by-line to support large files (1M+ records)
/// without loading the entire file into memory.
/// Expected format: SourceSystem,PortfolioNumber,PnLAmount (with header row)
/// </summary>
public sealed class PnlFileReader(ILogger<PnlFileReader> logger)
{
    public async IAsyncEnumerable<PnlRecordEvent> ReadAsync(string filePath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"PnL file not found: {filePath}");

        using var reader = new StreamReader(filePath);

        // Skip header row
        await reader.ReadLineAsync(cancellationToken);

        int lineNumber = 1;
        int skipped = 0;

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(cancellationToken);
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (!TryParseLine(line, lineNumber, out var record))
            {
                skipped++;
                continue;
            }

            yield return record!;
        }

        if (skipped > 0)
            logger.LogWarning("Skipped {Count} invalid lines in file {File}", skipped, filePath);
    }

    private bool TryParseLine(string line, int lineNumber, out PnlRecordEvent? record)
    {
        record = null;
        var parts = line.Split(',');

        if (parts.Length != 3)
        {
            logger.LogWarning("Line {Line}: expected 3 columns, got {Count}. Skipping.", lineNumber, parts.Length);
            return false;
        }

        if (!int.TryParse(parts[1].Trim(), out var portfolioNumber))
        {
            logger.LogWarning("Line {Line}: invalid PortfolioNumber '{Value}'. Skipping.", lineNumber, parts[1]);
            return false;
        }

        if (!int.TryParse(parts[2].Trim(), out var pnlAmount))
        {
            logger.LogWarning("Line {Line}: invalid PnLAmount '{Value}'. Skipping.", lineNumber, parts[2]);
            return false;
        }

        record = new PnlRecordEvent(parts[0].Trim(), portfolioNumber, pnlAmount, DataSource.SODFile, DateTime.Now);
        return true;
    }
}
