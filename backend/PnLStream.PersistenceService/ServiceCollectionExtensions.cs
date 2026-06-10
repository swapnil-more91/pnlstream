using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PnLStream.Persistence.Db;
using PnLStream.Persistence.Interfaces;
using PnLStream.Persistence.Repositories;

namespace PnLStream.Persistence.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PnlDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("PnlStreamDatabase")));

        services.AddScoped<IPnlRepository, PnlRepository>();

        return services;
    }
}