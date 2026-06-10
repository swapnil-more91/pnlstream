using PnLStream.Common.Enums;

namespace PnLStream.Common.Contracts;

public record PnlRecordEvent
  (
      string SourceSystem,
      int PortfolioNumber,
      int PnlAmount,
      DataSource DataSource,
      DateTime CreatedAt
  );