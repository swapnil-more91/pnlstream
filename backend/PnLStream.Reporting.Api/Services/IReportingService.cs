using PnLStream.Common.Contracts;
using PnLStream.Common.Enums;
using PnLStream.Reporting.Api.Models;

namespace PnLStream.Reporting.Api.Services
{
    public interface IReportingService
    {
        Task<PagedResponse<PnlRecordDto>> GetValidRecordsPagedAsync( int page, int pageSize, string? sortBy, string? sortOrder);
        Task<PagedResponse<PnlRecordDto>> GetInvalidRecordsPagedAsync(int page, int pageSize, string? sortBy, string? sortOrder);
        Task<string> GenerateReport(ReportType ReportType,DateOnly startDate,DateOnly endDate);
    }
}
