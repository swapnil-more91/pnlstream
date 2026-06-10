using PnLStream.Common.Enums;

namespace PnLStream.Reporting.Api.Models
{
    public class PnlRecordDto
    {
        public long Id { get; set; }

        public string SourceSystem { get; set; } = string.Empty;

        public int PortfolioNumber { get; set; }

        public int PnlAmount { get; set; }

        public bool IsValid { get; set; }

        public string ValidationReasons { get; set; }

        public DataSource DataSource { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime LastUpdatedAt { get; set; }
    }
}
