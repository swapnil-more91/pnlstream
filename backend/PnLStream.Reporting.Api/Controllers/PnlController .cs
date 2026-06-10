using Microsoft.AspNetCore.Mvc;
using PnLStream.Common.Enums;
using PnLStream.Reporting.Api.Services;
using System.Text;

namespace PnLStream.Reporting.Api.Controllers;


[ApiController]
[Route("api/pnl")]
public class PnlController : ControllerBase
{
    private readonly IReportingService _reportingService;

    public PnlController(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    [HttpGet("valid/paginated")]
    public async Task<IActionResult>GetValidRecordsPaged( [FromQuery] int page,
                                                          [FromQuery] int pageSize,
                                                          [FromQuery] string? sortBy,
                                                          [FromQuery] string? sortOrder)
    {
        var result = await _reportingService.GetValidRecordsPagedAsync(page,pageSize,sortBy,sortOrder);

        return Ok(result);
    }

    [HttpGet("invalid/paginated")]
    public async Task<IActionResult> GetInvalidRecordsPaged( [FromQuery] int page,
                            [FromQuery] int pageSize,
                            [FromQuery] string? sortBy,
                            [FromQuery] string? sortOrder)
    {
        var result = await _reportingService.GetInvalidRecordsPagedAsync(page,pageSize,sortBy,sortOrder);

        return Ok(result);
    }

    [HttpGet("download/validreport")]
    public async Task<IActionResult> DownloadValidReport([FromQuery] DateOnly startDate,[FromQuery] DateOnly endDate)
    {
        var result = await _reportingService.GenerateReport(ReportType.ValidPnl, startDate, endDate);

        var bytes = Encoding.UTF8.GetBytes(result.ToString());
        var fileName = $"Valid-pnl-report-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

        return File(bytes, "text/csv", fileName);
    }

    [HttpGet("download/excludedreport")]
    public async Task<IActionResult> DownloadInvalidReport([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
    {
        var result = await _reportingService.GenerateReport(ReportType.ExcludedPnl, startDate, endDate);

        var csvContent = string.Join(Environment.NewLine, result);
        var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        var fileName = $"Excluded-pnl-report-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

        return File(bytes, "text/csv", fileName);
    }
}