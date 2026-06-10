using PnlStream.ValidationEngine.Interfaces;
using PnLStream.Common.Contracts;

namespace PnlStream.ValidationEngine.VaalidationRules;

internal class ZeroPnlValidationRule : IValidationRule
{
    public (bool IsValid, string reason) IsValid(PnlRecordEvent record)
    {
        if (record.PnlAmount == 0)
        {
            return (false, "PnlAmount is zero");
        }
        
        return (true, string.Empty);
    }
}
