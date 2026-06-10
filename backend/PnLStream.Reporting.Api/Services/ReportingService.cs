using Microsoft.AspNetCore.DataProtection.Repositories;
using PnLStream.Common.Contracts;
using PnLStream.Common.Entities;
using PnLStream.Common.Enums;
using PnLStream.Persistence.Interfaces;
using PnLStream.Reporting.Api.Models;
using System.Text;

namespace PnLStream.Reporting.Api.Services
{
    public class ReportingService : IReportingService
    {
        private readonly IPnlRepository _repository;

        public ReportingService(IPnlRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResponse<PnlRecordDto>> GetValidRecordsPagedAsync(int page,
                                                                              int pageSize,
                                                                              string? sortBy,
                                                                              string? sortOrder)
        {
            var result = await _repository.GetValidRecordsPagedAsync(page, pageSize, sortBy, sortOrder);

            return new PagedResponse<PnlRecordDto>
            {
                Data = result.Records
                    .Select(x => new PnlRecordDto
                    {
                        SourceSystem = x.SourceSystem,
                        PortfolioNumber = x.PortfolioNumber,
                        PnlAmount = x.PnlAmount,
                        IsValid = x.IsValid,
                        //ValidationReasons = x.ValidationReasons == null ? "" : string.Join('|', x.ValidationReasons),
                        DataSource = x.DataSource,
                        CreatedAt = x.CreatedAt,
                        LastUpdatedAt = x.LastUpdatedAt
                    }).ToList(),

                TotalRecords = result.TotalCount,
                TotalPages = (int)Math.Ceiling(result.TotalCount /(double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                HasNext = page < (int)Math.Ceiling( result.TotalCount / (double)pageSize) - 1,
                HasPrevious = page > 0
            };
        }

        public async Task<PagedResponse<PnlRecordDto>> GetInvalidRecordsPagedAsync( int page,
                                                                                 int pageSize,
                                                                                 string? sortBy,
                                                                                 string? sortOrder)
        {
            var result = await _repository.GetInvalidRecordsPagedAsync(page,pageSize,sortBy,sortOrder);

            var totalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);

            return new PagedResponse<PnlRecordDto>
            {
                Data = result.Records
                    .Select(x => new PnlRecordDto
                    {
                        SourceSystem = x.SourceSystem,
                        PortfolioNumber = x.PortfolioNumber,
                        PnlAmount = x.PnlAmount,
                        IsValid = x.IsValid,
                        ValidationReasons = x.ValidationReasons == null ? "" : string.Join('|', x.ValidationReasons),
                        DataSource = x.DataSource,
                        CreatedAt = x.CreatedAt,
                        LastUpdatedAt = x.LastUpdatedAt
                    }).ToList(),

                TotalRecords = result.TotalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                HasNext = page < totalPages - 1,
                HasPrevious = page > 0
            };
        }

        public async Task<string> GenerateReport(ReportType reportType, DateOnly startDate, DateOnly endDate)
        {
            var records = await _repository.GetRecordsAsync(reportType, startDate, endDate);

            var csv = new StringBuilder();

            if(reportType == ReportType.ValidPnl)
            {
                csv.AppendLine("SourceSystem,PortfolioNumber,PnlAmount,DataSource,CreatedAt");

                foreach (var record in records)
                {
                    csv.AppendLine(
                        $"{record.SourceSystem}," +
                        $"{record.PortfolioNumber}," +
                        $"{record.PnlAmount}," +
                        $"{record.DataSource}," +
                        $"{record.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                }
            }
            else
            {
                csv.AppendLine("SourceSystem,PortfolioNumber,PnlAmount,DataSource,ValidationReasons,CreatedAt");

                foreach (var record in records)
                {
                    csv.AppendLine(
                        $"{record.SourceSystem}," +
                        $"{record.PortfolioNumber}," +
                        $"{record.PnlAmount}," +
                        $"{record.DataSource}," +
                        $"{string.Join('|', record.ValidationReasons)}," +
                        $"{record.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                }
            }

            return csv.ToString();
        }
    }
}
