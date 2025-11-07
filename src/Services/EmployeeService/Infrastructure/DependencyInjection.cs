using EmployeeService.Domain.Repositories;
using EmployeeService.Infrastructure.Data;
using EmployeeService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeService.Infrastructure;

/// <summary>
/// Infrastructure層の依存性注入設定
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Infrastructure層のサービスを追加
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        // Remove unsupported Extensions parameter from connection string
        var cleanedConnectionString = RemoveUnsupportedParameters(connectionString);
        
        services.AddDbContext<EmployeeDbContext>(options =>
            options.UseSqlite(cleanedConnectionString));

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();

        return services;
    }

    /// <summary>
    /// Removes unsupported parameters from SQLite connection string
    /// </summary>
    private static string RemoveUnsupportedParameters(string connectionString)
    {
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
        var cleanedParts = parts
            .Where(part => !part.Trim().StartsWith("Extensions=", StringComparison.OrdinalIgnoreCase))
            .Select(part => part.Trim());
        
        return string.Join(";", cleanedParts);
    }
}
