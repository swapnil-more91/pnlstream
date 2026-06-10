using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PnLStream.Common.Entities;
using PnLStream.Common.Enums;
using PnLStream.Persistence.Db;
using PnLStream.Persistence.Interfaces;

namespace PnLStream.Persistence.Repositories;

public class PnlRepository : IPnlRepository
{
    private readonly PnlDbContext _context;
    private readonly ILogger<PnlRepository> _logger;

    public PnlRepository(PnlDbContext context, ILogger<PnlRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PnlRecord?> SaveAsync(PnlRecord record)
    {
        _logger.LogDebug("Saving PnL record.Portfolio:{PortfolioNumber}, SourceSystem:{SourceSystem}", record.PortfolioNumber,record.SourceSystem);

        try
        {
            var existingRecord = await _context.PnlRecords.FirstOrDefaultAsync(x => x.PortfolioNumber == record.PortfolioNumber 
                                                                                && x.SourceSystem == record.SourceSystem);

            if (existingRecord is null)
            {
                record.CreatedAt = DateTime.UtcNow;
                record.LastUpdatedAt = DateTime.UtcNow;

                await _context.PnlRecords.AddAsync(record);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New PnL record inserted.Portfolio:{PortfolioNumber}, SourceSystem:{SourceSystem}",
                    record.PortfolioNumber,record.SourceSystem);

                return record;
            }

            existingRecord.PnlAmount = record.PnlAmount;
            existingRecord.IsValid = record.IsValid;
            existingRecord.ValidationReasons = record.ValidationReasons;
            existingRecord.DataSource = record.DataSource;
            existingRecord.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated existing PnL record.Portfolio:{PortfolioNumber}, SourceSystem:{SourceSystem}",
                   record.PortfolioNumber, record.SourceSystem);

            return existingRecord;
        }
        catch (Exception ex)
        {
            _logger.LogError(
            ex,
            "Failed to save/update PnL record. Portfolio: {PortfolioNumber}, SourceSystem: {SourceSystem}",
            record.PortfolioNumber,
            record.SourceSystem);

            return null;
        }
    }

    public async Task<(List<PnlRecord> Records, int TotalCount)> GetValidRecordsPagedAsync( int page,
                                                                                            int pageSize,
                                                                                            string? sortBy,
                                                                                            string? sortOrder)
    {
        IQueryable<PnlRecord> query = _context.PnlRecords.AsNoTracking().Where(x => x.IsValid);

        query = (sortBy?.ToLower(), sortOrder?.ToUpper()) switch
        {
            ("sourcesystem", "DESC") => query.OrderByDescending(x => x.SourceSystem),

            ("sourcesystem", _) => query.OrderBy(x => x.SourceSystem),

            ("pnlamount", "DESC") => query.OrderByDescending(x => x.PnlAmount),

            ("pnlamount", _) => query.OrderBy(x => x.PnlAmount),

            ("portfolionumber", "DESC") => query.OrderByDescending(x => x.PortfolioNumber),

            ("portfolionumber", _) => query.OrderBy(x => x.PortfolioNumber),

            _ => query.OrderBy(x => x.Id)
        };

        var totalCount = await query.CountAsync();

        var records = await query
                        .Skip(page * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

        return (records, totalCount);
    }


    public async Task<(List<PnlRecord> Records, int TotalCount)> GetInvalidRecordsPagedAsync( int page,
                                                                                              int pageSize,
                                                                                              string? sortBy,
                                                                                              string? sortOrder)
    {
        IQueryable<PnlRecord> query = _context.PnlRecords.AsNoTracking().Where(x => !x.IsValid);

        query = (sortBy?.ToLower(), sortOrder?.ToUpper()) switch
        {
            ("sourcesystem", "DESC") => query.OrderByDescending(x => x.SourceSystem),

            ("sourcesystem", _) => query.OrderBy(x => x.SourceSystem),

            ("pnlamount", "DESC") => query.OrderByDescending(x => x.PnlAmount),

            ("pnlamount", _) => query.OrderBy(x => x.PnlAmount),

            ("portfolionumber", "DESC") => query.OrderByDescending(x => x.PortfolioNumber),

            ("portfolionumber", _) => query.OrderBy(x => x.PortfolioNumber),

            _ => query.OrderBy(x => x.Id)
        };

        var totalCount = await query.CountAsync();

        var records = await query
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (records, totalCount);
    }

    public async Task<List<PnlRecord>> GetRecordsAsync(ReportType reportType, DateOnly startDate, DateOnly endDate)
    {
        try
        {
            var start = startDate.ToDateTime(TimeOnly.MinValue);

            var end = endDate.ToDateTime(TimeOnly.MaxValue);

            if (reportType == ReportType.ValidPnl)
            {
                return await _context.PnlRecords.AsNoTracking()
                               .Where(x => x.IsValid &&
                                      x.CreatedAt >= start &&
                                      x.CreatedAt <= end)
                               .OrderBy(x => x.CreatedAt).ToListAsync();
            }
            else if (reportType == ReportType.ExcludedPnl)
            {
                return await _context.PnlRecords.AsNoTracking()
                               .Where(x => !x.IsValid &&
                                      x.CreatedAt >= start &&
                                      x.CreatedAt <= end)
                               .OrderBy(x => x.CreatedAt).ToListAsync();
            }
            else
            {
                throw new NotSupportedException($"ReportType :{reportType} not supported.");
            }
        }
        catch 
        {
            throw;
        }
    }
}