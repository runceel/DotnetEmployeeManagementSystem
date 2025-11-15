using AttendanceService.Infrastructure.Data;

namespace AttendanceService.API.Extensions;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        // EmployeeServiceから従業員IDを取得してシードデータを生成
        // リトライロジック付き（EmployeeServiceの初期化完了を待つ）
        var httpClientFactory = app.Services.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("EmployeeService");
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        const int maxRetries = 5;
        const int delayMilliseconds = 2000;
        List<EmployeeDto>? employees = null;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                logger.LogInformation("Attempting to fetch employee IDs from EmployeeService (attempt {Attempt}/{MaxRetries})...",
                    attempt, maxRetries);
                
                var response = await httpClient.GetAsync("/api/employees");
                if (response.IsSuccessStatusCode)
                {
                    employees = await response.Content.ReadFromJsonAsync<List<EmployeeDto>>();
                    if (employees != null && employees.Any())
                    {
                        logger.LogInformation("Successfully retrieved {Count} employee IDs from EmployeeService.", employees.Count);
                        break;
                    }
                    else
                    {
                        logger.LogWarning("No employees found in EmployeeService on attempt {Attempt}.", attempt);
                    }
                }
                else
                {
                    logger.LogWarning("Failed to fetch employees from EmployeeService (Status: {StatusCode}) on attempt {Attempt}.",
                        response.StatusCode, attempt);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error occurred while fetching employee IDs from EmployeeService on attempt {Attempt}.", attempt);
            }

            // Wait before retrying (except on the last attempt)
            if (attempt < maxRetries)
            {
                logger.LogInformation("Waiting {Delay}ms before retry...", delayMilliseconds);
                await Task.Delay(delayMilliseconds);
            }
        }

        // Initialize with employee IDs if we got them, otherwise skip seed data
        if (employees != null && employees.Any())
        {
            var employeeIds = employees.Select(e => e.Id).ToList();
            await DbInitializer.InitializeAsync(app.Services, employeeIds);
        }
        else
        {
            logger.LogWarning("Could not retrieve employees from EmployeeService after {MaxRetries} attempts. Skipping seed data generation.",
                maxRetries);
            await DbInitializer.InitializeAsync(app.Services);
        }
    }
}

// EmployeeDto for deserialization
internal record EmployeeDto(Guid Id, string FirstName, string LastName, string Email, DateTime HireDate, Guid DepartmentId, string Position);
