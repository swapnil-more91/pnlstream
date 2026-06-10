namespace PnLStream.Common.Contracts;

public class PnlNotificationDto
{
    public string SourceSystem { get; set; } = string.Empty;

    public int PortfolioNumber { get; set; }

    public int PnlAmount { get; set; }

    public bool IsValid { get; set; }

    public string? ValidationReason { get; set; }

    public string DataSource { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime LastUpdatedAt { get; set; }
}