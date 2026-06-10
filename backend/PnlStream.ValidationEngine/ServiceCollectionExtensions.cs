using Microsoft.Extensions.DependencyInjection;
using PnlStream.ValidationEngine.Interfaces;

namespace PnlStream.ValidationEngine;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddValidationEngine(this IServiceCollection services)
    {
        // Register all validation rules and the engine
        services.AddSingleton<IValidationRule, VaalidationRules.ZeroPnlValidationRule>();
        services.AddSingleton<IValidationEngine, ValidationEngine>();

        return services;
    }
}
