using PnLStream.Common.Entities;
using PnLStream.Common.Enums;

namespace PnLStream.Persistence.Interfaces;

public interface IPnlRepository
{
    Task<PnlRecord?> SaveAsync(PnlRecord record);
    Task<(List<PnlRecord> Records, int TotalCount)> GetValidRecordsPagedAsync(int page,int pageSize,string? sortBy, string? sortOrder);
    Task<(List<PnlRecord> Records, int TotalCount)> GetInvalidRecordsPagedAsync(int page, int pageSize, string? sortBy, string? sortOrder);
    Task<List<PnlRecord>> GetRecordsAsync(ReportType ReportType, DateOnly startDate, DateOnly endDate);
}