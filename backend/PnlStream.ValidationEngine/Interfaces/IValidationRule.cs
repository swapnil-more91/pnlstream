using PnLStream.Common.Contracts;

namespace PnlStream.ValidationEngine.Interfaces;

public interface IValidationRule
{
    (bool IsValid, string reason) IsValid(PnlRecordEvent record);
}
