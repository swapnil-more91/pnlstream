using PnLStream.Common.Contracts;

namespace PnlStream.ValidationEngine.Interfaces;

public interface IValidationEngine
{
    ValidationResultEvent Validate(PnlRecordEvent record);
}
