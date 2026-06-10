using PnlStream.ValidationEngine.Interfaces;
using PnLStream.Common.Contracts;

namespace PnlStream.ValidationEngine;

public class ValidationEngine : IValidationEngine
{
    private readonly IEnumerable<IValidationRule> _rules;

    public ValidationEngine(IEnumerable<IValidationRule> rules)
    {
        _rules = rules;
    }

    public ValidationResultEvent Validate(PnlRecordEvent record)
    {
        var finalResult = new ValidationResultEvent() { PnlRecordRecordEvent = record };

        foreach (var rule in _rules)
        {
            var result = rule.IsValid(record);

            if (!result.IsValid)
            {
                finalResult.Errors.Add(result.reason);
            }
        }

        return finalResult;
    }
}
